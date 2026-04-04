using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Vacation_Manager.Models;

namespace Vacation_Manager.Services;

public sealed class AppRepository
{
    private readonly IWebHostEnvironment _environment;
    private readonly List<Role> _roles = [];
    private readonly List<AppUser> _users = [];
    private readonly List<Project> _projects = [];
    private readonly List<Team> _teams = [];
    private readonly List<LeaveRequest> _leaves = [];
    private int _roleId = 1;
    private int _userId = 1;
    private int _projectId = 1;
    private int _teamId = 1;
    private int _leaveId = 1;

    public AppRepository(IWebHostEnvironment environment)
    {
        _environment = environment;
        Seed();
    }

    public IEnumerable<Role> Roles => _roles;
    public IEnumerable<AppUser> Users => _users;
    public IEnumerable<Project> Projects => _projects;
    public IEnumerable<Team> Teams => _teams;
    public IEnumerable<LeaveRequest> Leaves => _leaves;

    public AppUser? ValidateUser(string username, string password)
    {
        return _users.FirstOrDefault(u =>
            string.Equals(u.Username, username.Trim(), StringComparison.OrdinalIgnoreCase) &&
            u.Password == password.Trim());
    }

    public Role? GetRole(int id) => _roles.FirstOrDefault(r => r.Id == id);
    public AppUser? GetUser(int id) => _users.FirstOrDefault(u => u.Id == id);
    public Project? GetProject(int id) => _projects.FirstOrDefault(p => p.Id == id);
    public Team? GetTeam(int id) => _teams.FirstOrDefault(t => t.Id == id);
    public LeaveRequest? GetLeave(int id) => _leaves.FirstOrDefault(l => l.Id == id);

    public string GetRoleName(int roleId) => GetRole(roleId)?.Name ?? AppRoles.Unassigned;
    public string GetTeamName(int? teamId) => teamId.HasValue ? GetTeam(teamId.Value)?.Name ?? "No team" : "No team";
    public string GetProjectName(int? projectId) => projectId.HasValue ? GetProject(projectId.Value)?.Name ?? "No project" : "No project";
    public string GetUserName(int? userId) => userId.HasValue ? GetUser(userId.Value)?.FullName ?? "-" : "-";

    public Team? GetLedTeam(int userId) => _teams.FirstOrDefault(t => t.TeamLeadId == userId);
    public IReadOnlyList<AppUser> GetTeamMembers(int teamId) => _users.Where(u => u.TeamId == teamId).OrderBy(u => u.FirstName).ToList();
    public IReadOnlyList<Team> GetProjectTeams(int projectId) => _teams.Where(t => t.ProjectId == projectId).OrderBy(t => t.Name).ToList();
    public IReadOnlyList<LeaveRequest> GetUserLeaves(int userId) => _leaves.Where(l => l.ApplicantId == userId).OrderByDescending(l => l.CreatedOn).ToList();

    public PagedResult<UserListItemViewModel> GetUsersPage(string? search, string? role, int page, int pageSize)
    {
        var query = _users.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(u =>
                u.Username.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                u.FirstName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                u.LastName.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            query = query.Where(u => string.Equals(GetRoleName(u.RoleId), role, StringComparison.OrdinalIgnoreCase));
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
                RoleName = GetRoleName(u.RoleId),
                TeamName = GetTeamName(u.TeamId)
            })
            .ToList();

