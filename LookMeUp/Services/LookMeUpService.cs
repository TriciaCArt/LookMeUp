﻿using LookMeUp.Data;
using LookMeUp.Models;
using LookMeUp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LookMeUp.Services
{
    public class LookMeUpService : ILookMeUpService
    {
        private readonly ApplicationDbContext _context;

        public LookMeUpService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddContactToCategoryAsync(int categoryId, int contactId)
        {
            try
            {
                if (!await IsContactInCategory(categoryId, contactId))
                {
                    Contact? contact = await _context.Contacts.FindAsync(contactId);
                    Category? category = await _context.Categories.FindAsync(categoryId);

                    if (category != null && contact != null)
                    {
                        category.Contacts.Add(contact);
                        await _context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
            
        }

       

        public async Task<ICollection<Category>> GetContactCategoriesAsync(int contactId)
        {
            try
            {
                Contact? contact = await _context.Contacts.Include(c=>c.Categories).FirstOrDefaultAsync(c=>c.Id == contactId);
                return contact.Categories;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<ICollection<int>> GetContactCategoryIdsAsync(int contactId)
        {
            try
            {
                List<int> categoryIds = (await GetContactCategoriesAsync(contactId)).Select(c=>c.Id).ToList();
                return categoryIds;
            }
            catch (Exception)
            {

                throw;
            }
            
        } 


        public async Task<IEnumerable<Category>> GetUserCategoriesAsync(string UserId)
        {
            List<Category> categories = new List<Category>();

            try
            {
                categories = await _context.Categories.Where(c => c.AppUserID == UserId).ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
            return categories;
        }

        public async Task<bool> IsContactInCategory(int categoryId, int contactId)
        {
            try
            {
                Contact? contact = await _context.Contacts.FindAsync(contactId);
                return await _context.Categories
                                     .Include(c => c.Contacts)
                                     .Where(c => c.Id == categoryId && c.Contacts.Contains(contact))
                                     .AnyAsync();
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        public async Task RemoveContactFromCategoryAsync(int categoryId, int contactId)
        {
            try
            {
                if (await IsContactInCategory(categoryId, contactId))
                {
                    Contact? contact = await _context.Contacts.FindAsync(contactId);
                    Category? category = await _context.Categories.FindAsync(categoryId);

                    if (category != null && contact != null)
                    {
                        category.Contacts.Remove(contact);
                        await _context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
