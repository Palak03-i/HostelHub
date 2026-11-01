using Hostel_hub.Models;
using Hostel_hub.Services;
using Hostel_hub.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace HostelHub.Tests;

public sealed class ServiceIntegrationTests : IDisposable
{
    private readonly TestDatabase data = new();
    private RoomService Rooms => new(data.Db,NullLogger<RoomService>.Instance);
    private RoomChangeService Changes => new(data.Db,NullLogger<RoomChangeService>.Instance);
    private ComplaintService Complaints => new(data.Db,NullLogger<ComplaintService>.Instance);
    private FeedbackService Feedback => new(data.Db,NullLogger<FeedbackService>.Instance);
    private MealSelectionService Meals => new(data.Db,new ConfigurationBuilder().Build(),NullLogger<MealSelectionService>.Instance);
    private MessMenuService Menus => new(data.Db,NullLogger<MessMenuService>.Instance);
    public void Dispose()=>data.Dispose();

    [Fact] public async Task Room_CRUD_rejects_duplicate_and_invalid_capacity()
    {
        Assert.True((await Rooms.CreateRoomAsync(1," 103 ",3)).Success);
        var room=await data.Db.Rooms.SingleAsync(r=>r.RoomNumber=="103");
        Assert.False((await Rooms.CreateRoomAsync(1,"103",3)).Success);
        Assert.False((await Rooms.UpdateRoomAsync(room.RoomId,"103",21)).Success);
        Assert.True((await Rooms.UpdateRoomAsync(room.RoomId,"104",4)).Success);
        Assert.True((await Rooms.DeleteRoomAsync(room.RoomId)).Success);
        Assert.False(await data.Db.Rooms.AnyAsync(r=>r.RoomId==room.RoomId));
    }
    [Fact] public async Task Allocation_rejects_cross_hostel_and_updates_occupancy_once()
    {
        Assert.False((await Rooms.AllocateStudentAsync(2,2)).Success);
        Assert.True((await Rooms.AllocateStudentAsync(2,3)).Success);
        Assert.False((await Rooms.AllocateStudentAsync(2,3)).Success);
        Assert.Equal(1,(await data.Db.Rooms.FindAsync(3))!.CurrentOccupancy);
        Assert.Equal(3,(await data.Db.Students.FindAsync(2))!.RoomId);
    }
    [Fact] public async Task Occupied_room_cannot_be_deleted_or_shrunk()
    {
        Assert.False((await Rooms.DeleteRoomAsync(1)).Success);
        (await data.Db.Rooms.FindAsync(1))!.CurrentOccupancy=2;
        await data.Db.SaveChangesAsync();
        Assert.False((await Rooms.UpdateRoomAsync(1,"101",1)).Success);
    }
    [Fact] public async Task Room_change_approval_moves_student_and_cannot_repeat()
    {
        Assert.True((await Changes.CreateRequestAsync(1,2,"Need quieter room")).Success);
        Assert.False((await Changes.CreateRequestAsync(1,2,"Duplicate")).Success);
        int id=(await data.Db.RoomChangeRequests.SingleAsync()).RoomChangeRequestId;
        Assert.True((await Changes.ApproveRequestAsync(id)).Success);
        Assert.False((await Changes.ApproveRequestAsync(id)).Success);
        Assert.Equal(0,(await data.Db.Rooms.FindAsync(1))!.CurrentOccupancy);
        Assert.Equal(1,(await data.Db.Rooms.FindAsync(2))!.CurrentOccupancy);
        Assert.Equal(2,(await data.Db.Students.FindAsync(1))!.RoomId);
        Assert.False((await Rooms.DeleteRoomAsync(1)).Success); // historical FK
    }
    [Fact] public async Task Room_change_rejects_other_hostel_and_same_room()
    {
        Assert.False((await Changes.CreateRequestAsync(1,3,"Other hostel")).Success);
        Assert.False((await Changes.CreateRequestAsync(1,1,"Same room")).Success);
        Assert.Empty(await data.Db.RoomChangeRequests.ToListAsync());
    }
    [Fact] public async Task Approval_rechecks_capacity_and_rejection_is_final()
    {
        await Changes.CreateRequestAsync(1,2,"Test");
        var request=await data.Db.RoomChangeRequests.SingleAsync();
        (await data.Db.Rooms.FindAsync(2))!.CurrentOccupancy=2;
        await data.Db.SaveChangesAsync();
        Assert.False((await Changes.ApproveRequestAsync(request.RoomChangeRequestId)).Success);
        Assert.Equal(1,(await data.Db.Students.FindAsync(1))!.RoomId);
        Assert.True((await Changes.RejectRequestAsync(request.RoomChangeRequestId)).Success);
        Assert.False((await Changes.ApproveRequestAsync(request.RoomChangeRequestId)).Success);
    }
    [Fact] public async Task Stale_room_change_cannot_decrement_old_occupancy()
    {
        await Changes.CreateRequestAsync(1,2,"Test");
        var request=await data.Db.RoomChangeRequests.SingleAsync();
        (await data.Db.Students.FindAsync(1))!.RoomId=null;
        await data.Db.SaveChangesAsync();
        Assert.False((await Changes.ApproveRequestAsync(request.RoomChangeRequestId)).Success);
        Assert.Equal(1,(await data.Db.Rooms.FindAsync(1))!.CurrentOccupancy);
    }
    [Fact] public async Task Complaint_lifecycle_records_history_and_accepts_one_feedback()
    {
        Assert.False((await Complaints.UpdateStatusAsync(1,ComplaintStatus.Resolved,"Staff",null)).Success);
        Assert.True((await Complaints.AssignStaffAsync(1,1,"Warden")).Success);
        Assert.True((await Complaints.UpdateStatusAsync(1,ComplaintStatus.InProgress,"Staff",null)).Success);
        Assert.True((await Complaints.UpdateStatusAsync(1,ComplaintStatus.Resolved,"Staff","Fixed")).Success);
        Assert.NotNull((await data.Db.Complaints.FindAsync(1))!.ResolvedDate);
        Assert.Equal(3,(await Complaints.GetHistoryAsync(1)).Count);
        Assert.True((await Feedback.SubmitFeedbackAsync(1,FeedbackSource.Complaint,1,5,"Thanks")).Success);
        Assert.False((await Feedback.SubmitFeedbackAsync(1,FeedbackSource.Complaint,1,4,null)).Success);
    }
    [Fact] public async Task Complaint_assignment_rejects_inactive_and_other_hostel_staff()
    {
        var staff=(await data.Db.MaintenanceStaff.FindAsync(1))!;
        staff.IsActive=false; await data.Db.SaveChangesAsync();
        Assert.False((await Complaints.AssignStaffAsync(1,1,"Warden")).Success);
        staff.IsActive=true;staff.HostelId=2; await data.Db.SaveChangesAsync();
        Assert.False((await Complaints.AssignStaffAsync(1,1,"Warden")).Success);
    }
    [Fact] public async Task Complaint_edit_and_filters_preserve_ownership()
    {
        Assert.False((await Complaints.UpdateComplaintAsync(1,2,"Stolen","Bad",ComplaintCategory.Other)).Success);
        Assert.True((await Complaints.UpdateComplaintAsync(1,1,"New light","Details",ComplaintCategory.Electrical)).Success);
        Assert.Single(await Complaints.GetAllComplaintsAsync(1,new ComplaintFilterViewModel {Search="#1"}));
        Assert.Empty(await Complaints.GetAllComplaintsAsync(2,new ComplaintFilterViewModel()));
        Assert.Empty(await Complaints.GetAllComplaintsAsync(1,new ComplaintFilterViewModel {FromDate=DateTime.Today,ToDate=DateTime.Today.AddDays(-1)}));
    }
    [Fact] public async Task Feedback_rejects_unresolved_other_owner_and_bad_rating()
    {
        Assert.False((await Feedback.SubmitFeedbackAsync(1,FeedbackSource.Complaint,1,5,null)).Success);
        Assert.False((await Feedback.SubmitFeedbackAsync(2,FeedbackSource.Complaint,1,5,null)).Success);
        Assert.False((await Feedback.SubmitFeedbackAsync(1,FeedbackSource.General,null,6,null)).Success);
        Assert.False((await Feedback.SubmitFeedbackAsync(1,(FeedbackSource)99,null,5,null)).Success);
        Assert.True((await Feedback.SubmitFeedbackAsync(1,FeedbackSource.General,1,4,"Good")).Success);
        Assert.Null((await data.Db.Feedbacks.SingleAsync()).ComplaintId);
    }
    [Fact] public async Task Meal_selection_upserts_and_rejects_other_hostel_and_bad_enum()
    {
        Assert.True((await Meals.SelectMealAsync(1,1,MealType.Lunch,MealSelectionStatus.Take)).Success);
        Assert.True((await Meals.SelectMealAsync(1,1,MealType.Lunch,MealSelectionStatus.Skip)).Success);
        Assert.Equal(MealSelectionStatus.Skip,(await data.Db.MealSelections.SingleAsync()).Status);
        Assert.False((await Meals.SelectMealAsync(2,1,MealType.Lunch,MealSelectionStatus.Take)).Success);
        Assert.False((await Meals.SelectMealAsync(1,1,MealType.Lunch,(MealSelectionStatus)99)).Success);
        var report=await Meals.GetParticipationReportAsync(1);
        Assert.Equal(1,report!.TotalStudents);
        Assert.Equal(1,report.Rows.Single(r=>r.MealType==MealType.Lunch).Skipping);
    }
    [Fact] public async Task Unavailable_and_past_meals_cannot_be_selected()
    {
        var menu=(await data.Db.MessMenus.FindAsync(1))!; menu.IsLunchAvailable=false;
        await data.Db.SaveChangesAsync();
        Assert.False((await Meals.SelectMealAsync(1,1,MealType.Lunch,MealSelectionStatus.Take)).Success);
        menu.MenuDate=DateOnly.FromDateTime(DateTime.Today.AddDays(-1));await data.Db.SaveChangesAsync();
        Assert.False((await Meals.SelectMealAsync(1,1,MealType.Dinner,MealSelectionStatus.Take)).Success);
    }
    [Fact] public async Task Menu_CRUD_and_cleanup_preserve_today_and_remove_old_dependencies()
    {
        var date=DateOnly.FromDateTime(DateTime.Today);
        var menu=new MessMenu {HostelId=1,CreatedByUserId=2,MenuDate=date};
        Assert.True((await Menus.CreateMenuAsync(menu,new(){new MessMenuItem {MealType=MealType.Lunch,ItemName="Rice"}})).Success);
        Assert.False((await Menus.CreateMenuAsync(new MessMenu {HostelId=1,MenuDate=date},new())).Success);
        Assert.True((await Menus.UpdateMenuAsync(menu.MessMenuId,new MessMenu(),new(){new MessMenuItem {MealType=MealType.Dinner,ItemName="Soup"}})).Success);
        Assert.Equal("Soup",(await Menus.GetMenuByIdAsync(menu.MessMenuId))!.Items.Single().ItemName);
        await Meals.SelectMealAsync(1,1,MealType.Lunch,MealSelectionStatus.Take);
        Assert.False((await Menus.DeleteMenuAsync(1)).Success);
        (await data.Db.MessMenus.FindAsync(1))!.MenuDate=date.AddDays(-1);await data.Db.SaveChangesAsync();
        Assert.Equal(1,await Menus.DeletePastMenusAsync());
        Assert.NotNull(await data.Db.MessMenus.FindAsync(menu.MessMenuId));
        Assert.Empty(await data.Db.MealSelections.ToListAsync());
        Assert.True((await Menus.DeleteMenuAsync(menu.MessMenuId)).Success);
    }
    [Fact] public async Task Admin_identity_requires_real_admin_without_warden_record()
    {
        var context=new WardenContext(data.Db);
        Assert.True(await context.IsSuperAdminAsync(1));
        foreach(var id in new[]{2,3,5,6,999}) Assert.False(await context.IsSuperAdminAsync(id));
        Assert.Equal(1,await context.GetScopedHostelIdAsync(2));
    }
    [Fact] public async Task Student_profile_and_initial_hostel_assignment()
    {
        var service=new StudentService(data.Db,NullLogger<StudentService>.Instance);
        Assert.True(await service.UpdateProfileAsync(3,"Updated","9876543210"));
        Assert.Equal("Updated",(await service.GetProfileByUserIdAsync(3))!.FullName);
        Assert.False(await service.AssignHostelAsync(1,2));
        (await data.Db.Students.FindAsync(2))!.HostelId=null;await data.Db.SaveChangesAsync();
        Assert.False(await service.AssignHostelAsync(2,999));
        Assert.True(await service.AssignHostelAsync(2,1));
        Assert.Single(await service.GetUnallocatedStudentsAsync(1));
    }
    [Fact] public async Task Hostel_CRUD_and_related_data_deletion_guard()
    {
        var service=new HostelService(data.Db,NullLogger<HostelService>.Instance);
        var hostel=new Hostel {Name="Empty",Type=HostelType.Mixed};await service.CreateHostelAsync(hostel);
        Assert.True(await service.UpdateHostelAsync(hostel.HostelId,"Renamed",HostelType.Boys));
        Assert.True((await service.DeleteHostelAsync(hostel.HostelId)).Success);
        Assert.False((await service.DeleteHostelAsync(1)).Success);
    }
    [Fact] public async Task Staff_creation_updates_and_activation()
    {
        var service=new MaintenanceStaffService(data.Db,new AuthService(),NullLogger<MaintenanceStaffService>.Instance);
        Assert.True((await service.CreateAsync("new@test.local","Test-only-123!","New Staff",null,StaffSpecialization.General,1)).Success);
        Assert.False((await service.CreateAsync("new@test.local","Test-only-123!","Duplicate",null,StaffSpecialization.General,1)).Success);
        Assert.True(await service.UpdateAsync(1,"Updated Staff",null,StaffSpecialization.Electrical));
        Assert.True((await service.DeactivateStaffAsync(1)).Success);
        Assert.False(await service.IsStaffActiveAsync(1));
        Assert.True((await service.ReactivateStaffAsync(1)).Success);
        Assert.True(await service.IsStaffActiveAsync(1));
    }
    [Fact] public async Task Announcements_respect_dates_scope_and_deactivation()
    {
        var service=new AnnouncementService(data.Db,NullLogger<AnnouncementService>.Instance);
        var today=DateOnly.FromDateTime(DateTime.Today);
        Announcement New(int? hostel)=>new(){Title="Notice",Content="Details",PostedByUserId=1,HostelId=hostel,PublishDate=today,ExpiryDate=today.AddDays(1)};
        var global=New(null);Assert.True((await service.CreateAnnouncementAsync(global)).Success);
        Assert.True((await service.CreateAnnouncementAsync(New(2))).Success);
        Assert.Single(await service.GetActiveAnnouncementsAsync(1));
        var invalid=New(1); invalid.ExpiryDate=today.AddDays(-1);
        Assert.False((await service.CreateAnnouncementAsync(invalid)).Success);
        var updated=New(null);updated.Title="Updated";
        Assert.True((await service.UpdateAnnouncementAsync(global.AnnouncementId,updated)).Success);
        Assert.True((await service.DeactivateAnnouncementAsync(global.AnnouncementId)).Success);
        Assert.Empty(await service.GetActiveAnnouncementsAsync(1));
    }
    [Fact] public async Task Dashboard_counts_are_hostel_scoped()
    {
        var service=new AdminDashboardService(data.Db);
        var scoped=await service.GetDashboardAsync(1);
        Assert.Equal(1,scoped.TotalStudents);Assert.Equal(2,scoped.TotalRooms);Assert.Equal(1,scoped.TotalComplaints);
        var all=await service.GetDashboardAsync(null);
        Assert.Equal(2,all.TotalStudents);Assert.Equal(3,all.TotalRooms);
    }
}