        return new PagedResult<UserListItemViewModel> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }

    public PagedResult<RoleSummaryViewModel> GetRolesPage(int page, int pageSize)
    {
        var total = _roles.Count;
        var items = _roles
            .OrderBy(r => r.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new RoleSummaryViewModel
            {
                Role = r,
                UserCount = _users.Count(u => u.RoleId == r.Id)
            })
            .ToList();

        return new PagedResult<RoleSummaryViewModel> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }

    public PagedResult<TeamListItemViewModel> GetTeamsPage(string? search, int page, int pageSize)
    {
        var query = _teams.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(t =>
                t.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                GetProjectName(t.ProjectId).Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        var total = query.Count();
        var items = query
            .OrderBy(t => t.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TeamListItemViewModel
            {
                Team = t,
                ProjectName = GetProjectName(t.ProjectId),
                TeamLeadName = GetUserName(t.TeamLeadId),
                MembersCount = _users.Count(u => u.TeamId == t.Id)
            })
            .ToList();

        return new PagedResult<TeamListItemViewModel> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }

    public PagedResult<ProjectListItemViewModel> GetProjectsPage(string? search, int page, int pageSize)
    {
        var query = _projects.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p =>
                p.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                p.Description.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        var total = query.Count();
        var items = query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProjectListItemViewModel
            {
                Project = p,
                TeamCount = _teams.Count(t => t.ProjectId == p.Id)
            })
            .ToList();

        return new PagedResult<ProjectListItemViewModel> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }

    public PagedResult<LeaveListItemViewModel> GetLeavesPage(AppUser user, DateTime? createdAfter, int page, int pageSize)
    {
        var query = _leaves.Where(l => l.ApplicantId == user.Id);
        if (createdAfter.HasValue)
        {
            query = query.Where(l => l.CreatedOn.Date >= createdAfter.Value.Date);
        }

        var total = query.Count();
        var items = query
            .OrderByDescending(l => l.CreatedOn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ToLeaveListItem)
            .ToList();

        return new PagedResult<LeaveListItemViewModel> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }

    public PagedResult<LeaveListItemViewModel> GetPendingApprovalsPage(AppUser approver, int page, int pageSize)
    {
        var query = _leaves.Where(l => !l.IsApproved && CanApprove(approver, l));
        var total = query.Count();
        var items = query
            .OrderBy(l => l.StartDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ToLeaveListItem)
            .ToList();

        return new PagedResult<LeaveListItemViewModel> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }

    public LeaveListItemViewModel ToLeaveListItem(LeaveRequest leave)
    {
        return new LeaveListItemViewModel
        {
            Leave = leave,
            Applicant = GetUser(leave.ApplicantId)!,
            ApproverName = GetUserName(leave.ApprovedById)
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

        var applicant = GetUser(leave.ApplicantId);
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
        role.Id = _roleId++;
        _roles.Add(role);
    }

    public bool UpdateRole(Role role)
    {
        var existing = GetRole(role.Id);
        if (existing is null)
        {
            return false;
        }

        existing.Name = role.Name.Trim();
        return true;
    }

    public bool DeleteRole(int id)
    {
        if (_users.Any(u => u.RoleId == id))
        {
            return false;
        }

        var role = GetRole(id);
        return role is not null && _roles.Remove(role);
    }

    public void CreateUser(UserFormViewModel form)
    {
        _users.Add(new AppUser
        {
            Id = _userId++,
            Username = form.Username.Trim(),
            Password = form.Password.Trim(),
            FirstName = form.FirstName.Trim(),
            LastName = form.LastName.Trim(),
            RoleId = form.RoleId,
            TeamId = form.TeamId
        });
    }

    public bool UpdateUser(UserFormViewModel form)
    {
        if (!form.Id.HasValue)
        {
            return false;
        }

        var user = GetUser(form.Id.Value);
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
        return true;
    }

    public bool DeleteUser(int id)
    {
        var user = GetUser(id);
        if (user is null)
        {
            return false;
        }

        foreach (var team in _teams.Where(t => t.TeamLeadId == id))
        {
            team.TeamLeadId = null;
        }

        _leaves.RemoveAll(l => l.ApplicantId == id || l.ApprovedById == id);
        return _users.Remove(user);
    }

    public void CreateProject(ProjectFormViewModel form)
    {
        _projects.Add(new Project
        {
            Id = _projectId++,
            Name = form.Name.Trim(),
            Description = form.Description.Trim()
        });
    }

    public bool UpdateProject(ProjectFormViewModel form)
    {
        if (!form.Id.HasValue)
        {
            return false;
        }

        var project = GetProject(form.Id.Value);
        if (project is null)
        {
            return false;
        }

        project.Name = form.Name.Trim();
        project.Description = form.Description.Trim();
        return true;
    }

    public bool DeleteProject(int id)
    {
        var project = GetProject(id);
        if (project is null)
        {
            return false;
        }

        foreach (var team in _teams.Where(t => t.ProjectId == id))
        {
            team.ProjectId = null;
        }

        return _projects.Remove(project);
    }

    public void CreateTeam(TeamFormViewModel form)
    {
        var team = new Team
        {
            Id = _teamId++,
            Name = form.Name.Trim(),
            ProjectId = form.ProjectId,
            TeamLeadId = form.TeamLeadId
        };

        _teams.Add(team);
        SyncTeamMembers(team.Id, form.MemberIds, form.TeamLeadId);
    }

    public bool UpdateTeam(TeamFormViewModel form)
    {
        if (!form.Id.HasValue)
        {
            return false;
        }

        var team = GetTeam(form.Id.Value);
        if (team is null)
        {
            return false;
        }

        team.Name = form.Name.Trim();
        team.ProjectId = form.ProjectId;
        team.TeamLeadId = form.TeamLeadId;
        SyncTeamMembers(team.Id, form.MemberIds, form.TeamLeadId);
        return true;
    }

    public bool DeleteTeam(int id)
    {
        var team = GetTeam(id);
        if (team is null)
        {
            return false;
        }

        foreach (var user in _users.Where(u => u.TeamId == id))
        {
            user.TeamId = null;
        }

        return _teams.Remove(team);
    }

    private void SyncTeamMembers(int teamId, List<int> memberIds, int? teamLeadId)
    {
        var intendedMembers = memberIds.Distinct().ToHashSet();
        if (teamLeadId.HasValue)
        {
            intendedMembers.Add(teamLeadId.Value);
        }

        foreach (var user in _users)
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
    }

    public void CreateLeave(AppUser applicant, LeaveFormViewModel form, IFormFile? attachment)
    {
        var leave = new LeaveRequest
        {
            Id = _leaveId++,
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

        _leaves.Add(leave);
    }

    public bool UpdateLeave(AppUser applicant, LeaveFormViewModel form, IFormFile? attachment)
    {
        if (!form.Id.HasValue)
        {
            return false;
        }

        var leave = GetLeave(form.Id.Value);
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

        return true;
    }

    public bool DeleteLeave(AppUser applicant, int id)
    {
        var leave = GetLeave(id);
        return leave is not null && leave.ApplicantId == applicant.Id && !leave.IsApproved && _leaves.Remove(leave);
    }

    public bool ApproveLeave(AppUser approver, int id)
    {
        var leave = GetLeave(id);
        if (leave is null || leave.IsApproved || !CanApprove(approver, leave))
        {
            return false;
        }

        leave.IsApproved = true;
        leave.ApprovedById = approver.Id;
        return true;
    }

    public IReadOnlyList<SelectListItem> RoleOptions(int? selected = null) =>
        _roles.OrderBy(r => r.Name)
            .Select(r => new SelectListItem(r.Name, r.Id.ToString(), selected == r.Id))
            .ToList();

    public IReadOnlyList<SelectListItem> TeamOptions(int? selected = null)
    {
        var items = new List<SelectListItem> { new("No team", string.Empty, !selected.HasValue) };
        items.AddRange(_teams.OrderBy(t => t.Name).Select(t => new SelectListItem(t.Name, t.Id.ToString(), selected == t.Id)));
        return items;
    }

    public IReadOnlyList<SelectListItem> ProjectOptions(int? selected = null)
    {
        var items = new List<SelectListItem> { new("No project", string.Empty, !selected.HasValue) };
        items.AddRange(_projects.OrderBy(p => p.Name).Select(p => new SelectListItem(p.Name, p.Id.ToString(), selected == p.Id)));
        return items;
    }

    public IReadOnlyList<SelectListItem> UserOptions(int? selected = null, Func<AppUser, bool>? predicate = null)
    {
        predicate ??= _ => true;
        var items = new List<SelectListItem> { new("None", string.Empty, !selected.HasValue) };
        items.AddRange(_users.Where(predicate)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Select(u => new SelectListItem($"{u.FullName} ({u.Username})", u.Id.ToString(), selected == u.Id)));
        return items;
    }

    private void Seed()
    {
        foreach (var roleName in AppRoles.All)
        {
            _roles.Add(new Role { Id = _roleId++, Name = roleName });
        }

        var ceoRoleId = _roles.First(r => r.Name == AppRoles.CEO).Id;
        var developerRoleId = _roles.First(r => r.Name == AppRoles.Developer).Id;
        var leadRoleId = _roles.First(r => r.Name == AppRoles.TeamLead).Id;
        var unassignedRoleId = _roles.First(r => r.Name == AppRoles.Unassigned).Id;

        _users.AddRange([
            new AppUser { Id = _userId++, Username = "ceo", Password = "ceo123", FirstName = "Elena", LastName = "Petrova", RoleId = ceoRoleId },
            new AppUser { Id = _userId++, Username = "lead1", Password = "lead123", FirstName = "Martin", LastName = "Dimitrov", RoleId = leadRoleId },
            new AppUser { Id = _userId++, Username = "lead2", Password = "lead123", FirstName = "Nina", LastName = "Koleva", RoleId = leadRoleId },
            new AppUser { Id = _userId++, Username = "dev1", Password = "dev123", FirstName = "Ivan", LastName = "Georgiev", RoleId = developerRoleId },
            new AppUser { Id = _userId++, Username = "dev2", Password = "dev123", FirstName = "Mira", LastName = "Stoicheva", RoleId = developerRoleId },
            new AppUser { Id = _userId++, Username = "dev3", Password = "dev123", FirstName = "Petar", LastName = "Nikolov", RoleId = developerRoleId },
            new AppUser { Id = _userId++, Username = "newhire", Password = "welcome1", FirstName = "Raya", LastName = "Ilieva", RoleId = unassignedRoleId }
        ]);

        _projects.AddRange([
            new Project { Id = _projectId++, Name = "Client Portal", Description = "Customer-facing portal for vacation requests and team visibility." },
            new Project { Id = _projectId++, Name = "HR Core", Description = "Internal administration, reporting and employee operations." }
        ]);

        _teams.AddRange([
            new Team { Id = _teamId++, Name = "Platform Team", ProjectId = _projects[0].Id, TeamLeadId = _users[1].Id },
            new Team { Id = _teamId++, Name = "People Ops", ProjectId = _projects[1].Id, TeamLeadId = _users[2].Id }
        ]);

        _users[1].TeamId = _teams[0].Id;
        _users[2].TeamId = _teams[1].Id;
        _users[3].TeamId = _teams[0].Id;
        _users[4].TeamId = _teams[0].Id;
        _users[5].TeamId = _teams[1].Id;

        _leaves.AddRange([
            new LeaveRequest
            {
                Id = _leaveId++,
                ApplicantId = _users[3].Id,
                Type = LeaveType.Paid,
                StartDate = DateTime.Today.AddDays(5),
                EndDate = DateTime.Today.AddDays(7),
                CreatedOn = DateTime.Today.AddDays(-2),
                IsHalfDay = false,
                IsApproved = false
            },
            new LeaveRequest
            {
                Id = _leaveId++,
                ApplicantId = _users[4].Id,
                Type = LeaveType.Unpaid,
                StartDate = DateTime.Today.AddDays(12),
                EndDate = DateTime.Today.AddDays(12),
                CreatedOn = DateTime.Today.AddDays(-1),
                IsHalfDay = true,
                IsApproved = true,
                ApprovedById = _users[1].Id
            }
        ]);
    }
}
