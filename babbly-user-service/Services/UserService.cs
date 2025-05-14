using babbly_user_service.Data;
using babbly_user_service.Models;
using Microsoft.EntityFrameworkCore;

namespace babbly_user_service.Services
{
    public class UserService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<UserService> _logger;

        public UserService(ApplicationDbContext dbContext, ILogger<UserService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Get a user by ID
        /// </summary>
        public async Task<User?> GetUserByIdAsync(int id)
        {
            try
            {
                return await _dbContext.Users
                    .Include(u => u.ExtraData)
                    .FirstOrDefaultAsync(u => u.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by ID: {UserId}", id);
                throw;
            }
        }

        /// <summary>
        /// Get a user by Auth0 ID
        /// </summary>
        public async Task<User?> GetUserByAuth0IdAsync(string auth0Id)
        {
            try
            {
                return await _dbContext.Users
                    .Include(u => u.ExtraData)
                    .FirstOrDefaultAsync(u => u.Auth0Id == auth0Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by Auth0 ID: {Auth0Id}", auth0Id);
                throw;
            }
        }

        /// <summary>
        /// Get a user by email
        /// </summary>
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            try
            {
                return await _dbContext.Users
                    .Include(u => u.ExtraData)
                    .FirstOrDefaultAsync(u => u.Email == email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by email: {Email}", email);
                throw;
            }
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        public async Task<User> CreateUserAsync(User user)
        {
            try
            {
                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user: {Email}", user.Email);
                throw;
            }
        }

        /// <summary>
        /// Update an existing user
        /// </summary>
        public async Task<User> UpdateUserAsync(User user)
        {
            try
            {
                user.UpdatedAt = DateTime.UtcNow;
                
                if (user.ExtraData != null)
                {
                    user.ExtraData.UpdatedAt = DateTime.UtcNow;
                }
                
                _dbContext.Users.Update(user);
                await _dbContext.SaveChangesAsync();
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {UserId}", user.Id);
                throw;
            }
        }

        /// <summary>
        /// Get all users
        /// </summary>
        public async Task<List<User>> GetAllUsersAsync(int skip = 0, int take = 50)
        {
            try
            {
                return await _dbContext.Users
                    .Include(u => u.ExtraData)
                    .OrderBy(u => u.Id)
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                throw;
            }
        }

        /// <summary>
        /// Search users by name or email
        /// </summary>
        public async Task<List<User>> SearchUsersAsync(string searchTerm, int skip = 0, int take = 50)
        {
            try
            {
                if (string.IsNullOrEmpty(searchTerm))
                {
                    return await GetAllUsersAsync(skip, take);
                }

                var normalizedSearchTerm = searchTerm.ToLower();

                return await _dbContext.Users
                    .Include(u => u.ExtraData)
                    .Where(u => 
                        u.Email.ToLower().Contains(normalizedSearchTerm) ||
                        u.Username.ToLower().Contains(normalizedSearchTerm) ||
                        u.FirstName.ToLower().Contains(normalizedSearchTerm) ||
                        u.LastName.ToLower().Contains(normalizedSearchTerm) ||
                        (u.ExtraData != null && u.ExtraData.DisplayName != null && 
                         u.ExtraData.DisplayName.ToLower().Contains(normalizedSearchTerm))
                    )
                    .OrderBy(u => u.Id)
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users: {SearchTerm}", searchTerm);
                throw;
            }
        }

        /// <summary>
        /// Delete a user
        /// </summary>
        public async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                var user = await _dbContext.Users.FindAsync(userId);
                
                if (user == null)
                {
                    return false;
                }

                _dbContext.Users.Remove(user);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserId}", userId);
                throw;
            }
        }
    }
} 