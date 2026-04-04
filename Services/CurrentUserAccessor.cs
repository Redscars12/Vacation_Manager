using System.Security.Claims;
using Vacation_Manager.Models;

namespace Vacation_Manager.Services;

public sealed class CurrentUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AppRepository _repository;

    public CurrentUserAccessor(IHttpContextAccessor httpContextAccessor, AppRepository repository)
    {
        _httpContextAccessor = httpContextAccessor;
        _repository = repository;
    }

    public AppUser? GetCurrentUser()
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        var idValue = principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idValue, out var id) ? _repository.GetUser(id) : null;
    }
}
