using Microsoft.AspNetCore.Mvc;
using Vacation_Manager.Models;
using Vacation_Manager.Services;

namespace Vacation_Manager.Controllers;

public sealed class TeamsController : BaseAppController
{
    public TeamsController(AppRepository repository, CurrentUserAccessor currentUserAccessor)
        : base(repository, currentUserAccessor)
    {
    }

    public IActionResult Index(string? search, int page = 1, int pageSize = 10)
    {
        pageSize = NormalizePageSize(pageSize);
        return View(new TeamIndexViewModel
        {
            Teams = Repository.GetTeamsPage(search, page, pageSize),
            Search = search
        });
    }

    public IActionResult Details(int id)
    {
        var team = Repository.GetTeam(id);
        if (team is null)
        {
            return NotFound();
        }

        return View(new TeamDetailsViewModel
        {
            Team = team,
            ProjectName = Repository.GetProjectName(team.ProjectId),
            TeamLeadName = Repository.GetUserName(team.TeamLeadId),
            Members = Repository.GetTeamMembers(team.Id)
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

        return View(BuildTeamForm(new TeamFormViewModel()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(TeamFormViewModel model)
    {
        var ceoResult = CeoOnly();
        if (ceoResult is not EmptyResult)
        {
            return ceoResult;
        }

        if (!ModelState.IsValid)
        {
            return View(BuildTeamForm(model));
        }

        Repository.CreateTeam(model);
        TempData["Alert"] = "Team created successfully.";
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

        var team = Repository.GetTeam(id);
        if (team is null)
        {
            return NotFound();
        }

        return View(BuildTeamForm(new TeamFormViewModel
        {
            Id = team.Id,
            Name = team.Name,
            ProjectId = team.ProjectId,
            TeamLeadId = team.TeamLeadId,
            MemberIds = Repository.GetTeamMembers(id).Select(m => m.Id).ToList()
        }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(TeamFormViewModel model)
    {
        var ceoResult = CeoOnly();
        if (ceoResult is not EmptyResult)
        {
            return ceoResult;
        }

        if (!ModelState.IsValid)
        {
            return View(BuildTeamForm(model));
        }

        Repository.UpdateTeam(model);
        TempData["Alert"] = "Team updated successfully.";
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

        Repository.DeleteTeam(id);
        TempData["Alert"] = "Team deleted.";
        return RedirectToAction(nameof(Index));
    }

    private TeamFormViewModel BuildTeamForm(TeamFormViewModel model)
    {
        model.ProjectOptions = Repository.ProjectOptions(model.ProjectId);
        model.TeamLeadOptions = Repository.UserOptions(model.TeamLeadId, u => Repository.IsTeamLead(u) || Repository.IsChiefExecutive(u));
        model.MemberOptions = Repository.UserOptions(null, u => !Repository.IsChiefExecutive(u));
        return model;
    }
}
