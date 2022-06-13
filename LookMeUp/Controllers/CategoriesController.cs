#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LookMeUp.Data;
using LookMeUp.Models;
using Microsoft.AspNetCore.Authorization;
using LookMeUp.Enumes;
using Microsoft.AspNetCore.Identity;
using LookMeUp.Models.ViewModels;
using LookMeUp.Services.Interfaces;

namespace LookMeUp.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILookMeUpEmailSender _lookMeUpEmailSender;

        public CategoriesController(ApplicationDbContext context, UserManager<AppUser> userManager, ILookMeUpEmailSender lookMeUpEmailSender)
        {
            _context = context;
            _userManager = userManager;
            _lookMeUpEmailSender = lookMeUpEmailSender;
        }

        // GET: Categories
        [Authorize]
        public async Task<IActionResult> Index()
        {
            string userId = _userManager.GetUserId(User);

            List<Category> categories = await _context.Categories.Where(c => c.AppUserID == userId).Include(c => c.AppUser).ToListAsync();


            return View(categories);
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Category category = await _context.Categories
                .Include(c => c.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

       
        [HttpGet]
        public async Task<IActionResult> EmailCategory(int? id)
        {
            Category category = await _context.Categories.Include(c => c.Contacts).FirstOrDefaultAsync(c => c.Id == id);

            List<string> emails = category.Contacts.Select(c => c.Email).ToList();

            EmailData emailData = new()
            {
                GroupName = category.Name,
                EmailAddress = string.Join(";", emails),
                Subject = $"Group Message: - {category.Name}"
            };

            EmailCategoryViewModel model = new()
            {
                Contacts = category.Contacts.ToList(),
                EmailData = emailData
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmailCategory(EmailData emailData)
        {
            if (ModelState.IsValid)
            {
                AppUser appUser = await _userManager.GetUserAsync(User);
                string emailBody = _lookMeUpEmailSender.CompseEmailBody(appUser, emailData);

                await _lookMeUpEmailSender.SendEmailAsync(emailData.EmailAddress, emailData.Subject, emailBody);

                return RedirectToAction("Index", "Categories");
            };
            return View();
        }

        // GET: Categories/Create
        [Authorize]
        public IActionResult Create()
        {
            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>().ToList());
            return View();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Category category)
        {
            ModelState.Remove("AppUserId");

            if (ModelState.IsValid)
            {
                category.AppUserID = _userManager.GetUserId(User);
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // GET: Categories/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }   
            
            Category category = await _context.Categories.FindAsync(id);

            category.AppUserID = _userManager.GetUserId(User);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id, Name")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    category.AppUserID = _userManager.GetUserId(User);
                    _context.Update(category);
                    await _context.SaveChangesAsync();

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
