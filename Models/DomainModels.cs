using System.ComponentModel.DataAnnotations;

namespace Vacation_Manager.Models;

public static class AppRoles
{
    public const string CEO = "CEO";
    public const string Developer = "Developer";
    public const string TeamLead = "Team Lead";
    public const string Unassigned = "Unassigned";

    public static readonly string[] All = [CEO, Developer, TeamLead, Unassigned];
}

public enum LeaveType
{
    Paid = 1,
    Unpaid = 2,
    Sick = 3
}

public sealed class Role
{
    public int Id { get; set; }

    [Required, StringLength(60)]
    public string Name { get; set; } = string.Empty;
}

public sealed class AppUser
{
    public int Id { get; set; }

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

    public string FullName => $"{FirstName} {LastName}";
}

public sealed class Project
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(600)]
    public string Description { get; set; } = string.Empty;
}

public sealed class Team
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    public int? ProjectId { get; set; }

    public int? TeamLeadId { get; set; }
}

public sealed class LeaveRequest
{
    public int Id { get; set; }
    public LeaveType Type { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CreatedOn { get; set; }
    public bool IsHalfDay { get; set; }
    public bool IsApproved { get; set; }
    public int ApplicantId { get; set; }
    public int? ApprovedById { get; set; }
    public string? AttachmentFileName { get; set; }
    public string? AttachmentStoredName { get; set; }
}

public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => TotalCount == 0 ? 1 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
