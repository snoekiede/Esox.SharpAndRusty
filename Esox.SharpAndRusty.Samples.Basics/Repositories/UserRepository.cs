using System;
using System.Collections.Generic;
using System.Text;
using Esox.SharpAndRusty.Samples.Basics.Models;

namespace Esox.SharpAndRusty.Samples.Basics.Repositories;

internal class UserRepository
{
    private List<User> _users = new List<User>();

    public UserRepository()
    {
        _users = new();
    }

    public void AddUser(User user)
    {
        if (_users.All(x=>x.Name != user.Name))
        {
            _users.Add(user);
        }
        else
        {
            throw new InvalidOperationException("User already exists.");
        }
    }

    public User? GetUserByName(string name)
    {
        return _users.FirstOrDefault(x => x.Name == name);
    }

    public User? GetUserById(int id)
    {
        return _users.FirstOrDefault(x => x.Id == id);
    }
}
