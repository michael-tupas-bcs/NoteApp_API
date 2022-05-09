using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NoteApp_API.Data;
using NoteApp_API.Services.UserService;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace NoteApp_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        public readonly DataContext _usercontext;
        public static UserModel.UserData user = new UserModel.UserData();
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
         
        public AuthController(IConfiguration configuration, IUserService userService, DataContext dataContext) 
        {
            _configuration = configuration;
            _userService = userService;
            _usercontext = dataContext;
        }

        [HttpGet, Authorize]
        public ActionResult<string> GetMe()
        { 
            var userName = _userService.GetMyName();
            return Ok(userName);

            //var userName = User?.Identity?.Name;
            //var userName2 = User?.FindFirstValue(ClaimTypes.Name);
            //var role = User?.FindFirstValue(ClaimTypes.Role);

            //return Ok(new { userName, userName2, role});
        }


        [HttpPost("register")]
        public async Task<ActionResult<UserModel.UserData>> Register(UserModel.UserDto request)
        {
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            user.UserName = request.UserName;
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _usercontext.Users.Add(user);
            await _usercontext.SaveChangesAsync();
            //return Ok(await _usercontext.Users.ToListAsync());
            return Ok("Registration Successful.");
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserModel.UserDto request)
        {
            var dbUser = await _usercontext.Users.FirstOrDefaultAsync(x => x.UserName == request.UserName);

            if (dbUser.UserName != request.UserName)
            {
                return BadRequest("User not found.");
            }

            if (!VerifyPasswordHash(request.Password, dbUser.PasswordHash, dbUser.PasswordSalt))
            {
                return BadRequest("Wrong password.");
            }
            user.Id = dbUser.Id;
            string token = CreateToken(user);
            return Ok(token);
        }

        private string CreateToken(UserModel.UserData user) 
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Id.ToString()),
                new Claim(ClaimTypes.Role, "Admin"),
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value));

            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: cred);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            { 
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
    }
}
