using DummyProject.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Security.Claims;

namespace DummyProject.Controllers
{
    public class DummyController : Controller
    {
        private readonly IConfiguration _configuration;
        public DummyController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            // Validate the user's credentials by checking the database
            bool isValidUser = ValidateUser(model.email, model.password);

            if (isValidUser)
            {
                // Create claims
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, model.email),
            new Claim(ClaimTypes.Email, model.email)
        };

                // Create identity
                var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuthenticationScheme");

                // Create principal
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // Sign in the user with cookie authentication
                await HttpContext.SignInAsync("MyCookieAuthenticationScheme", claimsPrincipal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true, // Make the authentication cookie persistent
                        ExpiresUtc = DateTime.UtcNow.AddMinutes(30) // Expiration time for the cookie
                    });

                // Add custom authentication cookie
                Response.Cookies.Append("IsAuthenticated", "true", new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddMinutes(2), // Set the expiration time for the custom cookie
                    HttpOnly = true, // Prevents JavaScript access
                    Secure = true // Use only in HTTPS
                });

                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.ErrorMessage = "Invalid login attempt.";
                return View();
            }
        }



        private bool ValidateUser(string email, string password)
        {
            bool isValid = false;
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("usp_ValidateUser", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@Password", password);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                isValid = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred: " + ex.Message;
            }

            return isValid;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {           
            await HttpContext.SignOutAsync("MyCookieAuthenticationScheme"); 
            Response.Cookies.Delete("IsAuthenticated");
            return RedirectToAction("Login", "Dummy");
        }


        public ActionResult SignUp()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Signup(LoginModel model)
        {
            
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            bool isSignedUp = false;
            
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("usp_UserSignup", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Email", model.email);
                        cmd.Parameters.AddWithValue("@Password", model.password);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        isSignedUp = rowsAffected > 0;
                    }
                }

                if (isSignedUp)
                {                    
                    return RedirectToAction("Login");
                }
                else
                {
                    ViewBag.ErrorMessage = "Signup failed. Please try again.";
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred: " + ex.Message;
                return View();
            }
        }

       
    }
}
