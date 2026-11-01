using System.Net;
using System.Text.RegularExpressions;
using Hostel_hub.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HostelHub.Tests;

public sealed class HostelFactory : WebApplicationFactory<Program>
{
    private readonly TestDatabase data = new();
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services => {
            services.RemoveAll<ApplicationDbContext>();
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
            services.AddScoped<ApplicationDbContext>(_ => new TestDbContext(
                new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(data.Connection).Options));
        });
    }
    public HttpClient NewClient() => CreateClient(new WebApplicationFactoryClientOptions {
        BaseAddress=new Uri("https://localhost"), AllowAutoRedirect=false, HandleCookies=true });
    protected override void Dispose(bool disposing) { base.Dispose(disposing); if(disposing) data.Dispose(); }
}

public sealed class WebIntegrationTests
{
    private static async Task<string> Token(HttpClient client,string path)
    {
        var response=await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK,response.StatusCode);
        var html=await response.Content.ReadAsStringAsync();
        var match=Regex.Match(html,"name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
        Assert.True(match.Success,"Expected an antiforgery token on "+path);
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }
    private static async Task Login(HttpClient client,string email)
    {
        var token=await Token(client,"/Account/Login");
        var response=await client.PostAsync("/Account/Login",new FormUrlEncodedContent(new Dictionary<string,string>{
            ["Email"]=email,["Password"]="Test-only-123!",["__RequestVerificationToken"]=token }));
        Assert.Equal(HttpStatusCode.Redirect,response.StatusCode);
        Assert.Contains(response.Headers.GetValues("Set-Cookie"),c=>c.Contains(".AspNetCore.Cookies="));
    }
    [Theory]
    [InlineData("/")] [InlineData("/Account/Login")] [InlineData("/Account/Register")] [InlineData("/Home/Privacy")]
    public async Task Public_pages_render(string path)
    {
        using var app=new HostelFactory();using var client=app.NewClient();
        Assert.Equal(HttpStatusCode.OK,(await client.GetAsync(path)).StatusCode);
    }
    [Theory]
    [InlineData("/Student/Dashboard")] [InlineData("/Admin/Dashboard")] [InlineData("/Maintenance/Dashboard")]
    public async Task Anonymous_users_are_sent_to_login(string path)
    {
        using var app=new HostelFactory();using var client=app.NewClient();
        var response=await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.Redirect,response.StatusCode);
        Assert.Contains("/Account/Login",response.Headers.Location!.ToString());
    }
    [Fact] public async Task Posts_without_antiforgery_token_are_rejected()
    {
        using var app=new HostelFactory();using var client=app.NewClient();
        var response=await client.PostAsync("/Account/Login",new FormUrlEncodedContent(new Dictionary<string,string>{["Email"]="admin@test.local",["Password"]="Test-only-123!"}));
        Assert.Equal(HttpStatusCode.BadRequest,response.StatusCode);
    }
    [Theory]
    [InlineData("student@test.local","/Student/Dashboard")]
    [InlineData("student@test.local","/Student/Profile")]
    [InlineData("student@test.local","/Student/EditProfile")]
    [InlineData("student@test.local","/Student/Complaints")]
    [InlineData("student@test.local","/Student/ComplaintDetails/1")]
    [InlineData("student@test.local","/Student/CreateComplaint")]
    [InlineData("student@test.local","/Student/EditComplaint/1")]
    [InlineData("student@test.local","/Student/RoomChangeRequests")]
    [InlineData("student@test.local","/Student/RequestRoomChange")]
    [InlineData("student@test.local","/Student/Mess")]
    [InlineData("student@test.local","/Student/Feedback")]
    [InlineData("student@test.local","/Student/SubmitFeedback")]
    [InlineData("student@test.local","/Student/Announcements")]
    [InlineData("warden@test.local","/Admin/Dashboard")]
    [InlineData("warden@test.local","/Admin/Rooms")]
    [InlineData("warden@test.local","/Admin/Rooms/Details/1")]
    [InlineData("warden@test.local","/Admin/Rooms/Create")]
    [InlineData("warden@test.local","/Admin/Rooms/Edit/1")]
    [InlineData("warden@test.local","/Admin/Rooms/Allocate/2")]
    [InlineData("warden@test.local","/Admin/Students")]
    [InlineData("warden@test.local","/Admin/Students/Details/1")]
    [InlineData("warden@test.local","/Admin/Complaints")]
    [InlineData("warden@test.local","/Admin/Complaints/Details/1")]
    [InlineData("warden@test.local","/Admin/RoomChangeRequests")]
    [InlineData("warden@test.local","/Admin/MessMenus")]
    [InlineData("warden@test.local","/Admin/MessMenus/Create")]
    [InlineData("warden@test.local","/Admin/MessMenus/Edit/1")]
    [InlineData("warden@test.local","/Admin/MessMenus/Details/1")]
    [InlineData("warden@test.local","/Admin/MessMenus/Participation/1")]
    [InlineData("warden@test.local","/Admin/MaintenanceStaff")]
    [InlineData("warden@test.local","/Admin/MaintenanceStaff/Create")]
    [InlineData("warden@test.local","/Admin/MaintenanceStaff/Edit/1")]
    [InlineData("warden@test.local","/Admin/MaintenanceStaff/Details/1")]
    [InlineData("warden@test.local","/Admin/Feedback")]
    [InlineData("admin@test.local","/Admin/Dashboard")]
    [InlineData("admin@test.local","/Admin/AllHostelsOverview")]
    [InlineData("admin@test.local","/Admin/Hostels")]
    [InlineData("admin@test.local","/Admin/Hostels/Create")]
    [InlineData("admin@test.local","/Admin/Hostels/Edit/1")]
    [InlineData("admin@test.local","/Admin/Hostels/Details/1")]
    [InlineData("admin@test.local","/Admin/Announcements")]
    [InlineData("admin@test.local","/Admin/Announcements/Create")]
    [InlineData("admin@test.local","/Admin/MaintenanceStaff")]
    [InlineData("staff@test.local","/Maintenance/Dashboard")]
    [InlineData("staff@test.local","/Maintenance/AssignedComplaints")]
    public async Task Authenticated_feature_pages_render(string email,string path)
    {
        using var app=new HostelFactory();using var client=app.NewClient();await Login(client,email);
        var response=await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK,response.StatusCode);
        Assert.Contains("HostelHub",await response.Content.ReadAsStringAsync());
    }
    [Theory]
    [InlineData("student@test.local","/Admin/Rooms")]
    [InlineData("warden@test.local","/Admin/Rooms/Details/3")]
    [InlineData("warden@test.local","/Admin/Hostels/Create")]
    [InlineData("warden@test.local","/Admin/Students/Details/2")]
    [InlineData("other@test.local","/Student/ComplaintDetails/1")]
    [InlineData("staff@test.local","/Maintenance/ComplaintDetails/1")]
    [InlineData("unassigned@test.local","/Admin/Rooms")]
    public async Task Wrong_role_or_hostel_is_denied(string email,string path)
    {
        using var app=new HostelFactory();using var client=app.NewClient();await Login(client,email);
        var response=await client.GetAsync(path);
        Assert.True(response.StatusCode==HttpStatusCode.Forbidden || response.StatusCode==HttpStatusCode.NotFound ||
            (response.StatusCode==HttpStatusCode.Redirect && response.Headers.Location!.ToString().Contains("/Account/AccessDenied")),
            $"Expected denial, got {response.StatusCode} {response.Headers.Location}");
    }
    [Fact] public async Task Deactivating_staff_revokes_existing_session_access()
    {
        using var app=new HostelFactory();using var client=app.NewClient();await Login(client,"staff@test.local");
        using(var scope=app.Services.CreateScope()) {
            var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            (await db.MaintenanceStaff.FindAsync(1))!.IsActive=false;await db.SaveChangesAsync();
        }
        var response=await client.GetAsync("/Maintenance/Dashboard");
        Assert.Equal(HttpStatusCode.Redirect,response.StatusCode);
        Assert.Contains("/Account/AccessDenied",response.Headers.Location!.ToString());
    }
    [Fact] public async Task Login_does_not_redirect_to_external_return_url()
    {
        using var app=new HostelFactory();using var client=app.NewClient();var token=await Token(client,"/Account/Login");
        var response=await client.PostAsync("/Account/Login?returnUrl=https%3A%2F%2Fevil.example",new FormUrlEncodedContent(new Dictionary<string,string>{
            ["Email"]="student@test.local",["Password"]="Test-only-123!",["__RequestVerificationToken"]=token}));
        Assert.Equal(HttpStatusCode.Redirect,response.StatusCode);
        Assert.DoesNotContain("evil.example",response.Headers.Location!.ToString());
    }
    [Fact] public async Task Registration_persists_student_with_hashed_password_and_login_cookie()
    {
        using var app=new HostelFactory();using var client=app.NewClient();var token=await Token(client,"/Account/Register");
        var response=await client.PostAsync("/Account/Register",new FormUrlEncodedContent(new Dictionary<string,string>{
            ["Email"]="new-student@test.local",["FullName"]="New Student",["RollNumber"]="NEW1",
            ["Password"]="Test-only-123!",["ConfirmPassword"]="Test-only-123!",["__RequestVerificationToken"]=token}));
        Assert.Equal(HttpStatusCode.Redirect,response.StatusCode);
        using var scope=app.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var student=await db.Students.Include(s=>s.User).SingleAsync(s=>s.RollNumber=="NEW1");
        Assert.Equal(Hostel_hub.Models.UserRole.Student,student.User!.Role);
        Assert.NotEqual("Test-only-123!",student.User.PasswordHash);
        Assert.Equal(HttpStatusCode.OK,(await client.GetAsync("/Student/Dashboard")).StatusCode);
    }
    [Fact] public async Task Unknown_route_returns_404()
    {
        using var app=new HostelFactory();using var client=app.NewClient();
        Assert.Equal(HttpStatusCode.NotFound,(await client.GetAsync("/does-not-exist")).StatusCode);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Unallocated_student_sees_room_change_error_only_on_room_requests(bool post)
    {
        using var app = new HostelFactory();
        using var client = app.NewClient();
        await Login(client, "other@test.local"); // Has a hostel but no room.
        HttpResponseMessage response;
        if (post)
        {
            var token = await Token(client, "/Student/Profile");
            response = await client.PostAsync("/Student/RequestRoomChange",
                new FormUrlEncodedContent(new Dictionary<string, string> {
                    ["RequestedRoomId"] = "3", ["Reason"] = "Request a change",
                    ["__RequestVerificationToken"] = token
                }));
        }
        else response = await client.GetAsync("/Student/RequestRoomChange");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Student/RoomChangeRequests", response.Headers.Location!.OriginalString);
        const string message = "You must have an existing room before requesting a change.";
        // Visiting another feature first must not consume or display the error.
        Assert.DoesNotContain(message, await client.GetStringAsync("/Student/Complaints"));
        Assert.Contains(message, await client.GetStringAsync("/Student/RoomChangeRequests"));
        Assert.DoesNotContain(message, await client.GetStringAsync("/Student/Complaints"));
        Assert.DoesNotContain(message, await client.GetStringAsync("/Student/RoomChangeRequests"));
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.False(await db.RoomChangeRequests.AnyAsync(r => r.StudentId == 2));
    }
}
