using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MSH.Infrastructure.Data;

namespace MSH.Web.Pages.Auth;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager,
        ILogger<LoginModel> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        _logger.LogInformation("Login page accessed. ReturnUrl: {ReturnUrl}", returnUrl);
        
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            _logger.LogWarning("Error message present: {ErrorMessage}", ErrorMessage);
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        returnUrl ??= Url.Content("~/");
        ReturnUrl = returnUrl;
        _logger.LogInformation("Final ReturnUrl: {ReturnUrl}", ReturnUrl);
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        _logger.LogInformation("Login POST request received. ReturnUrl: {ReturnUrl}", returnUrl);
        
        returnUrl ??= Url.Content("~/");
        _logger.LogInformation("Login attempt for email: {Email}", Input.Email);

        if (!ModelState.IsValid)
        {
            var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            _logger.LogWarning("ModelState invalid. Errors: {Errors}", errors);
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);
        _logger.LogInformation("PasswordSignInAsync result: Succeeded={Succeeded}, RequiresTwoFactor={RequiresTwoFactor}, IsLockedOut={IsLockedOut}", 
            result.Succeeded, result.RequiresTwoFactor, result.IsLockedOut);

        if (result.Succeeded)
        {
            _logger.LogInformation("User logged in successfully. Redirecting to: {ReturnUrl}", returnUrl);
            return LocalRedirect(returnUrl);
        }
        if (result.RequiresTwoFactor)
        {
            _logger.LogInformation("Two factor authentication required");
            return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
        }
        if (result.IsLockedOut)
        {
            _logger.LogWarning("User account locked out");
            return RedirectToPage("./Lockout");
        }
        else
        {
            _logger.LogWarning("Invalid login attempt for user: {Email}", Input.Email);
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }
    }
} 