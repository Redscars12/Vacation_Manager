using Microsoft.AspNetCore.Mvc;
using Vacation_Manager.Models;
using Vacation_Manager.Services;

namespace Vacation_Manager.Controllers;

public sealed class ProjectsController : BaseAppController
{
    public ProjectsController(AppRepository repository, CurrentUserAccessor currentUserAccessor)
        : base(repository, currentUserAccessor)
    {
    }

    public IActionResult Index(string? search, int page = 1, int pageSize = 10)
    {
        pageSize = NormalizePageSize(pageSize);
        return View(new ProjectIndexViewModel
        {
            Projects = Repository.GetProjectsPage(search, page, pageSize),
            Search = search
        });
    }

    public IActionResult Details(int id)
    {
        var project = Repository.GetProject(id);
        if (project is null)
        {
            return NotFound();
        }

        return View(new ProjectDetailsViewModel
        {
            Project = project,
            Teams = Repository.GetProjectTeams(id)
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

        return View(new ProjectFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(ProjectFormViewModel model)
    {
        var ceoResult = CeoOnly();
        if (ceoResult is not EmptyResult)
        {
            return ceoResult;
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        Repository.CreateProject(model);
        TempData["Alert"] = "Project created successfully.";
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

        var project = Repository.GetProject(id);
        if (project is null)
        {
            return NotFound();
        }

        return View(new ProjectFormViewModel
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(ProjectFormViewModel model)
    {
        var ceoResult = CeoOnly();
        if (ceoResult is not EmptyResult)
        {
            return ceoResult;
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        Repository.UpdateProject(model);
        TempData["Alert"] = "Project updated successfully.";
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

        Repository.DeleteProject(id);
        TempData["Alert"] = "Project deleted.";
        return RedirectToAction(nameof(Index));
    }
}
