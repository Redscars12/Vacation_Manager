using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Vacation_Manager.Models;
using Vacation_Manager.Services;

namespace Vacation_Manager.Controllers;

public sealed class UsersController : BaseAppController
{
    public UsersController(AppRepository repository, CurrentUserAccessor currentUserAccessor)
        : base(repository, currentUserAccessor)
    {
    }

    public IActionResult Index(string? search, string? role, int page = 1, int pageSize = 10)
    {
        pageSize = NormalizePageSize(pageSize);
        var model = new UserIndexViewModel
        {
            Users = Repository.GetUsersPage(search, role, page, pageSize),
            Search = search,
            Role = role,
            RoleOptions = Repository.Roles.Select(r => new SelectListItem(r.Name, r.Name, r.Name == role)).ToList()
        };

        return View(model);
    }

    public IActionResult Details(int id)
    {
        var user = Repository.GetUser(id);
        if (user is null)
        {
            return NotFound();
        }

        var model = new UserDetailsViewModel
        {
            User = user,
            RoleName = Repository.GetRoleName(user.RoleId),
            TeamName = Repository.GetTeamName(user.TeamId),
            LedTeamName = Repository.GetLedTeam(user.Id)?.Name ?? "Does not lead a team",
            Leaves = Repository.GetUserLeaves(user.Id).Take(10).Select(Repository.ToLeaveListItem).ToList()
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        var ceoResult = CeoOnly();
        if (ceoResult is not EmptyResult)
        {
            return ceoResult;
        }

        return View(BuildUserForm(new UserFormViewModel()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(UserFormViewModel model)
    {
        var ceoResult = CeoOnly();
        if (ceoResult is not EmptyResult)
        {
            return ceoResult;
        }

        if (Repository.Users.Any(u => string.Equals(u.Username, model.Username.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError(nameof(model.Username), "Username already exists.");
        }

        if (!ModelState.IsValid)
        {
            return View(BuildUserForm(model));
        }

        Repository.CreateUser(model);
        TempData["Alert"] = "User created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var ceoResult = CeoOnly();
        if (ceoResult is not EmptyResult)
        {
            return ceoResult;
        }

        var user = Repository.GetUser(id);
        if (user is null)
        {
            return NotFound();
        }

        return View(BuildUserForm(new UserFormViewModel
        {
            Id = user.Id,
            Username = user.Username,
            Password = user.Password,
            FirstName = user.FirstName,
            LastName = user.LastName,
            RoleId = user.RoleId,
            TeamId = user.TeamId
        }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(UserFormViewModel model)
    {
        var ceoResult = CeoOnly();
        if (ceoResult is not EmptyResult)
        {
            return ceoResult;
        }

        if (Repository.Users.Any(u => u.Id != model.Id && string.Equals(u.Username, model.Username.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError(nameof(model.Username), "Username already exists.");
        }

        if (!ModelState.IsValid)
        {
            return View(BuildUserForm(model));
        }

        Repository.UpdateUser(model);
        TempData["Alert"] = "User updated successfully.";
        return RedirectToAction(nameof(Details), new { id = model.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var ceoResult = CeoOnly();
        if (ceoResult is not EmptyResult)
        {
            return ceoResult;
        }

        Repository.DeleteUser(id);
        TempData["Alert"] = "User deleted.";
        return RedirectToAction(nameof(Index));
    }

    private UserFormViewModel BuildUserForm(UserFormViewModel model)
    {
        model.RoleOptions = Repository.RoleOptions(model.RoleId);
        model.TeamOptions = Repository.TeamOptions(model.TeamId);
        return model;
    }
}
