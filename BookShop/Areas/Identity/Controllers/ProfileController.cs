using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookShop.Areas.Identity.Controllers
{
    [Route("[area]/[controller]")]
    [ApiController]
    [Authorize]
    [Area("Identity")]
    public class ProfileController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public ProfileController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetInfo()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
                return NotFound();

            //var userVM = new ApplicationUserVM()
            //{
            //    //FullName = user.FirstName + " " + user.LastName,
            //    FullName = $"{user.FirstName} {user.LastName}",
            //    Address = user.Address,
            //    Email = user.Email,
            //    PhoneNumber = user.PhoneNumber
            //};

            var userVM = user.Adapt<ApplicationUserResponse>();

            return Ok(userVM);
        }

        [HttpPut("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile(ApplicationUserRequest applicationUserRequest)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
                return NotFound();

            var names = applicationUserRequest.FullName.Split(" ");

            user.PhoneNumber = applicationUserRequest.PhoneNumber;
            user.Address = applicationUserRequest.Address;
            user.FirstName = names[0];
            user.LastName = names[1];

            await _userManager.UpdateAsync(user);

            return Ok(new
            {
                msg = "Profile Updated"
            });
        }
        [HttpPut("UpdatePassword")]
        public async Task<IActionResult> UpdatePassword(ApplicationUserRequest applicationUserResponse)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
                return NotFound();

            if (applicationUserResponse.CurrentPassword is null || applicationUserResponse.NewPassword is null)
            {
                return BadRequest(new ErrorModelResponse
                {
                    Code = "Invalid Data",
                    Description = "Current Password and New Password are required"
                });
            }

            var result = await _userManager.ChangePasswordAsync(user, applicationUserResponse.CurrentPassword, applicationUserResponse.NewPassword);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new
            {
                msg = "Password Updated"
            });
        }
    }
}
