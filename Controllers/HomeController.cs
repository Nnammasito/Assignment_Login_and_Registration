using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Assignment_Login_and_Registration.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;


namespace Assignment_Login_and_Registration.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly MyContext _context;

    public HomeController(ILogger<HomeController> logger, MyContext context)
    {
        _logger = logger;
        _context = context;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        MyViewModel MyModels = new MyViewModel();
        return View("Index", MyModels);
    }

    [HttpGet("success")]
    // The name we gave our class minus "Attribute"
    [SessionCheck]
    // The rest of the code

    public IActionResult Success()
    {
        return View("Success");
    }
    [HttpPost("users/create")]
    public IActionResult RegistrationUser(User user)
    {
        if (ModelState.IsValid)
        {
            PasswordHasher<User> Hasher = new PasswordHasher<User>();   
            // Updating our newUser's password to a hashed version         
            user.Password = Hasher.HashPassword(user, user.Password); 
            _context.Users.Add(user);
            _context.SaveChanges();
            HttpContext.Session.SetInt32("UserId", user.UserId);
            return RedirectToAction("Success");
        }
        else
        {
            MyViewModel MyModels = new MyViewModel();
            MyModels.User = user;
            return View("Index", MyModels);
        }
    }

    [HttpPost("users/login")]
    public IActionResult Login(Login userSubmission)
    {
        if (ModelState.IsValid)
        {
            // If initial ModelState is valid, query for a user with the provided email        
            User? userInDb = _context.Users.FirstOrDefault(u => u.Email == userSubmission.EmailLogin);
            // If no user exists with the provided email        
            if (userInDb == null)
            {
                // Add an error to ModelState and return to View!            
                ModelState.AddModelError("Email", "Invalid Email/Password");
                MyViewModel viewModel = new MyViewModel();
                viewModel.Login = userSubmission;
                return View("Index", viewModel);
            }
            // Otherwise, we have a user, now we need to check their password                 
            // Initialize hasher object        
            PasswordHasher<Login> hasher = new PasswordHasher<Login>();
            // Verify provided password against hash stored in db        
            var result = hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.Password);                                    // Result can be compared to 0 for failure        
            if (result == 0)
            {
                // Handle failure (this should be similar to how "existing email" is handled)        
                MyViewModel viewModel = new MyViewModel();
                viewModel.Login = userSubmission;
                return View("Index", viewModel);
            }
            // Surrounding registration code
            HttpContext.Session.SetInt32("UserId", userInDb.UserId);
            return RedirectToAction("Success");
        }
        else
        {
            MyViewModel viewModel = new MyViewModel();
            viewModel.Login = userSubmission;
            return View("Index", viewModel);
        }
    }

    [HttpGet("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index");
    }

    [HttpGet("privacy")]
    public IActionResult Privacy()
    {
        return View("Privacy");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
// Name this anything you want with the word "Attribute" at the end
public class SessionCheckAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Find the session, but remember it may be null so we need int?
        int? userId = context.HttpContext.Session.GetInt32("UserId");
        // Check to see if we got back null
        if (userId == null)
        {
            // Redirect to the Index page if there was nothing in session
            // "Home" here is referring to "HomeController", you can use any controller that is appropriate here
            context.Result = new RedirectToActionResult("Index", "Home", null);
        }
    }
}

