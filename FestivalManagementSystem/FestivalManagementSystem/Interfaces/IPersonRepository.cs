using System.Collections.Generic;
using FestivalManagementSystem.Models;

namespace FestivalManagementSystem.Interfaces
{
    // Defines the data access operations for people in the system.
    public interface IPersonRepository
    {
        void Add(Person person);
        List<Person> GetAll();
        Person GetById(int id);
        void Update(Person person);
        void Delete(int id);
        bool EmailExists(string email);
        bool EmailExistsExcept(string email, int excludedPersonId);
    }
}