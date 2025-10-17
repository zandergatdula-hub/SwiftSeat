using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SwiftSeat.Controllers
{
    public class AccountController : Controller
    {

        private readonly IConfiguration _configuration;

        public AccountController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public IActionResult Login()  // GET: /Account/Login
        {
            string username = _configuration["username"];
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string userName, string password, string returnUrl)
        {
            if (userName == _configuration["username"] && password == _configuration["password"]) // to validate username and password
            {
                var claims = new List<Claim> // creating a list of claims
                {
                    new Claim(ClaimTypes.NameIdentifier, "admin"), // the unique ID 
                    new Claim(ClaimTypes.Name,"admin123") // human readable name 
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme); // creating the indentity for claims 

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, // Sign-In the user with the cookie authentication scheme 
                    new ClaimsPrincipal(claimsIdentity));

                if (!string.IsNullOrEmpty(returnUrl)) // Just for fixing the the url when it's null 
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    return RedirectToAction("Index", "Home"); // redirect to the home page
                }
            }
            ViewBag.ErrorMessage = "Invalid username or password";
            return RedirectToAction("Index", "Home");

        }

            public IActionResult Logout()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogoutConfirmed()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }

    }
  }
