using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Vacation_Manager.Models;
using Vacation_Manager.Services;

namespace Vacation_Manager.Controllers;

public sealed class HomeController : BaseAppController
{
    public HomeController(AppRepository repository, CurrentUserAccessor currentUserAccessor)
        : base(repository, currentUserAccessor)
    {
    }

    public IActionResult Index()
    {
        var user = CurrentUser;
        var model = new DashboardViewModel
        {
            CurrentUser = user,
            CurrentRoleName = Repository.GetRoleName(user.RoleId),
            TotalUsers = Repository.Users.Count(),
            TotalTeams = Repository.Teams.Count(),
            TotalProjects = Repository.Projects.Count(),
            TotalLeaveRequests = Repository.Leaves.Count(),
            PendingApprovals = Repository.GetPendingApprovalsPage(user, 1, 10).TotalCount,
            RecentLeaves = Repository.GetLeavesPage(user, null, 1, 5).Items
        };

        return View(model);
    }

    [AllowAnonymous]
    public IActionResult Privacy()
    {
        return View();
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
