using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookShop.Areas.Identity.Controllers
{
    [Route("[Area]/[controller]")]
    [ApiController]
    [Area("Identity")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IRepository<ApplicationUserOTP> _applicationUserOTPRepository;
        public AccountController(UserManager<ApplicationUser> userManager, IEmailSender emailSender, SignInManager<ApplicationUser> signInManager, IRepository<ApplicationUserOTP> applicationUserOTPRepository)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _signInManager = signInManager;
            _applicationUserOTPRepository = applicationUserOTPRepository;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterRequest registerRequest)
        {

            var user = new ApplicationUser()
            {
                FirstName = registerRequest.FirstName,
                LastName = registerRequest.LastName,
                Email = registerRequest.Email,
                UserName = registerRequest.UserName,
            };

            var result = await _userManager.CreateAsync(user, registerRequest.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // Send Confirmation Mail
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action(nameof(ConfirmEmail), "Account", new { area = "Identity", token, userId = user.Id }, Request.Scheme);

            await _emailSender.SendEmailAsync(registerRequest.Email, "Ecommerce 519 - Confirm Your Email!"
                , $"<h1>Confirm Your Email By Clicking <a href='{link}'>Here</a></h1>");

            await _userManager.AddToRoleAsync(user, SD.CUSTOMER_ROLE);

            return Ok(new
            {
                msg = "Registration Successful, Please Confirm Your Email"
            });
        }

        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
                return NotFound(new ErrorModelResponse
                {
                    Code = "User Not Found",
                    Description = "User Not Found"
                });

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
                return BadRequest(new ErrorModelResponse
                {
                    Code = "Email Confirmation Failed",
                    Description = "Email Confirmation Failed, Please Try Again"
                });
            else
                return Ok(new
                {
                    msg = "Email Confirmed Successfully"
                });

        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {
            var user = await _userManager.FindByNameAsync(loginRequest.UserNameOREmail) ?? await _userManager.FindByEmailAsync(loginRequest.UserNameOREmail);

            if (user is null)
            {
                return NotFound(new
                {
                    msg = "Invalid User Name / Email OR Password"
                });
            }

            var result = await _signInManager.PasswordSignInAsync(user, loginRequest.Password, loginRequest.RememberMe, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                    return BadRequest(new ErrorModelResponse
                    {
                        Code = "Too many Attemps",
                        Description = "User Account Locked Out. Please Try Again Later"
                    });
                else if (!user.EmailConfirmed)
                    return BadRequest(new ErrorModelResponse
                    {
                        Code = "Email Not Confirmed",
                        Description = "Please Confirm Your Email First!!"
                    });
                else
                    return BadRequest(new ErrorModelResponse
                    {
                        Code = "Invalid Credentials",
                        Description = "Invalid User Name / Email OR Password"
                    });
            }

            return Ok(new
            {
                msg = "Login Successful"
            });
        }
        
        [HttpPost("ResendEmailConfirmation")]
        public async Task<IActionResult> ResendEmailConfirmation(ResendEmailConfirmationRequest resendEmailConfirmationRequest)
        {

            var user = await _userManager.FindByNameAsync(resendEmailConfirmationRequest.UserNameOREmail) ?? await _userManager.FindByEmailAsync(resendEmailConfirmationRequest.UserNameOREmail);

            if (user is null)
            {
                return NotFound(new ErrorModelResponse
                {
                    Code = "User Not Found",
                    Description = "Invalid User Name / Email"
                });
            }

            if (user.EmailConfirmed)
            {
                return BadRequest(new ErrorModelResponse
                {
                    Code = "Email Already Confirmed",
                    Description = "Email Already Confirmed"
                });
            }

            // Send Confirmation Mail
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action(nameof(ConfirmEmail), "Account", new { area = "Identity", token, userId = user.Id }, Request.Scheme);

            await _emailSender.SendEmailAsync(user.Email!, "Ecommerce 519 - Resend Confirm Your Email!"
                , $"<h1>Confirm Your Email By Clicking <a href='{link}'>Here</a></h1>");

            return Ok(new
            {
                msg = "Confirmation Email Sent. Please Confirm Your Email"
            });
        }
        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordRequest forgetPasswordRequest, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByNameAsync(forgetPasswordRequest.UserNameOREmail) ?? await _userManager.FindByEmailAsync(forgetPasswordRequest.UserNameOREmail);

            if (user is null)
            {
                return NotFound(new ErrorModelResponse
                {
                    Code = "User Not Found",
                    Description = "Invalid User Name / Email"
                });
            }

            var userOTPs = await _applicationUserOTPRepository.GetAsync(e => e.ApplicationUserId == user.Id);

            var totalOTPs = userOTPs.Count(e => (DateTime.UtcNow - e.CreateAt).TotalHours < 24);

            if (totalOTPs > 3)
            {
                return BadRequest(new ErrorModelResponse
                {
                    Code = "OTP Limit Reached",
                    Description = "You Have Reached The Limit Of OTP Requests."
                });
            }

            var otp = new Random().Next(1000, 9999).ToString(); // 1000 - 9999

            await _applicationUserOTPRepository.AddAsync(new()
            {
                Id = Guid.NewGuid().ToString(),
                ApplicationUserId = user.Id,
                CreateAt = DateTime.UtcNow,
                IsValid = true,
                OTP = otp,
                ValidTo = DateTime.UtcNow.AddDays(1),
            });
            await _applicationUserOTPRepository.CommitAsync(cancellationToken);

            await _emailSender.SendEmailAsync(user.Email!, "Ecommerce 519 - Reset Your Password"
                , $"<h1>Use This OTP: {otp} To Reset Your Account. Don't share it.</h1>");

            return CreatedAtAction(nameof(ValidateOTP), new { userId = user.Id }); 
        }
        [HttpPost("ValidateOTP")]
        public async Task<IActionResult> ValidateOTP(ValidateOTPRequest validateOTPRequest)
        {
            var result = await _applicationUserOTPRepository.GetOneAsync(e => e.ApplicationUserId == validateOTPRequest.ApplicationUserId && e.OTP == validateOTPRequest.OTP && e.IsValid);

            if (result is null)
            {
                return CreatedAtAction(nameof(ValidateOTP), new { userId = validateOTPRequest.ApplicationUserId });            
            }

            return CreatedAtAction(nameof(NewPassword), new { userId = validateOTPRequest.ApplicationUserId });
        }
        [HttpPost("NewPassword")]
        public async Task<IActionResult> NewPassword(NewPasswordRequest newPasswordRequest)
        {
            var user = await _userManager.FindByIdAsync(newPasswordRequest.ApplicationUserId);

            if (user is null)
            {
                return NotFound(new ErrorModelResponse
                {
                    Code = "User Not Found",
                    Description = "User Not Found"
                });
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var result = await _userManager.ResetPasswordAsync(user, token, newPasswordRequest.Password);


            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok();
        }
    }
}