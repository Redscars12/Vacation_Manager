using Vacation_Manager.Models;

namespace Vacation_Manager.Data;

public static class ApplicationDbInitializer
{
    public static void Seed(ApplicationDbContext db)
    {
        db.Database.EnsureCreated();

        if (db.Roles.Any())
        {
            return;
        }

        var roles = AppRoles.All
            .Select(name => new Role { Name = name })
            .ToList();
        db.Roles.AddRange(roles);
        db.SaveChanges();

        int roleId(string name) => db.Roles.Single(r => r.Name == name).Id;

        var users = new List<AppUser>
        {
            new() { Username = "ceo", Password = "ceo123", FirstName = "Elena", LastName = "Petrova", RoleId = roleId(AppRoles.CEO) },
            new() { Username = "lead1", Password = "lead123", FirstName = "Martin", LastName = "Dimitrov", RoleId = roleId(AppRoles.TeamLead) },
            new() { Username = "lead2", Password = "lead123", FirstName = "Nina", LastName = "Koleva", RoleId = roleId(AppRoles.TeamLead) },
            new() { Username = "dev1", Password = "dev123", FirstName = "Ivan", LastName = "Georgiev", RoleId = roleId(AppRoles.Developer) },
            new() { Username = "dev2", Password = "dev123", FirstName = "Mira", LastName = "Stoicheva", RoleId = roleId(AppRoles.Developer) },
            new() { Username = "dev3", Password = "dev123", FirstName = "Petar", LastName = "Nikolov", RoleId = roleId(AppRoles.Developer) },
            new() { Username = "newhire", Password = "welcome1", FirstName = "Raya", LastName = "Ilieva", RoleId = roleId(AppRoles.Unassigned) }
        };
        db.Users.AddRange(users);
        db.SaveChanges();

        var projects = new List<Project>
        {
            new() { Name = "Client Portal", Description = "Customer-facing portal for vacation requests and team visibility." },
            new() { Name = "HR Core", Description = "Internal administration, reporting and employee operations." }
        };
        db.Projects.AddRange(projects);
        db.SaveChanges();

        var teams = new List<Team>
        {
            new() { Name = "Platform Team", ProjectId = projects[0].Id, TeamLeadId = users[1].Id },
            new() { Name = "People Ops", ProjectId = projects[1].Id, TeamLeadId = users[2].Id }
        };
        db.Teams.AddRange(teams);
        db.SaveChanges();

        users[1].TeamId = teams[0].Id;
        users[2].TeamId = teams[1].Id;
        users[3].TeamId = teams[0].Id;
        users[4].TeamId = teams[0].Id;
        users[5].TeamId = teams[1].Id;

        db.LeaveRequests.AddRange(
            new LeaveRequest
            {
                ApplicantId = users[3].Id,
                Type = LeaveType.Paid,
                StartDate = DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(5), DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(7), DateTimeKind.Utc),
                CreatedOn = DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(-2), DateTimeKind.Utc),
                IsHalfDay = false,
                IsApproved = false
            },
            new LeaveRequest
            {
                ApplicantId = users[4].Id,
                Type = LeaveType.Unpaid,
                StartDate = DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(12), DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(12), DateTimeKind.Utc),
                CreatedOn = DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(-1), DateTimeKind.Utc),
                IsHalfDay = true,
                IsApproved = true,
                ApprovedById = users[1].Id
            });

        db.SaveChanges();
    }
}
