using babbly_user_service.Data;
using babbly_user_service.DTOs;
using babbly_user_service.Models;
using babbly_user_service.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace babbly_user_service.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserService _userService;
        private readonly KafkaProducerService _kafkaProducer;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            ApplicationDbContext context,
            UserService userService,
            KafkaProducerService kafkaProducer,
            ILogger<UsersController> logger)
        {
            _context = context;
            _userService = userService;
            _kafkaProducer = kafkaProducer;
            _logger = logger;
        }

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return users.Select(u => MapUserToDto(u)).ToList();
        }

        // GET: api/users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return MapUserToDto(user);
        }

        // GET: api/users/auth0/auth0|123456789
        [HttpGet("auth0/{auth0Id}")]
        public async Task<ActionResult<UserDto>> GetUserByAuth0Id(string auth0Id)
        {
            var user = await _userService.GetUserByAuth0IdAsync(auth0Id);

            if (user == null)
            {
                return NotFound();
            }

            return MapUserToDto(user);
        }

        // POST: api/users
        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto createUserDto)
        {
            // Check if user with the same Auth0Id already exists
            if (await _context.Users.AnyAsync(u => u.Auth0Id == createUserDto.Auth0Id))
            {
                return Conflict("A user with this Auth0Id already exists.");
            }

            // Check if username is already taken
            if (await _context.Users.AnyAsync(u => u.Username == createUserDto.Username))
            {
                return Conflict("Username is already taken.");
            }

            // Check if email is already taken
            if (await _context.Users.AnyAsync(u => u.Email == createUserDto.Email))
            {
                return Conflict("Email is already taken.");
            }

            var user = new User
            {
                Auth0Id = createUserDto.Auth0Id,
                Username = createUserDto.Username,
                Email = createUserDto.Email,
                Role = createUserDto.Role,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Create UserExtraData if provided
            if (createUserDto.ExtraData != null)
            {
                user.ExtraData = new UserExtraData
                {
                    DisplayName = createUserDto.ExtraData.DisplayName,
                    ProfilePicture = createUserDto.ExtraData.ProfilePicture,
                    Bio = createUserDto.ExtraData.Bio,
                    Address = createUserDto.ExtraData.Address,
                    PhoneNumber = createUserDto.ExtraData.PhoneNumber,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
            }

            user = await _userService.CreateUserAsync(user);

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, MapUserToDto(user));
        }

        // PUT: api/users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDto updateUserDto)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Check if username is already taken
            if (updateUserDto.Username != null && 
                updateUserDto.Username != user.Username && 
                await _context.Users.AnyAsync(u => u.Username == updateUserDto.Username))
            {
                return Conflict("Username is already taken.");
            }

            // Check if email is already taken
            if (updateUserDto.Email != null && 
                updateUserDto.Email != user.Email && 
                await _context.Users.AnyAsync(u => u.Email == updateUserDto.Email))
            {
                return Conflict("Email is already taken.");
            }

            // Update user fields
            if (updateUserDto.Username != null)
                user.Username = updateUserDto.Username;
            
            if (updateUserDto.Email != null)
                user.Email = updateUserDto.Email;
            
            if (updateUserDto.Role != null)
                user.Role = updateUserDto.Role;
            
            // Update UserExtraData if provided
            if (updateUserDto.ExtraData != null)
            {
                if (user.ExtraData == null)
                {
                    // Create extra data if it doesn't exist
                    user.ExtraData = new UserExtraData
                    {
                        CreatedAt = DateTime.UtcNow,
                    };
                }

                // Update extra data fields
                if (updateUserDto.ExtraData.DisplayName != null)
                    user.ExtraData.DisplayName = updateUserDto.ExtraData.DisplayName;
                
                if (updateUserDto.ExtraData.ProfilePicture != null)
                    user.ExtraData.ProfilePicture = updateUserDto.ExtraData.ProfilePicture;
                
                if (updateUserDto.ExtraData.Bio != null)
                    user.ExtraData.Bio = updateUserDto.ExtraData.Bio;
                
                if (updateUserDto.ExtraData.Address != null)
                    user.ExtraData.Address = updateUserDto.ExtraData.Address;
                
                if (updateUserDto.ExtraData.PhoneNumber != null)
                    user.ExtraData.PhoneNumber = updateUserDto.ExtraData.PhoneNumber;
            }

            try
            {
                await _userService.UpdateUserAsync(user);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await UserExistsAsync(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        private async Task<bool> UserExistsAsync(int id)
        {
            return await _userService.GetUserByIdAsync(id) != null;
        }

        private static UserDto MapUserToDto(User user)
        {
            var userDto = new UserDto
            {
                Id = user.Id,
                Auth0Id = user.Auth0Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };

            if (user.ExtraData != null)
            {
                userDto.ExtraData = new UserExtraDataDto
                {
                    Id = user.ExtraData.Id,
                    UserId = user.ExtraData.UserId,
                    DisplayName = user.ExtraData.DisplayName,
                    ProfilePicture = user.ExtraData.ProfilePicture,
                    Bio = user.ExtraData.Bio,
                    Address = user.ExtraData.Address,
                    PhoneNumber = user.ExtraData.PhoneNumber,
                    CreatedAt = user.ExtraData.CreatedAt,
                    UpdatedAt = user.ExtraData.UpdatedAt
                };
            }

            return userDto;
        }

        /// <summary>
        /// Creates or updates a user profile based on Auth0 information
        /// </summary>
        [HttpPost("profile")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateOrUpdateFromAuth0([FromBody] Auth0UserProfile profile)
        {
            if (string.IsNullOrEmpty(profile.Auth0Id))
            {
                return BadRequest(new { error = "Auth0Id is required" });
            }

            if (string.IsNullOrEmpty(profile.Email))
            {
                return BadRequest(new { error = "Email is required" });
            }

            // Get the user ID from the claims (passed by the gateway)
            var userId = Request.Headers["X-User-Id"].FirstOrDefault();
            
            // Verify that the authenticated user is creating their own profile
            if (string.IsNullOrEmpty(userId) || userId != profile.Auth0Id)
            {
                return Unauthorized(new { error = "Cannot create or update another user's profile" });
            }
            
            // Get roles from the request headers
            var roles = Request.Headers["X-User-Roles"].FirstOrDefault()?.Split(',') ?? Array.Empty<string>();
            
            try
            {
                // Check if user already exists
                var existingUser = await _context.Users
                    .Include(u => u.ExtraData)
                    .FirstOrDefaultAsync(u => u.Auth0Id == profile.Auth0Id);
                
                if (existingUser != null)
                {
                    _logger.LogInformation("Updating existing user: {Auth0Id}", profile.Auth0Id);
                    
                    // Update user fields
                    existingUser.Email = profile.Email;
                    existingUser.Username = profile.Username ?? profile.Email.Split('@')[0];
                    existingUser.FirstName = profile.FirstName ?? string.Empty;
                    existingUser.LastName = profile.LastName ?? string.Empty;
                    existingUser.UpdatedAt = DateTime.UtcNow;
                    
                    // Update or create extra data
                    if (existingUser.ExtraData == null && !string.IsNullOrEmpty(profile.Picture))
                    {
                        existingUser.ExtraData = new UserExtraData
                        {
                            DisplayName = profile.FullName ?? profile.FirstName ?? string.Empty,
                            ProfilePicture = profile.Picture,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                    }
                    else if (existingUser.ExtraData != null)
                    {
                        if (!string.IsNullOrEmpty(profile.FullName))
                            existingUser.ExtraData.DisplayName = profile.FullName;
                        if (!string.IsNullOrEmpty(profile.Picture))
                            existingUser.ExtraData.ProfilePicture = profile.Picture;
                        existingUser.ExtraData.UpdatedAt = DateTime.UtcNow;
                    }
                    
                    // Update role if admin role is present in Auth0 claims
                    if (roles.Contains("admin") && existingUser.Role != "Admin")
                    {
                        existingUser.Role = "Admin";
                    }
                    
                    await _context.SaveChangesAsync();
                    
                    // Publish user updated event to Kafka
                    await _kafkaProducer.PublishUserUpdatedEventAsync(existingUser);
                    
                    _logger.LogInformation("Successfully updated user: {UserId}, {Auth0Id}", existingUser.Id, existingUser.Auth0Id);
                    return Ok(existingUser);
                }
                else
                {
                    _logger.LogInformation("Creating new user: {Auth0Id}", profile.Auth0Id);
                    
                    // Create new user
                    var newUser = new User
                    {
                        Auth0Id = profile.Auth0Id,
                        Email = profile.Email,
                        Username = profile.Username ?? profile.Email.Split('@')[0],
                        FirstName = profile.FirstName ?? string.Empty,
                        LastName = profile.LastName ?? string.Empty,
                        Role = roles.Contains("admin") ? "Admin" : "User",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    
                    // Create extra data if we have picture or display name
                    if (!string.IsNullOrEmpty(profile.Picture) || !string.IsNullOrEmpty(profile.FullName))
                    {
                        newUser.ExtraData = new UserExtraData
                        {
                            DisplayName = profile.FullName ?? profile.FirstName ?? string.Empty,
                            ProfilePicture = profile.Picture ?? string.Empty,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                    }
                    
                    _context.Users.Add(newUser);
                    await _context.SaveChangesAsync();
                    
                    // Publish user created event to Kafka
                    await _kafkaProducer.PublishUserCreatedEventAsync(newUser);
                    
                    _logger.LogInformation("Successfully created new user: {UserId}, {Auth0Id}", newUser.Id, newUser.Auth0Id);
                    return Ok(newUser);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating or updating user from Auth0: {Auth0Id}", profile.Auth0Id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { error = "Error creating or updating user profile" });
            }
        }

        /// <summary>
        /// Gets the current user's profile
        /// </summary>
        [HttpGet("me")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCurrentUser()
        {
            // Get the user ID from the claims (passed by the gateway)
            var userId = Request.Headers["X-User-Id"].FirstOrDefault();
            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User is not authenticated");
            }
            
            try
            {
                var user = await _context.Users
                    .Include(u => u.ExtraData)
                    .FirstOrDefaultAsync(u => u.Auth0Id == userId);
                    
                if (user == null)
                {
                    return NotFound("User not found");
                }
                
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current user");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving current user");
            }
        }

        /// <summary>
        /// Updates the current user's profile
        /// </summary>
        [HttpPut("profile")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateUserProfileDto profileDto)
        {
            // Get the user ID from the claims (passed by the gateway)
            var userId = Request.Headers["X-User-Id"].FirstOrDefault();
            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { error = "Authentication required. User ID not found in token." });
            }
            
            try
            {
                var user = await _context.Users
                    .Include(u => u.ExtraData)
                    .FirstOrDefaultAsync(u => u.Auth0Id == userId);
                    
                if (user == null)
                {
                    return NotFound(new { error = "User not found" });
                }
                
                // Update basic user fields
                if (!string.IsNullOrEmpty(profileDto.Username))
                {
                    // Check if username is already taken by another user
                    var existingUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.Username == profileDto.Username && u.Auth0Id != userId);
                    if (existingUser != null)
                    {
                        return BadRequest(new { error = "Username is already taken" });
                    }
                    user.Username = profileDto.Username;
                }
                
                if (!string.IsNullOrEmpty(profileDto.Email))
                {
                    user.Email = profileDto.Email;
                }
                
                // Update or create extra data
                if (user.ExtraData == null)
                {
                    user.ExtraData = new UserExtraData
                    {
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                }
                
                if (!string.IsNullOrEmpty(profileDto.DisplayName))
                {
                    user.ExtraData.DisplayName = profileDto.DisplayName;
                }
                
                if (!string.IsNullOrEmpty(profileDto.ProfilePicture))
                {
                    user.ExtraData.ProfilePicture = profileDto.ProfilePicture;
                }
                
                if (!string.IsNullOrEmpty(profileDto.Address))
                {
                    user.ExtraData.Address = profileDto.Address;
                }
                
                if (!string.IsNullOrEmpty(profileDto.PhoneNumber))
                {
                    user.ExtraData.PhoneNumber = profileDto.PhoneNumber;
                }
                
                user.UpdatedAt = DateTime.UtcNow;
                user.ExtraData.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Updated profile for user {UserId}", userId);
                
                return Ok(MapUserToDto(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { error = "Error updating user profile" });
            }
        }

        // GET: api/users/search?term={searchTerm}
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<UserDto>>> SearchUsers([FromQuery] string term, [FromQuery] int skip = 0, [FromQuery] int take = 50)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return await GetUsers();
            }

            var users = await _userService.SearchUsersAsync(term, skip, take);
            return users.Select(u => MapUserToDto(u)).ToList();
        }
    }
} 