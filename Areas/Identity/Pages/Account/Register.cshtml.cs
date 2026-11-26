using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VeterinerKlinigi.Data;
using VeterinerKlinigi.Models;

namespace VeterinerKlinigi.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class RegisterModel(
    ApplicationDbContext database,
    UserManager<IdentityUser> users,
    IWebHostEnvironment environment,
    IEmailSender emailSender) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    [TempData]
    public bool RegistrationCompleted { get; set; }

    [TempData]
    public string? ConfirmationLink { get; set; }

    public string ReturnUrl { get; private set; } = "/";

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl! : Url.Content("~/");
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl! : Url.Content("~/");
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var email = Input.Email.Trim();
        var user = new IdentityUser { UserName = email, Email = email };

        try
        {
            // Kullanıcı ve rol birlikte kaydedilir. Rol ataması başarısızsa kayıt geri alınır.
            await using var transaction = await database.Database.BeginTransactionAsync();
            var createResult = await users.CreateAsync(user, Input.Password);
            if (!createResult.Succeeded)
            {
                var message = createResult.Errors.Any(error => error.Code.StartsWith("Password", StringComparison.Ordinal))
                    ? "Parolan kayıt kurallarını karşılamıyor."
                    : "Bu e-posta adresiyle hesap oluşturulamadı.";
                ModelState.AddModelError(string.Empty, message);
                return Page();
            }

            var roleResult = await users.AddToRoleAsync(user, RoleNames.Member);
            if (!roleResult.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Üyelik tamamlanamadı. Tekrar dene.");
                return Page();
            }

            await transaction.CommitAsync();
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqliteException { SqliteExtendedErrorCode: 2067 })
        {
            ModelState.AddModelError(string.Empty, "Bu e-posta adresiyle hesap oluşturulamadı.");
            return Page();
        }

        var token = await users.GenerateEmailConfirmationTokenAsync(user);
        var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var verificationUrl = Url.Page(
            "/Account/ConfirmEmail",
            pageHandler: null,
            values: new { area = "Identity", userId = user.Id, code, returnUrl = ReturnUrl },
            protocol: Request.Scheme)
            ?? throw new InvalidOperationException("E-posta doğrulama adresi oluşturulamadı.");

        if (environment.IsDevelopment())
        {
            ConfirmationLink = verificationUrl;
        }
        else
        {
            await emailSender.SendEmailAsync(
                email,
                "E-posta adresini doğrula",
                $"E-posta adresini doğrulamak için <a href=\"{HtmlEncoder.Default.Encode(verificationUrl)}\">bağlantıyı aç</a>.");
        }

        RegistrationCompleted = true;
        return RedirectToPage("./RegisterConfirmation");
    }

    public class InputModel
    {
        [Required(ErrorMessage = "E-posta adresini gir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi gir.")]
        [StringLength(256)]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Parolanı gir.")]
        [StringLength(128, MinimumLength = 6, ErrorMessage = "Parola 6 ile 128 karakter arasında olmalı.")]
        [DataType(DataType.Password)]
        [Display(Name = "Parola")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Parolanı tekrar gir.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Parolalar eşleşmiyor.")]
        [Display(Name = "Parola tekrar")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
