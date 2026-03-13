using System;
using System.Collections.Generic;
using System.Linq;
using FestivalManagementSystem.Interfaces;
using FestivalManagementSystem.Models;

namespace FestivalManagementSystem.Data
{
    // Temporary in-memory repository used before the MySQL stage.
    public class InMemoryPersonRepository : IPersonRepository
    {
        private readonly List<Person> _people = new();
        private int _nextId = 1;

        public void Add(Person person)
        {
            if (EmailExists(person.Email))
                throw new Exception("Email already exists.");

            person.SetId(_nextId++);
            _people.Add(person);
        }

        public List<Person> GetAll()
        {
            // Returns a copy so the caller cannot directly replace the internal list.
            return new List<Person>(_people);
        }

        public Person GetById(int id)
        {
            return _people.FirstOrDefault(p => p.PersonId == id);
        }

        public void Update(Person person)
        {
            Person existing = GetById(person.PersonId);

            if (existing == null)
                throw new Exception("Person not found.");

            int index = _people.FindIndex(p => p.PersonId == person.PersonId);
            _people[index] = person;
        }

        public void Delete(int id)
        {
            Person person = GetById(id);

            if (person == null)
                throw new Exception("Person not found.");

            _people.Remove(person);
        }

        public bool EmailExists(string email)
        {
            return _people.Any(p => p.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        public bool EmailExistsExcept(string email, int excludedPersonId)
        {
            return _people.Any(p =>
                p.PersonId != excludedPersonId &&
                p.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }
    }
}