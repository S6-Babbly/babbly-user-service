using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace babbly_user_service.Services
{
    public class AuthorizationService
    {
        private readonly ILogger<AuthorizationService> _logger;

        public AuthorizationService(ILogger<AuthorizationService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Checks if the user is authorized to perform the specified operation on the resource
        /// </summary>
        /// <param name="userId">The user's ID</param>
        /// <param name="roles">The user's roles</param>
        /// <param name="resourcePath">The resource path being accessed</param>
        /// <param name="operation">The operation being performed (read, write, delete)</param>
        /// <returns>True if authorized, false otherwise</returns>
        public async Task<bool> IsAuthorizedAsync(string userId, List<string> roles, string resourcePath, string operation)
        {
            // Log the authorization request
            _logger.LogInformation(
                "Authorization check for user {UserId} with roles [{Roles}] on resource {Resource} for operation {Operation}", 
                userId, string.Join(", ", roles), resourcePath, operation);

            // Admin roles have full access
            if (roles.Contains("admin"))
            {
                return true;
            }

            // For user resources, users can access their own data
            if (resourcePath.StartsWith("/api/users/") && resourcePath.Contains(userId))
            {
                return true;
            }

            // Default role-based checks
            return operation.ToLower() switch
            {
                "read" => roles.Contains("user") || roles.Contains("editor"),
                "write" => roles.Contains("editor"),
                "delete" => false, // Only admins can delete by default
                _ => false
            };
        }
    }
} 