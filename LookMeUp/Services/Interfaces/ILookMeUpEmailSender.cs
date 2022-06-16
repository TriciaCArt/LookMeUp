using LookMeUp.Models;
using LookMeUp.Models.ViewModels;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace LookMeUp.Services.Interfaces
{
    public interface ILookMeUpEmailSender : IEmailSender
    {
        string ComposeEmailBody(AppUser sender, EmailData emailData);
        Task SendEmailAsync(AppUser appUser, List<Contact> contact, EmailData emailData);

    }
}
