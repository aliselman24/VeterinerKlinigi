using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VeterinerKlinigi.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class RegisterConfirmationModel : PageModel
{
    [TempData]
    public bool RegistrationCompleted { get; set; }

    [TempData]
    public string? ConfirmationLink { get; set; }

    public IActionResult OnGet()
    {
        if (!RegistrationCompleted)
        {
            return RedirectToPage("./Register");
        }

        // Doğrulama bağlantısını yalnızca kayıt yapan tarayıcıda gösteriyorum.
        TempData.Keep(nameof(RegistrationCompleted));
        TempData.Keep(nameof(ConfirmationLink));
        return Page();
    }
}
