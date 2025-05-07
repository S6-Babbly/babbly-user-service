using babbly_user_service.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace babbly_user_service.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        protected readonly AuthorizationService _authService;
        protected readonly ILogger<BaseController> _logger;

        public BaseController(AuthorizationService authService, ILogger<BaseController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Get the current user's ID from claims
        /// </summary>
        protected string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        /// <summary>
        /// Get the user's roles from claims
        /// </summary>
        protected List<string> GetUserRoles()
        {
            return User.Claims
                .Where(c => c.Type == ClaimTypes.Role || c.Type == "https://babbly.com/roles")
                .Select(c => c.Value)
                .ToList();
        }

        /// <summary>
        /// Check if the current user is authorized for the given resource and operation
        /// </summary>
        protected async Task<bool> IsAuthorizedForResourceAsync(string resourcePath, string operation)
        {
            string userId = GetUserId();
            List<string> roles = GetUserRoles();

            return await _authService.IsAuthorizedAsync(userId, roles, resourcePath, operation);
        }

        /// <summary>
        /// Creates a forbidden result with an error message
        /// </summary>
        protected IActionResult Forbidden(string message = "You are not authorized to perform this action")
        {
            return StatusCode(403, new { error = message });
        }
    }
} 