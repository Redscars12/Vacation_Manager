using Microsoft.AspNetCore.Mvc;
using Vacation_Manager.Models;
using Vacation_Manager.Services;

namespace Vacation_Manager.Controllers;

public sealed class LeavesController : BaseAppController
{
    public LeavesController(AppRepository repository, CurrentUserAccessor currentUserAccessor)
        : base(repository, currentUserAccessor)
    {
    }

    public IActionResult Index(DateTime? createdAfter, int page = 1, int pageSize = 10, int approvalsPage = 1)
    {
        pageSize = NormalizePageSize(pageSize);
        var user = CurrentUser;

        return View(new LeaveIndexViewModel
        {
            Leaves = Repository.GetLeavesPage(user, createdAfter, page, pageSize),
            PendingApprovals = Repository.IsChiefExecutiveOrLead(user)
                ? Repository.GetPendingApprovalsPage(user, approvalsPage, pageSize)
                : null,
            CreatedAfter = createdAfter
        });
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new LeaveFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(LeaveFormViewModel model, IFormFile? attachment)
    {
        ValidateLeave(model, attachment);
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        Repository.CreateLeave(CurrentUser, model, attachment);
        TempData["Alert"] = "Leave request submitted.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var leave = Repository.GetLeave(id);
        if (leave is null || leave.ApplicantId != CurrentUser.Id || leave.IsApproved)
        {
            return NotFound();
        }

        return View(new LeaveFormViewModel
        {
            Id = leave.Id,
            Type = leave.Type,
            StartDate = leave.StartDate,
            EndDate = leave.EndDate,
            IsHalfDay = leave.IsHalfDay,
            ExistingAttachmentFileName = leave.AttachmentFileName
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(LeaveFormViewModel model, IFormFile? attachment)
    {
        ValidateLeave(model, attachment, true);
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (!Repository.UpdateLeave(CurrentUser, model, attachment))
        {
            return NotFound();
        }

        TempData["Alert"] = "Leave request updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        TempData["Alert"] = Repository.DeleteLeave(CurrentUser, id)
            ? "Leave request deleted."
            : "Only non-approved personal requests can be deleted.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Approve(int id)
    {
        TempData["Alert"] = Repository.ApproveLeave(CurrentUser, id)
            ? "Leave request approved."
            : "You cannot approve this request.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Download(int id)
    {
        var leave = Repository.GetLeave(id);
        if (leave is null || string.IsNullOrWhiteSpace(leave.AttachmentStoredName))
        {
            return NotFound();
        }

        if (leave.ApplicantId != CurrentUser.Id && !Repository.CanApprove(CurrentUser, leave) && !Repository.IsChiefExecutive(CurrentUser))
        {
            return Forbid();
        }

        var path = Path.Combine(StoragePathHelper.GetUploadsRoot(HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>()), leave.AttachmentStoredName);
        if (!System.IO.File.Exists(path))
        {
            return NotFound();
        }

        return PhysicalFile(path, "application/octet-stream", leave.AttachmentFileName ?? "attachment");
    }

    private void ValidateLeave(LeaveFormViewModel model, IFormFile? attachment, bool isEdit = false)
    {
        if (model.Type == LeaveType.Sick && !isEdit && (attachment is null || attachment.Length == 0))
        {
            ModelState.AddModelError("attachment", "Sick leave requires an attachment.");
        }

        if (attachment is not null && attachment.Length > 5 * 1024 * 1024)
        {
            ModelState.AddModelError("attachment", "Attachment must be smaller than 5 MB.");
        }

        if (model.Type == LeaveType.Sick && isEdit && string.IsNullOrWhiteSpace(model.ExistingAttachmentFileName) && (attachment is null || attachment.Length == 0))
        {
            ModelState.AddModelError("attachment", "Sick leave requires an attachment.");
        }
    }
}
