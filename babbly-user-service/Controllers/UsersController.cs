using babbly_user_service.Data;
using babbly_user_service.DTOs;
using babbly_user_service.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace babbly_user_service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(ApplicationDbContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _context.Users.Include(u => u.ExtraData).ToListAsync();
            return users.Select(u => MapUserToDto(u)).ToList();
        }

        // GET: api/users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _context.Users.Include(u => u.ExtraData).FirstOrDefaultAsync(u => u.Id == id);

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
            var user = await _context.Users.Include(u => u.ExtraData).FirstOrDefaultAsync(u => u.Auth0Id == auth0Id);

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

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create UserExtraData if provided
            if (createUserDto.ExtraData != null)
            {
                var extraData = new UserExtraData
                {
                    UserId = user.Id,
                    DisplayName = createUserDto.ExtraData.DisplayName,
                    ProfilePicture = createUserDto.ExtraData.ProfilePicture,
                    Bio = createUserDto.ExtraData.Bio,
                    Address = createUserDto.ExtraData.Address,
                    PhoneNumber = createUserDto.ExtraData.PhoneNumber,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.UserExtraData.Add(extraData);
                await _context.SaveChangesAsync();
                
                // Reload user with extra data
                user = await _context.Users.Include(u => u.ExtraData).FirstOrDefaultAsync(u => u.Id == user.Id);
            }

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, MapUserToDto(user));
        }

        // PUT: api/users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDto updateUserDto)
        {
            var user = await _context.Users.Include(u => u.ExtraData).FirstOrDefaultAsync(u => u.Id == id);
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
            
            user.UpdatedAt = DateTime.UtcNow;

            // Update UserExtraData if provided
            if (updateUserDto.ExtraData != null)
            {
                if (user.ExtraData == null)
                {
                    // Create extra data if it doesn't exist
                    user.ExtraData = new UserExtraData
                    {
                        UserId = user.Id,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.UserExtraData.Add(user.ExtraData);
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
                
                user.ExtraData.UpdatedAt = DateTime.UtcNow;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
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
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
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
    }
} 