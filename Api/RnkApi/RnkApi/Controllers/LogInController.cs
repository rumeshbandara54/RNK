using login.model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RnkApi.Context;
using System.Text.RegularExpressions;
using System.Text;
using login.helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using login.model.Dto;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using login.UtilityService;

namespace RnkApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogInController : Controller
    {
        private readonly appDbContext _appDbContext;
        //forget password send mail
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        public LogInController(appDbContext appDbContext, IConfiguration configuration, IEmailService emailService)
        {
            _appDbContext = appDbContext;
            //forget password send mail
            _configuration = configuration;
            _emailService = emailService;

        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] User userObj)
        {
            if (userObj == null)

                return BadRequest();

            var user = await _appDbContext.Users.FirstOrDefaultAsync(x => x.username == userObj.username);
            if (user == null)

                return NotFound(new { Message = "User Not Found!" });

            if (!PasswordHasher.VerifyPassword(userObj.password, user.password))
            {
                return BadRequest(new { Message = "Password is incorrect" });
            }

            user.token = CreateJwt(user);
            var newAccessToken = user.token;
            var newRefreshToken = CreateRefreshToken();
            user.refreshToken = newRefreshToken;
            //refresh token
            user.refreshTokenExpiryTime = DateTime.Now.AddDays(5);
            await _appDbContext.SaveChangesAsync();

            return Ok(new TokenApiDto()
            {
                accessToken = newAccessToken,
                refreshToken = newRefreshToken
            });






        }

        [HttpPost("registor")]
        public async Task<IActionResult> RegisterUser([FromBody] User userObj)
        {
            if (userObj == null)

                return BadRequest();

            //check username
            if (await checkUserNameExistAsync(userObj.username))
                return BadRequest(new { Message = "UserName Already Exist" });


            //check mail
            if (await checkEmailExistAsync(userObj.email))
                return BadRequest(new { Message = "Email Already Exist" });

            //check password strength
            var pass = checkPasswordStrength(userObj.password);
            if (!string.IsNullOrEmpty(pass))
                return BadRequest(new { Message = pass.ToString() });


            // encript password
            userObj.password = PasswordHasher.HashPassword(userObj.password);


            userObj.role = "User";
            userObj.token = "";
            await _appDbContext.Users.AddAsync(userObj);
            await _appDbContext.SaveChangesAsync();
            return Ok(new
            {
                Message = "User Registered!"
            });

        }

        private Task<bool> checkUserNameExistAsync(string userName)
            => _appDbContext.Users.AnyAsync(x => x.username == userName);

        private Task<bool> checkEmailExistAsync(string Email)
            => _appDbContext.Users.AnyAsync(x => x.email == Email);

        private string checkPasswordStrength(string password)
        {
            StringBuilder sd = new StringBuilder();

            if (password.Length <= 8)
                sd.Append("minimum password length should be 8" + Environment.NewLine);

            if (!(Regex.IsMatch(password, "[a-z]") && Regex.IsMatch(password, "[A-Z]")
                && Regex.IsMatch(password, "[0-9]")))
                sd.Append("password should be Alphanumeric" + Environment.NewLine);

            if (!Regex.IsMatch(password, "[<,>,@,!,@,#,$,%,^,&,*,{,},+,=]"))
                sd.Append("password should special chars" + Environment.NewLine);

            return sd.ToString();


        }

        private string CreateJwt(User user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("veryverysceret.....");
            var identity = new ClaimsIdentity(new Claim[]
            {
                new Claim (ClaimTypes.Role, user.role),
                new Claim(ClaimTypes.Name,$"{user.username}")
            });

            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                //dashboard gatta data thiyena time eka 
                Subject = identity,
                Expires = DateTime.Now.AddSeconds(10),
                SigningCredentials = credentials
            };
            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            return jwtTokenHandler.WriteToken(token);


        }

        //refresh token 
        private string CreateRefreshToken()
        {
            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var refreshToken = Convert.ToBase64String(tokenBytes);

            var tokenInUser = _appDbContext.Users
               .Any(a => a.refreshToken == refreshToken);
            if (tokenInUser)
            {
                return CreateRefreshToken();
            }
            return refreshToken;
        }



        private ClaimsPrincipal GetPrincipleFromExpiredToken(string token)
        {
            var key = Encoding.ASCII.GetBytes("veryverysceret.....");
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = false
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("This is Invalid Token");
            return principal;

        }
        //new Access Token
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] TokenApiDto tokenApiDto)
        {
            if (tokenApiDto is null)
                return BadRequest("Invalid Client Request");
            string accessToken = tokenApiDto.accessToken;
            string refreshToken = tokenApiDto.refreshToken;
            var principal = GetPrincipleFromExpiredToken(accessToken);
            var username = principal.Identity.Name;
            var user = await _appDbContext.Users.FirstOrDefaultAsync(u => u.username == username);
            if (user is null || user.refreshToken != refreshToken || user.refreshTokenExpiryTime <= DateTime.Now)
                return BadRequest("Invalid Request");
            var newAccessToken = CreateJwt(user);
            var newRefreshToken = CreateRefreshToken();
            user.refreshToken = newRefreshToken;
            await _appDbContext.SaveChangesAsync();
            return Ok(new TokenApiDto()
            {
                accessToken = newAccessToken,
                refreshToken = newRefreshToken,
            });
        }



        //protect api

        [HttpGet]
        public async Task<ActionResult<User>> GetAllUsers()
        {
            return Ok(await _appDbContext.Users.ToListAsync());
        }


        //forget password send mail

        [HttpPost("send-reset-email/{email}")]
        public async Task<IActionResult> SendEmail(string email)
        {
            var user = await _appDbContext.Users.FirstOrDefaultAsync(a => a.email == email);
            if (user is null)
            {
                return NotFound(new
                {
                    statusCode = 404,
                    Message = "email Doesnot Exit"

                });
            }
            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var emailToken = Convert.ToBase64String(tokenBytes);
            user.ResetPasswordToken = emailToken;
            user.ResetPasswordExpiry = DateTime.Now.AddMinutes(15);
            string from = _configuration["EmailSettings:From"];
            var emailModel = new EmailModel(email, "reset Password!!", EmailBody.EmailStringBody(email, emailToken));
            _emailService.SendEmail(emailModel);
            _appDbContext.Entry(user).State = EntityState.Modified;
            await _appDbContext.SaveChangesAsync();
            return Ok(new
            {
                StatusCode = 200,
                Message = "Email Sent!"
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            var newToken = resetPasswordDto.EmailToken.Replace(" ", "+");
            var user = await _appDbContext.Users.AsNoTracking().FirstOrDefaultAsync(a => a.email == resetPasswordDto.Email);
            if (user is null)
            {
                return NotFound(new
                {
                    statusCode = 404,
                    Message = "user Doesnot Exit"

                });
            }
            var tokenCode = user.ResetPasswordToken;
            DateTime emailTokenExpiry = user.ResetPasswordExpiry;
            if (tokenCode != resetPasswordDto.EmailToken || emailTokenExpiry < DateTime.Now)
            {
                return BadRequest(new
                {
                    StatusCode = 400,
                    Message = "Invalid Reset link"
                });
            }

            user.password = PasswordHasher.HashPassword(resetPasswordDto.NewPassword);
            _appDbContext.Entry(user).State = EntityState.Modified;
            await _appDbContext.SaveChangesAsync();
            return Ok(new
            {
                StatusCode = 200,
                Message = "Password Reset Succesfully"
            });
        }


    }

}
