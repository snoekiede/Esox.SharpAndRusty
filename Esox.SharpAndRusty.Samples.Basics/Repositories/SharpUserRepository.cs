using Esox.SharpAndRusty.Samples.Basics.Models;
using Esox.SharpAndRusty.Types;
using Esox.SharpAndRusty.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Esox.SharpAndRusty.Samples.Basics.Repositories
{
    internal class SharpUserRepository
    {
        private List<User> _users = new List<User>();

        public SharpUserRepository()
        {
            _users = new();
        }

        public Result<Unit,Error> AddUser(User user)
        {
            if (_users.All(x => x.Name != user.Name))
            {
                _users.Add(user);
                return Result<Unit,Error>.Ok();
            }
            else
            {
                return Error.New("User already exists.");
            }
        }

        public Result<User, Error> GetUserByName(string name)
        {
            var user = _users.FirstOrDefault(x => x.Name == name);
            if (user != null)
            {
                return user;
            }
            else
            {
                return Error.New("User not found.");
            }
        }

        public Result<User, Error> GetUserById(int id)
        {
            var user = _users.FirstOrDefault(x => x.Id == id);
            if (user != null)
            {
                return user;
            }
            else
            {
                return Error.New("User not found.");
            }
        }

        
    }
}
