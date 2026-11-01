using System.ComponentModel.DataAnnotations;
using Hostel_hub.Controllers;
using Hostel_hub.Models;
using Hostel_hub.Services;
using Hostel_hub.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Claims;

namespace HostelHub.Tests;

public class UnitTests
{
    [Fact]
    public void Passwords_are_salted_and_wrong_password_is_rejected()
    {
        var auth = new AuthService();
        var first = auth.HashPassword("Example-123!");
        Assert.NotEqual("Example-123!", first);
        Assert.NotEqual(first, auth.HashPassword("Example-123!"));
        Assert.True(auth.VerifyPassword("Example-123!", first));
        Assert.False(auth.VerifyPassword("wrong", first));
    }

    [Theory]
    [InlineData("Student", "Student")]
    [InlineData("Admin", "Admin")]
    [InlineData("MaintenanceStaff", "Maintenance")]
    public void Home_routes_authenticated_roles_to_their_dashboard(string role, string controller)
    {
        var home = new HomeController(NullLogger<HomeController>.Instance)
        { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[] {new Claim(ClaimTypes.Role, role)}, "unit-test")) } } };
        var result = Assert.IsType<RedirectToActionResult>(home.Index());
        Assert.Equal(controller, result.ControllerName);
        Assert.Equal("Dashboard", result.ActionName);
    }

    [Theory]
    [InlineData("2026-11-03T22:59:59Z", true)]
    [InlineData("2026-11-03T23:00:00Z", true)]
    [InlineData("2026-11-03T23:00:01Z", false)]
    public void Breakfast_cutoff_is_previous_day_and_inclusive(string now, bool expected)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?> {
            ["MealCutoffs:Breakfast:DaysBefore"]="1", ["MealCutoffs:Breakfast:Time"]="23:00" }).Build();
        // This pure cutoff method must not access its database dependency.
        var service = new MealSelectionService(null!,config,NullLogger<MealSelectionService>.Instance,new FixedClock(DateTimeOffset.Parse(now)));
        Assert.Equal(expected, service.IsMealSelectionWithinCutoff(new DateOnly(2026,11,4),MealType.Breakfast,out var cutoff));
        Assert.Equal(new DateTime(2026,11,3,23,0,0),cutoff);
    }

    [Theory]
    [InlineData(MealType.Lunch, "11:30", 11, 30)]
    [InlineData(MealType.Dinner, "19:00", 19, 0)]
    public void Lunch_and_dinner_cutoffs_are_on_menu_date(MealType meal,string time,int hour,int minute)
    {
        var config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?> {[$"MealCutoffs:{meal}:Time"]=time}).Build();
        var service=new MealSelectionService(null!,config,NullLogger<MealSelectionService>.Instance,new FixedClock(new DateTimeOffset(2026,11,4,0,0,0,TimeSpan.Zero)));
        Assert.True(service.IsMealSelectionWithinCutoff(new DateOnly(2026,11,4),meal,out var cutoff));
        Assert.Equal(new DateTime(2026,11,4,hour,minute,0),cutoff);
    }

    [Fact]
    public void Registration_rejects_short_or_mismatched_passwords()
    {
        var model = new RegisterViewModel {Email="test@example.com",Password="short",ConfirmPassword="different",FullName="Test",RollNumber="T1"};
        var errors=new List<ValidationResult>();
        Assert.False(Validator.TryValidateObject(model,new ValidationContext(model),errors,true));
        Assert.Contains(errors,e=>e.MemberNames.Contains("Password"));
        Assert.Contains(errors,e=>e.MemberNames.Contains("ConfirmPassword"));
    }

    [Theory]
    [InlineData(0)] [InlineData(6)]
    public void Feedback_model_rejects_out_of_range_rating(int rating)
    {
        var feedback=new Feedback {Rating=rating};
        Assert.False(Validator.TryValidateObject(feedback,new ValidationContext(feedback),new List<ValidationResult>(),true));
    }
}
