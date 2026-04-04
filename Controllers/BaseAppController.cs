using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vacation_Manager.Models;
using Vacation_Manager.Services;

namespace Vacation_Manager.Controllers;

[Authorize]
public abstract class BaseAppController : Controller
{
    protected readonly AppRepository Repository;
    protected readonly CurrentUserAccessor CurrentUserAccessor;

    protected BaseAppController(AppRepository repository, CurrentUserAccessor currentUserAccessor)
    {
        Repository = repository;
        CurrentUserAccessor = currentUserAccessor;
    }

    protected AppUser CurrentUser => CurrentUserAccessor.GetCurrentUser()
        ?? throw new InvalidOperationException("Authenticated user not found.");

    protected int NormalizePageSize(int pageSize) => pageSize is 25 or 50 ? pageSize : 10;

    protected IActionResult CeoOnly()
    {
        if (!Repository.IsChiefExecutive(CurrentUser))
        {
            TempData["Alert"] = "Only the CEO can access this action.";
            return RedirectToAction("Index", "Home");
        }

        return new EmptyResult();
    }
}
