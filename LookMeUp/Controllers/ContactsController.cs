﻿#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LookMeUp.Data;
using LookMeUp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using LookMeUp.Enumes;
using LookMeUp.Services.Interfaces;
using LookMeUp.Services;

namespace LookMeUp.Controllers
{
    public class ContactsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILookMeUpService _lookMeUpService;
        private readonly IImageService _iimageService;
        private readonly SearchService _searchService;

        public ContactsController(ApplicationDbContext context, UserManager<AppUser> userManager, ILookMeUpService lookMeUpService, IImageService iimageService, SearchService searchService)
        {
            _context = context;
            _userManager = userManager;
            _lookMeUpService = lookMeUpService;
            _iimageService = iimageService;
            _searchService = searchService;
        }

        // GET: Contacts
        [Authorize]
        public IActionResult Index(int id)
        {
            List<Contact> contacts = new List<Contact>();

            string appUserId = _userManager.GetUserId(User);
            AppUser appUser = _context.Users
                .Include(c => c.Contacts)
                .ThenInclude(c => c.Categories)
                .FirstOrDefault(u => u.Id == appUserId);
            
            if(id == 0)
            {
                contacts = appUser.Contacts.ToList();
            } else
            {
                contacts = appUser.Categories.FirstOrDefault(c => c.Id == id).Contacts.ToList();
            }

            ViewData["CategoryId"] = new SelectList(appUser.Categories, "Id", "Name", id);

            return View(contacts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SearchContacts(string searchString)
        {
            var userId = _userManager.GetUserId(User);
            List<Contact> contacts = _searchService.SearchContacts(searchString, userId).ToList();

            return View(nameof(Index), contacts);
        }

        // GET: Contacts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Contact contact = await _context.Contacts
                .Include(c => c.AppUser)
                .Include(c=> c.Categories)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // GET: Contacts/Create
        [Authorize]
        public async Task<IActionResult> Create()
        {
            string appUserId = _userManager.GetUserId(User);

            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>().ToList());
            ViewData["CategoryList"] = new MultiSelectList(await _lookMeUpService.GetUserCategoriesAsync(appUserId), "Id", "Name");
            return View();
        }

        // POST: Contacts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,Birthdate,Address1,Address2,City,State,ZipCode,Email,PhoneNumber,ImageType")] Contact contact, List<int> categoryList)
        {
            ModelState.Remove("AppUserId");

            if (ModelState.IsValid)
            {
              
                contact.AppUserId = _userManager.GetUserId(User);
                contact.Created = DateTime.SpecifyKind(DateTime.Now,DateTimeKind.Utc);

                if(contact.Birthdate != null)
                {
                    contact.Birthdate = DateTime.SpecifyKind(contact.Birthdate.Value, DateTimeKind.Utc);
                }
                if(contact.ImageFile != null)
                {
                    //TODO: Image Service
                    contact.ImageData = await _iimageService.ConvertFileToByteArrayAsync(contact.ImageFile);
                    contact.ImageType = contact.ImageFile.ContentType;
                }

                _context.Add(contact);
                await _context.SaveChangesAsync();

                //Add Contact to Categories
                foreach(int categoryId in categoryList)
                {
                    await _lookMeUpService.AddContactToCategoryAsync(categoryId, contact.Id);
                }
                

                return RedirectToAction(nameof(Index));
            }
            string appUserId = _userManager.GetUserId(User);

            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>().ToList());
            ViewData["CategoryList"] = new MultiSelectList(await _lookMeUpService.GetUserCategoriesAsync(appUserId), "Id", "Name");
            return View(contact);
        }

        // GET: Contacts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Contact contact = await _context.Contacts.FindAsync(id);
            if (contact == null)
            {
                return NotFound();
            }

            string appUserId = _userManager.GetUserId(User);

            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>().ToList());
            ViewData["CategoryList"] = new MultiSelectList(await _lookMeUpService.GetUserCategoriesAsync(appUserId), "Id", "Name", await _lookMeUpService.GetContactCategoryIdsAsync(contact.Id));
            return View(contact);
        }

        // POST: Contacts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id, AppUserId, FirstName,LastName,Birthdate,Address1,Address2,City,State,ZipCode,Email,PhoneNumber, Created, ImageFile, ImageData, ImageType")] Contact contact, List<int> categoryList)
        {
            if (id != contact.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    contact.Created = DateTime.SpecifyKind(contact.Created, DateTimeKind.Utc);

                    if (contact.Birthdate != null)
                    {
                        contact.Birthdate = DateTime.SpecifyKind(contact.Birthdate.Value, DateTimeKind.Utc);
                    }
                    if (contact.ImageFile != null)
                    {
                        //TODO: Image Service
                        contact.ImageData = await _iimageService.ConvertFileToByteArrayAsync(contact.ImageFile);
                        contact.ImageType = contact.ImageFile.ContentType;
                    }

                    _context.Update(contact);
                    await _context.SaveChangesAsync();

                    List<Category> oldCategories = (await _lookMeUpService.GetContactCategoriesAsync(contact.Id)).ToList();
                    foreach(Category category in oldCategories)
                    {
                        await _lookMeUpService.AddContactToCategoryAsync(category.Id, contact.Id);
                    }

                    foreach (int categoryId in categoryList)
                    {
                        await _lookMeUpService.RemoveContactFromCategoryAsync(categoryId, contact.Id);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContactExists(contact.Id))
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

            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>().ToList());
            return View(contact);
        }

        // GET: Contacts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Contact contact = await _context.Contacts
                .Include(c => c.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // POST: Contacts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Contact contact = await _context.Contacts.FindAsync(id);
            _context.Contacts.Remove(contact);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContactExists(int id)
        {
            return _context.Contacts.Any(e => e.Id == id);
        }
    }
}