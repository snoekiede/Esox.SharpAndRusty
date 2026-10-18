using System;
using System.Collections.Generic;
using System.Text;

namespace Esox.SharpAndRusty.Samples.Basics.Models
{
    internal class User
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }

        public override string ToString()
        {
            return $"Id: {Id}, Name: {Name}, Email: {Email}";
        }

        public string Greet()
        {
            return $"Hello, my name is {Name}";
        }
    }
}
