using Microsoft.EntityFrameworkCore;
using Shop.Entities;

namespace Shop.Users;

public class UserService
{
    private readonly ShopContext _shopContext;

    public UserService(ShopContext shopContext)
    {
        _shopContext = shopContext;
    }

    public async Task<User> CreateIfNeeded(string name)
    {
        var user = await _shopContext.Users.AsNoTracking().FirstOrDefaultAsync(i => i.Name == name);
        if (user == null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Name = name
            };
            _shopContext.Users.Add(user);
            await _shopContext.SaveChangesAsync();
        }

        return user;
    }
}