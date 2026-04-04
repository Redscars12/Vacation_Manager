using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vacation_Manager.Data;
using Vacation_Manager.Models;

namespace Vacation_Manager.Services;

public sealed class AppRepository
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _environment;

    public AppRepository(ApplicationDbContext db, IWebHostEnvironment environment)
    {
        _db = db;
        _environment = environment;
    }

    public IEnumerable<Role> Roles => _db.Roles.AsNoTracking().AsEnumerable();
    public IEnumerable<AppUser> Users => _db.Users.AsNoTracking().AsEnumerable();
    public IEnumerable<Project> Projects => _db.Projects.AsNoTracking().AsEnumerable();
    public IEnumerable<Team> Teams => _db.Teams.AsNoTracking().AsEnumerable();
    public IEnumerable<LeaveRequest> Leaves => _db.LeaveRequests.AsNoTracking().AsEnumerable();

    public AppUser? ValidateUser(string username, string password)
    {
        return _db.Users
            .AsNoTracking()
            .FirstOrDefault(u =>
                u.Username.ToLower() == username.Trim().ToLower() &&
                u.Password == password.Trim());
    }

    public Role? GetRole(int id) => _db.Roles.AsNoTracking().FirstOrDefault(r => r.Id == id);

    public AppUser? GetUser(int id) => _db.Users
        .AsNoTracking()
        .Include(u => u.Role)
        .Include(u => u.Team)
        .FirstOrDefault(u => u.Id == id);

    public Project? GetProject(int id) => _db.Projects.AsNoTracking().FirstOrDefault(p => p.Id == id);

    public Team? GetTeam(int id) => _db.Teams
        .AsNoTracking()
        .Include(t => t.Project)
        .Include(t => t.TeamLead)
        .FirstOrDefault(t => t.Id == id);

    public LeaveRequest? GetLeave(int id) => _db.LeaveRequests
        .AsNoTracking()
        .Include(l => l.Applicant)
        .Include(l => l.ApprovedBy)
        .FirstOrDefault(l => l.Id == id);

    public string GetRoleName(int roleId) => _db.Roles
        .Where(r => r.Id == roleId)
        .Select(r => r.Name)
        .FirstOrDefault() ?? AppRoles.Unassigned;

    public string GetTeamName(int? teamId) => teamId.HasValue
        ? _db.Teams.Where(t => t.Id == teamId.Value).Select(t => t.Name).FirstOrDefault() ?? "No team"
        : "No team";

    public string GetProjectName(int? projectId) => projectId.HasValue
        ? _db.Projects.Where(p => p.Id == projectId.Value).Select(p => p.Name).FirstOrDefault() ?? "No project"
        : "No project";

    public string GetUserName(int? userId) => userId.HasValue
        ? _db.Users.Where(u => u.Id == userId.Value).Select(u => u.FirstName + " " + u.LastName).FirstOrDefault() ?? "-"
        : "-";

    public Team? GetLedTeam(int userId) => _db.Teams.AsNoTracking().FirstOrDefault(t => t.TeamLeadId == userId);

    public IReadOnlyList<AppUser> GetTeamMembers(int teamId) => _db.Users
        .AsNoTracking()
        .Where(u => u.TeamId == teamId)
        .OrderBy(u => u.FirstName)
        .ThenBy(u => u.LastName)
        .ToList();

    public IReadOnlyList<Team> GetProjectTeams(int projectId) => _db.Teams
        .AsNoTracking()
        .Where(t => t.ProjectId == projectId)
        .OrderBy(t => t.Name)
        .ToList();

    public IReadOnlyList<LeaveRequest> GetUserLeaves(int userId) => _db.LeaveRequests
        .AsNoTracking()
        .Where(l => l.ApplicantId == userId)
        .OrderByDescending(l => l.CreatedOn)
        .ToList();

    public PagedResult<UserListItemViewModel> GetUsersPage(string? search, string? role, int page, int pageSize)
    {
        var query = _db.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .Include(u => u.Team)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(u =>
                u.Username.ToLower().Contains(term) ||
                u.FirstName.ToLower().Contains(term) ||
                u.LastName.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            query = query.Where(u => u.Role != null && u.Role.Name == role);
        }

        var total = query.Count();
        var items = query
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserListItemViewModel
            {
                User = u,
                RoleName = u.Role != null ? u.Role.Name : AppRoles.Unassigned,
                TeamName = u.Team != null ? u.Team.Name : "No team"
            })
            .ToList();

        return new PagedResult<UserListItemViewModel> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }

    public PagedResult<RoleSummaryViewModel> GetRolesPage(int page, int pageSize)
    {
        var query = _db.Roles.AsNoTracking().Include(r => r.Users);
        var total = query.Count();
        var items = query
            .OrderBy(r => r.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new RoleSummaryViewModel
            {
                Role = r,
                UserCount = r.Users.Count
            })
            .ToList();

        return new PagedResult<RoleSummaryViewModel> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }

    public PagedResult<TeamListItemViewModel> GetTeamsPage(string? search, int page, int pageSize)
    {
        var query = _db.Teams
            .AsNoTracking()
            .Include(t => t.Project)
            .Include(t => t.TeamLead)
            .Include(t => t.Members)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(t =>
                t.Name.ToLower().Contains(term) ||
                (t.Project != null && t.Project.Name.ToLower().Contains(term)));
        }

        var total = query.Count();
        var items = query
            .OrderBy(t => t.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TeamListItemViewModel
            {
                Team = t,
                ProjectName = t.Project != null ? t.Project.Name : "No project",
                TeamLeadName = t.TeamLead != null ? t.TeamLead.FullName : "No lead",
                MembersCount = t.Members.Count
            })
            .ToList();

        return new PagedResult<TeamListItemViewModel> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }

    public PagedResult<ProjectListItemViewModel> GetProjectsPage(string? search, int page, int pageSize)
    {
        var query = _db.Projects.AsNoTracking().Include(p => p.Teams).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                p.Description.ToLower().Contains(term));
        }

        var total = query.Count();
        var items = query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProjectListItemViewModel
            {
                Project = p,
                TeamCount = p.Teams.Count
            })
            .ToList();

        return new PagedResult<ProjectListItemViewModel> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }

    public PagedResult<LeaveListItemViewModel> GetLeavesPage(AppUser user, DateTime? createdAfter, int page, int pageSize)
    {
        var query = _db.LeaveRequests
            .AsNoTracking()
            .Include(l => l.Applicant)
            .Include(l => l.ApprovedBy)
            .Where(l => l.ApplicantId == user.Id);

        if (createdAfter.HasValue)
        {
            query = query.Where(l => l.CreatedOn.Date >= createdAfter.Value.Date);
        }

        var total = query.Count();
        var items = query
            .OrderByDescending(l => l.CreatedOn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsEnumerable()
            .Select(ToLeaveListItem)
            .ToList();

        return new PagedResult<LeaveListItemViewModel> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }

    public PagedResult<LeaveListItemViewModel> GetPendingApprovalsPage(AppUser approver, int page, int pageSize)
    {
        var allPending = _db.LeaveRequests
            .AsNoTracking()
            .Include(l => l.Applicant)
            .Include(l => l.ApprovedBy)
            .Where(l => !l.IsApproved)
            .AsEnumerable()
            .Where(l => CanApprove(approver, l))
            .OrderBy(l => l.StartDate)
            .ToList();

        var total = allPending.Count;
        var items = allPending
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ToLeaveListItem)
            .ToList();

        return new PagedResult<LeaveListItemViewModel> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }

    public LeaveListItemViewModel ToLeaveListItem(LeaveRequest leave)
    {
        var applicant = leave.Applicant ?? _db.Users.AsNoTracking().First(u => u.Id == leave.ApplicantId);
        return new LeaveListItemViewModel
        {
            Leave = leave,
            Applicant = applicant,
            ApproverName = leave.ApprovedBy?.FullName ?? GetUserName(leave.ApprovedById)
        };
    }

    public bool IsChiefExecutive(AppUser user) => string.Equals(GetRoleName(user.RoleId), AppRoles.CEO, StringComparison.Ordinal);
    public bool IsTeamLead(AppUser user) => string.Equals(GetRoleName(user.RoleId), AppRoles.TeamLead, StringComparison.Ordinal);
    public bool IsChiefExecutiveOrLead(AppUser user) => IsChiefExecutive(user) || IsTeamLead(user);

    public bool CanApprove(AppUser approver, LeaveRequest leave)
    {
        if (approver.Id == leave.ApplicantId)
        {
            return false;
        }

        if (IsChiefExecutive(approver))
        {
            return true;
        }

        var applicant = _db.Users.AsNoTracking().FirstOrDefault(u => u.Id == leave.ApplicantId);
        var ledTeam = GetLedTeam(approver.Id);
        return applicant is not null && ledTeam is not null && applicant.TeamId == ledTeam.Id;
    }

    public bool SaveAttachment(IFormFile file, out string originalName, out string storedName)
    {
        originalName = file.FileName;
        storedName = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
        var directory = Path.Combine(_environment.WebRootPath, "uploads");
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, storedName);
        using var stream = File.Create(path);
        file.CopyTo(stream);
        return true;
    }

    public void CreateRole(Role role)
    {
        role.Name = role.Name.Trim();
        _db.Roles.Add(role);
        _db.SaveChanges();
    }

    public bool UpdateRole(Role role)
    {
        var existing = _db.Roles.FirstOrDefault(r => r.Id == role.Id);
        if (existing is null)
        {
            return false;
        }

        existing.Name = role.Name.Trim();
        _db.SaveChanges();
        return true;
    }

    public bool DeleteRole(int id)
    {
        if (_db.Users.Any(u => u.RoleId == id))
        {
            return false;
        }

        var role = _db.Roles.FirstOrDefault(r => r.Id == id);
        if (role is null)
        {
            return false;
        }

        _db.Roles.Remove(role);
        _db.SaveChanges();
        return true;
    }

    public void CreateUser(UserFormViewModel form)
    {
        _db.Users.Add(new AppUser
        {
            Username = form.Username.Trim(),
            Password = form.Password.Trim(),
            FirstName = form.FirstName.Trim(),
            LastName = form.LastName.Trim(),
            RoleId = form.RoleId,
            TeamId = form.TeamId
        });
        _db.SaveChanges();
    }

    public bool UpdateUser(UserFormViewModel form)
    {
        if (!form.Id.HasValue)
        {
            return false;
        }

        var user = _db.Users.FirstOrDefault(u => u.Id == form.Id.Value);
        if (user is null)
        {
            return false;
        }

        user.Username = form.Username.Trim();
        user.Password = form.Password.Trim();
        user.FirstName = form.FirstName.Trim();
        user.LastName = form.LastName.Trim();
        user.RoleId = form.RoleId;
        user.TeamId = form.TeamId;
        _db.SaveChanges();
        return true;
    }

    public bool DeleteUser(int id)
    {
        var user = _db.Users.FirstOrDefault(u => u.Id == id);
        if (user is null)
        {
            return false;
        }

        foreach (var team in _db.Teams.Where(t => t.TeamLeadId == id))
        {
            team.TeamLeadId = null;
        }

        var leaves = _db.LeaveRequests.Where(l => l.ApplicantId == id || l.ApprovedById == id);
        _db.LeaveRequests.RemoveRange(leaves);
        _db.Users.Remove(user);
        _db.SaveChanges();
        return true;
    }

    public void CreateProject(ProjectFormViewModel form)
    {
        _db.Projects.Add(new Project
        {
            Name = form.Name.Trim(),
            Description = form.Description.Trim()
        });
        _db.SaveChanges();
    }

    public bool UpdateProject(ProjectFormViewModel form)
    {
        if (!form.Id.HasValue)
        {
            return false;
        }

        var project = _db.Projects.FirstOrDefault(p => p.Id == form.Id.Value);
        if (project is null)
        {
            return false;
        }

        project.Name = form.Name.Trim();
        project.Description = form.Description.Trim();
        _db.SaveChanges();
        return true;
    }

    public bool DeleteProject(int id)
    {
        var project = _db.Projects.FirstOrDefault(p => p.Id == id);
        if (project is null)
        {
            return false;
        }

        foreach (var team in _db.Teams.Where(t => t.ProjectId == id))
        {
            team.ProjectId = null;
        }

        _db.Projects.Remove(project);
        _db.SaveChanges();
        return true;
    }

    public void CreateTeam(TeamFormViewModel form)
    {
        var team = new Team
        {
            Name = form.Name.Trim(),
            ProjectId = form.ProjectId,
            TeamLeadId = form.TeamLeadId
        };

        _db.Teams.Add(team);
        _db.SaveChanges();
        SyncTeamMembers(team.Id, form.MemberIds, form.TeamLeadId);
    }

    public bool UpdateTeam(TeamFormViewModel form)
    {
        if (!form.Id.HasValue)
        {
            return false;
        }

        var team = _db.Teams.FirstOrDefault(t => t.Id == form.Id.Value);
        if (team is null)
        {
            return false;
        }

        team.Name = form.Name.Trim();
        team.ProjectId = form.ProjectId;
        team.TeamLeadId = form.TeamLeadId;
        _db.SaveChanges();
        SyncTeamMembers(team.Id, form.MemberIds, form.TeamLeadId);
        return true;
    }

    public bool DeleteTeam(int id)
    {
        var team = _db.Teams.FirstOrDefault(t => t.Id == id);
        if (team is null)
        {
            return false;
        }

        foreach (var user in _db.Users.Where(u => u.TeamId == id))
        {
            user.TeamId = null;
        }

        _db.Teams.Remove(team);
        _db.SaveChanges();
        return true;
    }

    private void SyncTeamMembers(int teamId, List<int> memberIds, int? teamLeadId)
    {
        var intendedMembers = memberIds.Distinct().ToHashSet();
        if (teamLeadId.HasValue)
        {
            intendedMembers.Add(teamLeadId.Value);
        }

        foreach (var user in _db.Users)
        {
            if (intendedMembers.Contains(user.Id))
            {
                user.TeamId = teamId;
            }
            else if (user.TeamId == teamId)
            {
                user.TeamId = null;
            }
        }

        _db.SaveChanges();
    }

    public void CreateLeave(AppUser applicant, LeaveFormViewModel form, IFormFile? attachment)
    {
        var leave = new LeaveRequest
        {
            ApplicantId = applicant.Id,
            Type = form.Type,
            StartDate = form.StartDate.Date,
            EndDate = form.EndDate.Date,
            CreatedOn = DateTime.Now,
            IsHalfDay = form.Type == LeaveType.Sick ? false : form.IsHalfDay,
            IsApproved = false
        };

        if (attachment is not null && attachment.Length > 0)
        {
            SaveAttachment(attachment, out var original, out var stored);
            leave.AttachmentFileName = original;
            leave.AttachmentStoredName = stored;
        }

        _db.LeaveRequests.Add(leave);
        _db.SaveChanges();
    }

    public bool UpdateLeave(AppUser applicant, LeaveFormViewModel form, IFormFile? attachment)
    {
        if (!form.Id.HasValue)
        {
            return false;
        }

        var leave = _db.LeaveRequests.FirstOrDefault(l => l.Id == form.Id.Value);
        if (leave is null || leave.ApplicantId != applicant.Id || leave.IsApproved)
        {
            return false;
        }

        leave.Type = form.Type;
        leave.StartDate = form.StartDate.Date;
        leave.EndDate = form.EndDate.Date;
        leave.IsHalfDay = form.Type == LeaveType.Sick ? false : form.IsHalfDay;

        if (attachment is not null && attachment.Length > 0)
        {
            SaveAttachment(attachment, out var original, out var stored);
            leave.AttachmentFileName = original;
            leave.AttachmentStoredName = stored;
        }

        _db.SaveChanges();
        return true;
    }

    public bool DeleteLeave(AppUser applicant, int id)
    {
        var leave = _db.LeaveRequests.FirstOrDefault(l => l.Id == id);
        if (leave is null || leave.ApplicantId != applicant.Id || leave.IsApproved)
        {
            return false;
        }

        _db.LeaveRequests.Remove(leave);
        _db.SaveChanges();
        return true;
    }

    public bool ApproveLeave(AppUser approver, int id)
    {
        var leave = _db.LeaveRequests.FirstOrDefault(l => l.Id == id);
        if (leave is null || leave.IsApproved || !CanApprove(approver, leave))
        {
            return false;
        }

        leave.IsApproved = true;
        leave.ApprovedById = approver.Id;
        _db.SaveChanges();
        return true;
    }

    public IReadOnlyList<SelectListItem> RoleOptions(int? selected = null) =>
        _db.Roles.AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new SelectListItem(r.Name, r.Id.ToString(), selected == r.Id))
            .ToList();

    public IReadOnlyList<SelectListItem> TeamOptions(int? selected = null)
    {
        var items = new List<SelectListItem> { new("No team", string.Empty, !selected.HasValue) };
        items.AddRange(_db.Teams.AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new SelectListItem(t.Name, t.Id.ToString(), selected == t.Id)));
        return items;
    }

    public IReadOnlyList<SelectListItem> ProjectOptions(int? selected = null)
    {
        var items = new List<SelectListItem> { new("No project", string.Empty, !selected.HasValue) };
        items.AddRange(_db.Projects.AsNoTracking()
            .OrderBy(p => p.Name)
            .Select(p => new SelectListItem(p.Name, p.Id.ToString(), selected == p.Id)));
        return items;
    }

    public IReadOnlyList<SelectListItem> UserOptions(int? selected = null, Func<AppUser, bool>? predicate = null)
    {
        predicate ??= _ => true;
        var items = new List<SelectListItem> { new("None", string.Empty, !selected.HasValue) };
        items.AddRange(_db.Users.AsNoTracking()
            .AsEnumerable()
            .Where(predicate)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Select(u => new SelectListItem($"{u.FullName} ({u.Username})", u.Id.ToString(), selected == u.Id)));
        return items;
    }
}
