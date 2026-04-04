using Microsoft.AspNetCore.Mvc;
using Vacation_Manager.Models;
using Vacation_Manager.Services;

namespace Vacation_Manager.Controllers;

public sealed class RolesController : BaseAppController
{
    public RolesController(AppRepository repository, CurrentUserAccessor currentUserAccessor)
        : base(repository, currentUserAccessor)
    {
    }

    public IActionResult Index(int page = 1, int pageSize = 10)
    {
        pageSize = NormalizePageSize(pageSize);
        return View(new RoleIndexViewModel { Roles = Repository.GetRolesPage(page, pageSize) });
    }

    public IActionResult Details(int id)
    {
        var role = Repository.GetRole(id);
        if (role is null)
        {
            return NotFound();
        }

        return View(new RoleDetailsViewModel
        {
            Role = role,
            Users = Repository.Users.Where(u => u.RoleId == id).OrderBy(u => u.FirstName).ToList()
        });
    }

    [HttpGet]
    public IActionResult Create()
    {
        var ceoResult = CeoOnly();
        if (ceoResult is not EmptyResult)
        {
            return ceoResult;
        }

        return View(new Role());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Role model)
    {
        var ceoResult = CeoOnly();
        if (ceoResult is not EmptyResult)
        {
            return ceoResult;
        }

        if (Repository.Roles.Any(r => string.Equals(r.Name, model.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError(nameof(model.Name), "Role already exists.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        model.Name = model.Name.Trim();
        Repository.CreateRole(model);
        TempData["Alert"] = "Role created successfully.";
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

        var role = Repository.GetRole(id);
        return role is null ? NotFound() : View(role);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Role model)
    {
        var ceoResult = CeoOnly();
        if (ceoResult is not EmptyResult)
        {
            return ceoResult;
        }

        if (Repository.Roles.Any(r => r.Id != model.Id && string.Equals(r.Name, model.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError(nameof(model.Name), "Role already exists.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        model.Name = model.Name.Trim();
        Repository.UpdateRole(model);
        TempData["Alert"] = "Role updated successfully.";
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

        TempData["Alert"] = Repository.DeleteRole(id)
            ? "Role deleted."
            : "Role cannot be deleted while users are assigned to it.";
        return RedirectToAction(nameof(Index));
    }
}
