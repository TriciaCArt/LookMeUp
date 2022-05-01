using LookMeUp.Models;

namespace LookMeUp.Services.Interfaces
{
    public interface ILookMeUpService
    {
        Task AddContactToCategoryAsync(int categoryId, int contatId);
        Task<bool> IsContactInCategory(int categoryId, int contactId);
        Task<IEnumerable<Category>> GetUserCategoriesAsync(string UserId);
        Task<ICollection<int>> GetContactCategoryIdsAsync(int contactId);
        Task<ICollection<Category>> GetContactCategoriesAsync(int contactId);
        Task RemoveContactFromCategoryAsync(int categoryId, int contactId);
       
    }
}
