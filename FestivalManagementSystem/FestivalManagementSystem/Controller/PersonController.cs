using System;
using System.Collections.Generic;
using FestivalManagementSystem.Models;
using FestivalManagementSystem.Services;
using FestivalManagementSystem.View;

namespace FestivalManagementSystem.Controller
{
    // Coordinates the flow between the view and service layers.
    public class PersonController
    {
        private readonly PersonService _service;
        private readonly ConsoleView _view;

        public PersonController(PersonService service, ConsoleView view)
        {
            _service = service;
            _view = view;
        }

        // Main controller loop for the application.
        public void Run()
        {
            bool running = true;

            while (running)
            {
                _view.ShowMenu();
                int choice = _view.GetMenuChoice();

                try
                {
                    switch (choice)
                    {
                        case 1:
                            AddPerson();
                            break;
                        case 2:
                            ViewAllPeople();
                            break;
                        case 3:
                            ViewPeopleByRole();
                            break;
                        case 4:
                            EditPerson();
                            break;
                        case 5:
                            DeletePerson();
                            break;
                        case 0:
                            running = false;
                            _view.ShowMessage("Exiting application...");
                            break;
                        default:
                            _view.ShowMessage("Invalid option. Please try again.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _view.ShowMessage($"Error: {ex.Message}");
                }

                if (running)
                    _view.Pause();
            }
        }

        // Adds a new record using the selected role.
        private void AddPerson()
        {
            string role = _view.SelectRole();
            Person person = _view.CreatePerson(role);

            _service.AddPerson(person);
            _view.ShowMessage("Record added successfully.");
        }

        // Displays every person record.
        private void ViewAllPeople()
        {
            List<Person> people = _service.GetAllPeople();
            _view.DisplayPeople(people);
        }

        // Displays records filtered by role.
        private void ViewPeopleByRole()
        {
            string role = _view.SelectRole();
            List<Person> people = _service.GetPeopleByRole(role);
            _view.DisplayPeople(people);
        }

        // Edits an existing record by PersonID.
        private void EditPerson()
        {
            int id = _view.PromptPersonId("Enter Person ID to edit: ");
            Person existing = _service.GetPersonById(id);

            if (existing == null)
            {
                _view.ShowMessage("Person not found.");
                return;
            }

            Person updated = _view.EditPerson(existing);
            _service.UpdatePerson(updated);

            _view.ShowMessage("Record updated successfully.");
        }

        // Deletes an existing record by PersonID after confirmation.
        private void DeletePerson()
        {
            int id = _view.PromptPersonId("Enter Person ID to delete: ");
            Person existing = _service.GetPersonById(id);

            if (existing == null)
            {
                _view.ShowMessage("Person not found.");
                return;
            }

            _view.ShowSinglePerson(existing);

            if (_view.ConfirmDelete())
            {
                _service.DeletePerson(id);
                _view.ShowMessage("Record deleted successfully.");
            }
            else
            {
                _view.ShowMessage("Delete cancelled.");
            }
        }
    }
}