using System;

namespace FestivalManagementSystem.Models
{
    // Base class for all people managed by the festival system.
    public abstract class Person
    {
        private int _personId;
        private string _name;
        private string _email;
        private string _telephone;

        public int PersonId => _personId;

        // Allows the repository to assign a generated ID.
        public void SetId(int id) => _personId = id;

        public string Name
        {
            get => _name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Name cannot be empty.");
                _name = value.Trim();
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Email cannot be empty.");
                _email = value.Trim();
            }
        }

        public string Telephone
        {
            get => _telephone;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Telephone cannot be empty.");
                _telephone = value.Trim();
            }
        }

        protected Person(string name, string email, string telephone)
        {
            Name = name;
            Email = email;
            Telephone = telephone;
        }

        public abstract string GetRole();
        public abstract string GetDetails();
    }
}