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
            var users = await _context.Users.ToListAsync();
            return users.Select(u => MapUserToDto(u)).ToList();
        }

        // GET: api/users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

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
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Auth0Id == auth0Id);

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
                DisplayName = createUserDto.DisplayName,
                ProfilePicture = createUserDto.ProfilePicture,
                Bio = createUserDto.Bio,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, MapUserToDto(user));
        }

        // PUT: api/users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDto updateUserDto)
        {
            var user = await _context.Users.FindAsync(id);
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

            // Update only the provided fields
            if (updateUserDto.Username != null)
                user.Username = updateUserDto.Username;
            
            if (updateUserDto.Email != null)
                user.Email = updateUserDto.Email;
            
            if (updateUserDto.DisplayName != null)
                user.DisplayName = updateUserDto.DisplayName;
            
            if (updateUserDto.ProfilePicture != null)
                user.ProfilePicture = updateUserDto.ProfilePicture;
            
            if (updateUserDto.Bio != null)
                user.Bio = updateUserDto.Bio;
            
            user.UpdatedAt = DateTime.UtcNow;

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
            return new UserDto
            {
                Id = user.Id,
                Auth0Id = user.Auth0Id,
                Username = user.Username,
                Email = user.Email,
                DisplayName = user.DisplayName,
                ProfilePicture = user.ProfilePicture,
                Bio = user.Bio,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
    }
} 