using System;
using System.Collections.Generic;
using System.Linq;
using FestivalManagementSystem.Interfaces;
using FestivalManagementSystem.Models;

namespace FestivalManagementSystem.Services
{
    // Handles business logic and validation for all person records.
    public class PersonService
    {
        private readonly IPersonRepository _repository;

        public PersonService(IPersonRepository repository)
        {
            _repository = repository;
        }

        // Adds a new person after validating all business rules.
        public void AddPerson(Person person)
        {
            ValidatePerson(person, false);
            _repository.Add(person);
        }

        // Returns every record currently stored in the repository.
        public List<Person> GetAllPeople()
        {
            return _repository.GetAll();
        }

        // Returns records filtered by role.
        public List<Person> GetPeopleByRole(string role)
        {
            return _repository.GetAll()
                .Where(p => p.GetRole().Equals(role, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // Finds one person by their ID.
        public Person GetPersonById(int id)
        {
            return _repository.GetById(id);
        }

        // Updates an existing person after validation.
        public void UpdatePerson(Person person)
        {
            if (_repository.GetById(person.PersonId) == null)
                throw new Exception("Person ID not found.");

            ValidatePerson(person, true);
            _repository.Update(person);
        }

        // Deletes a person by ID.
        public void DeletePerson(int id)
        {
            if (_repository.GetById(id) == null)
                throw new Exception("Person ID not found.");

            _repository.Delete(id);
        }

        // Central place for business rules required by the coursework.
        private void ValidatePerson(Person person, bool isUpdate)
        {
            if (person == null)
                throw new ArgumentException("Person cannot be null.");

            if (string.IsNullOrWhiteSpace(person.Name))
                throw new ArgumentException("Name cannot be empty.");

            if (string.IsNullOrWhiteSpace(person.Email))
                throw new ArgumentException("Email cannot be empty.");

            if (string.IsNullOrWhiteSpace(person.Telephone))
                throw new ArgumentException("Telephone cannot be empty.");

            // Email must be unique for both add and edit operations.
            if (!isUpdate && _repository.EmailExists(person.Email))
                throw new Exception("Email already exists.");

            if (isUpdate && _repository.EmailExistsExcept(person.Email, person.PersonId))
                throw new Exception("Email already exists.");

            // Performer rules.
            if (person is Performer performer)
            {
                if (performer.Fee < 0)
                    throw new Exception("Performer fee cannot be negative.");

                if (performer.Genres == null || performer.Genres.Count == 0)
                    throw new Exception("A performer must have at least one genre.");
            }

            // Crew rules.
            if (person is Crew crew)
            {
                if (crew.HourlyRate < 0)
                    throw new Exception("Hourly rate cannot be negative.");

                if (crew.EmploymentType == EmploymentType.FullTime &&
                    (crew.WeeklyHours < 25 || crew.WeeklyHours > 40))
                {
                    throw new Exception("FullTime weekly hours must be between 25 and 40.");
                }

                if (crew.EmploymentType == EmploymentType.PartTime &&
                    (crew.WeeklyHours < 1 || crew.WeeklyHours > 24))
                {
                    throw new Exception("PartTime weekly hours must be between 1 and 24.");
                }
            }

            // Vendor rules.
            if (person is Vendor vendor)
            {
                if (string.IsNullOrWhiteSpace(vendor.CompanyName))
                    throw new Exception("Company name cannot be empty.");

                if (vendor.Categories == null || vendor.Categories.Count == 0)
                    throw new Exception("A vendor must have at least one product category.");
            }
        }
    }
}