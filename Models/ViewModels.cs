using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Vacation_Manager.Models;

public sealed class LoginViewModel
{
    [Required, StringLength(40)]
    public string Username { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), StringLength(80)]
    public string Password { get; set; } = string.Empty;
}

public sealed class DashboardViewModel
{
    public required AppUser CurrentUser { get; init; }
    public required string CurrentRoleName { get; init; }
    public int TotalUsers { get; init; }
    public int TotalTeams { get; init; }
    public int TotalProjects { get; init; }
    public int TotalLeaveRequests { get; init; }
    public int PendingApprovals { get; init; }
    public IReadOnlyList<LeaveListItemViewModel> RecentLeaves { get; init; } = [];
}

public sealed class UserIndexViewModel
{
    public required PagedResult<UserListItemViewModel> Users { get; init; }
    public string? Search { get; init; }
    public string? Role { get; init; }
    public required IReadOnlyList<SelectListItem> RoleOptions { get; init; }
}

public sealed class UserListItemViewModel
{
    public required AppUser User { get; init; }
    public required string RoleName { get; init; }
    public string TeamName { get; init; } = "No team";
}

public sealed class UserFormViewModel
{
    public int? Id { get; set; }

    [Required, StringLength(40, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required, StringLength(80, MinimumLength = 4)]
    public string Password { get; set; } = string.Empty;

    [Required, StringLength(60)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(60)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public int RoleId { get; set; }

    public int? TeamId { get; set; }

    public IReadOnlyList<SelectListItem> RoleOptions { get; set; } = [];
    public IReadOnlyList<SelectListItem> TeamOptions { get; set; } = [];
}

public sealed class UserDetailsViewModel
{
    public required AppUser User { get; init; }
    public required string RoleName { get; init; }
    public string TeamName { get; init; } = "No team";
    public string LedTeamName { get; init; } = "Does not lead a team";
    public IReadOnlyList<LeaveListItemViewModel> Leaves { get; init; } = [];
}

public sealed class RoleIndexViewModel
{
    public required PagedResult<RoleSummaryViewModel> Roles { get; init; }
}

public sealed class RoleSummaryViewModel
{
    public required Role Role { get; init; }
    public int UserCount { get; init; }
}

public sealed class RoleDetailsViewModel
{
    public required Role Role { get; init; }
    public IReadOnlyList<AppUser> Users { get; init; } = [];
}

public sealed class TeamIndexViewModel
{
    public required PagedResult<TeamListItemViewModel> Teams { get; init; }
    public string? Search { get; init; }
}

public sealed class TeamListItemViewModel
{
    public required Team Team { get; init; }
    public string ProjectName { get; init; } = "No project";
    public string TeamLeadName { get; init; } = "No lead";
    public int MembersCount { get; init; }
}

public sealed class TeamFormViewModel
{
    public int? Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    public int? ProjectId { get; set; }
    public int? TeamLeadId { get; set; }
    public List<int> MemberIds { get; set; } = [];
    public IReadOnlyList<SelectListItem> ProjectOptions { get; set; } = [];
    public IReadOnlyList<SelectListItem> TeamLeadOptions { get; set; } = [];
    public IReadOnlyList<SelectListItem> MemberOptions { get; set; } = [];
}

public sealed class TeamDetailsViewModel
{
    public required Team Team { get; init; }
    public string ProjectName { get; init; } = "No project";
    public string TeamLeadName { get; init; } = "No lead";
    public IReadOnlyList<AppUser> Members { get; init; } = [];
}

public sealed class ProjectIndexViewModel
{
    public required PagedResult<ProjectListItemViewModel> Projects { get; init; }
    public string? Search { get; init; }
}

public sealed class ProjectListItemViewModel
{
    public required Project Project { get; init; }
    public int TeamCount { get; init; }
}

public sealed class ProjectFormViewModel
{
    public int? Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(600)]
    public string Description { get; set; } = string.Empty;
}

public sealed class ProjectDetailsViewModel
{
    public required Project Project { get; init; }
    public IReadOnlyList<Team> Teams { get; init; } = [];
}

public sealed class LeaveIndexViewModel
{
    public required PagedResult<LeaveListItemViewModel> Leaves { get; init; }
    public PagedResult<LeaveListItemViewModel>? PendingApprovals { get; init; }
    public DateTime? CreatedAfter { get; init; }
}

public sealed class LeaveListItemViewModel
{
    public required LeaveRequest Leave { get; init; }
    public required AppUser Applicant { get; init; }
    public string ApproverName { get; init; } = "-";
}

public sealed class LeaveFormViewModel : IValidatableObject
{
    public int? Id { get; set; }

    [Required]
    public LeaveType Type { get; set; }

    [Required, DataType(DataType.Date)]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Required, DataType(DataType.Date)]
    public DateTime EndDate { get; set; } = DateTime.Today;

    public bool IsHalfDay { get; set; }

    public string? ExistingAttachmentFileName { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndDate.Date < StartDate.Date)
        {
            yield return new ValidationResult("End date must be after or equal to the start date.", [nameof(EndDate)]);
        }

        if (Type == LeaveType.Sick && IsHalfDay)
        {
            yield return new ValidationResult("Half day is not allowed for sick leave.", [nameof(IsHalfDay)]);
        }
    }
}
