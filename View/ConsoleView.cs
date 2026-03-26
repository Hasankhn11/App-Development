using System;
using System.Collections.Generic;
using FestivalManagementSystem.Models;

namespace FestivalManagementSystem.View
{
    // Handles all console input and output for the system.
    public class ConsoleView
    {
        // Displays the main menu in a cleaner styled layout.
        public void ShowMenu()
        {
            Console.Clear();
            Console.WriteLine("╔═══════════════════════════════════════╗");
            Console.WriteLine("║      FESTIVAL MANAGEMENT SYSTEM       ║");
            Console.WriteLine("╚═══════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine("1. Add new record");
            Console.WriteLine("2. View all records");
            Console.WriteLine("3. View records by role");
            Console.WriteLine("4. Edit existing record");
            Console.WriteLine("5. Delete existing record");
            Console.WriteLine("0. Exit");
            Console.WriteLine();
            Console.Write("Select option: ");
        }

        // Reads and validates the menu choice.
        public int GetMenuChoice()
        {
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out int choice))
                    return choice;

                Console.Write("Invalid option. Enter a number: ");
            }
        }

        // Lets the user choose one of the valid roles.
        public string SelectRole()
        {
            Console.WriteLine();
            Console.WriteLine("Select role:");
            Console.WriteLine("1. Performer");
            Console.WriteLine("2. Crew");
            Console.WriteLine("3. Vendor");
            Console.Write("Choice: ");

            while (true)
            {
                string input = Console.ReadLine();

                switch (input)
                {
                    case "1": return "Performer";
                    case "2": return "Crew";
                    case "3": return "Vendor";
                    default:
                        Console.Write("Invalid role. Enter 1, 2 or 3: ");
                        break;
                }
            }
        }

        // Creates a new person object based on the chosen role.
        public Person CreatePerson(string role)
        {
            Console.WriteLine();
            Console.WriteLine($"--- Creating {role} ---");

            string name = PromptRequiredString("Name");
            string email = PromptRequiredString("Email");
            string telephone = PromptRequiredString("Telephone");

            if (role == "Performer")
            {
                decimal fee = PromptDecimal("Fee");
                Performer performer = new Performer(name, email, telephone, fee);
                performer.Genres = PromptList("genre");
                return performer;
            }

            if (role == "Crew")
            {
                decimal hourlyRate = PromptDecimal("Hourly Rate");
                EmploymentType employmentType = PromptEmploymentType();
                int weeklyHours = PromptInt("Weekly Hours");
                return new Crew(name, email, telephone, hourlyRate, employmentType, weeklyHours);
            }

            if (role == "Vendor")
            {
                string companyName = PromptRequiredString("Company Name");
                Vendor vendor = new Vendor(name, email, telephone, companyName);
                vendor.Categories = PromptList("category");
                return vendor;
            }

            throw new Exception("Invalid role.");
        }

        // Builds an updated object while allowing the user to keep old values.
        public Person EditPerson(Person existingPerson)
        {
            Console.WriteLine();
            Console.WriteLine($"--- Editing {existingPerson.GetRole()} (ID: {existingPerson.PersonId}) ---");
            Console.WriteLine("Press Enter to keep the existing value.");
            Console.WriteLine();

            string name = PromptOptionalString("Name", existingPerson.Name);
            string email = PromptOptionalString("Email", existingPerson.Email);
            string telephone = PromptOptionalString("Telephone", existingPerson.Telephone);

            if (existingPerson is Performer performer)
            {
                decimal fee = PromptOptionalDecimal("Fee", performer.Fee);
                List<string> genres = PromptOptionalList("genre", performer.Genres);

                Performer updated = new Performer(name, email, telephone, fee);
                updated.SetId(existingPerson.PersonId);
                updated.Genres = genres;
                return updated;
            }

            if (existingPerson is Crew crew)
            {
                decimal hourlyRate = PromptOptionalDecimal("Hourly Rate", crew.HourlyRate);
                EmploymentType employmentType = PromptOptionalEmploymentType(crew.EmploymentType);
                int weeklyHours = PromptOptionalInt("Weekly Hours", crew.WeeklyHours);

                Crew updated = new Crew(name, email, telephone, hourlyRate, employmentType, weeklyHours);
                updated.SetId(existingPerson.PersonId);
                return updated;
            }

            if (existingPerson is Vendor vendor)
            {
                string companyName = PromptOptionalString("Company Name", vendor.CompanyName);
                List<string> categories = PromptOptionalList("category", vendor.Categories);

                Vendor updated = new Vendor(name, email, telephone, companyName);
                updated.SetId(existingPerson.PersonId);
                updated.Categories = categories;
                return updated;
            }

            throw new Exception("Unsupported person type.");
        }

        // Displays many people in a consistent format.
        public void DisplayPeople(List<Person> people)
        {
            Console.WriteLine();

            if (people == null || people.Count == 0)
            {
                Console.WriteLine("No records found.");
                return;
            }

            foreach (Person person in people)
            {
                ShowSinglePerson(person);
                Console.WriteLine(new string('-', 40));
            }
        }

        // Displays one person record.
        public void ShowSinglePerson(Person person)
        {
            Console.WriteLine($"ID: {person.PersonId}");
            Console.WriteLine($"Name: {person.Name}");
            Console.WriteLine($"Email: {person.Email}");
            Console.WriteLine($"Telephone: {person.Telephone}");
            Console.WriteLine($"Role: {person.GetRole()}");
            Console.WriteLine(person.GetDetails());
        }

        // Reads a valid ID from the user.
        public int PromptPersonId(string message)
        {
            return PromptIntWithMessage(message);
        }

        // Confirms whether the user wants to delete a record.
        public bool ConfirmDelete()
        {
            while (true)
            {
                Console.Write("Confirm delete (y/n): ");
                string input = Console.ReadLine()?.Trim().ToLower();

                if (input == "y") return true;
                if (input == "n") return false;

                Console.WriteLine("Please enter y or n.");
            }
        }

        // Shows a simple message to the user.
        public void ShowMessage(string message)
        {
            Console.WriteLine(message);
        }

        // Pauses the console so the user can read output before the menu returns.
        public void Pause()
        {
            Console.WriteLine();
            Console.Write("Press Enter to continue...");
            Console.ReadLine();
        }

        // -------------------------
        // Helper input methods
        // -------------------------

        private string PromptRequiredString(string label)
        {
            while (true)
            {
                Console.Write($"{label}: ");
                string input = Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(input))
                    return input.Trim();

                Console.WriteLine($"{label} cannot be empty.");
            }
        }

        private string PromptOptionalString(string label, string currentValue)
        {
            Console.Write($"{label} ({currentValue}): ");
            string input = Console.ReadLine();

            return string.IsNullOrWhiteSpace(input) ? currentValue : input.Trim();
        }

        private int PromptInt(string label)
        {
            while (true)
            {
                Console.Write($"{label}: ");
                if (int.TryParse(Console.ReadLine(), out int value))
                    return value;

                Console.WriteLine("Please enter a valid whole number.");
            }
        }

        private int PromptIntWithMessage(string message)
        {
            while (true)
            {
                Console.Write(message);
                if (int.TryParse(Console.ReadLine(), out int value))
                    return value;

                Console.WriteLine("Please enter a valid whole number.");
            }
        }

        private int PromptOptionalInt(string label, int currentValue)
        {
            while (true)
            {
                Console.Write($"{label} ({currentValue}): ");
                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    return currentValue;

                if (int.TryParse(input, out int value))
                    return value;

                Console.WriteLine("Please enter a valid whole number.");
            }
        }

        private decimal PromptDecimal(string label)
        {
            while (true)
            {
                Console.Write($"{label}: ");
                if (decimal.TryParse(Console.ReadLine(), out decimal value))
                    return value;

                Console.WriteLine("Please enter a valid decimal number.");
            }
        }

        private decimal PromptOptionalDecimal(string label, decimal currentValue)
        {
            while (true)
            {
                Console.Write($"{label} ({currentValue}): ");
                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    return currentValue;

                if (decimal.TryParse(input, out decimal value))
                    return value;

                Console.WriteLine("Please enter a valid decimal number.");
            }
        }

        private EmploymentType PromptEmploymentType()
        {
            Console.Write("Employment Type (1 = FullTime, 2 = PartTime): ");

            while (true)
            {
                string input = Console.ReadLine();

                if (input == "1") return EmploymentType.FullTime;
                if (input == "2") return EmploymentType.PartTime;

                Console.Write("Invalid choice. Enter 1 or 2: ");
            }
        }

        private EmploymentType PromptOptionalEmploymentType(EmploymentType currentValue)
        {
            Console.Write($"Employment Type ({currentValue}) [1 = FullTime, 2 = PartTime]: ");

            while (true)
            {
                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    return currentValue;

                if (input == "1") return EmploymentType.FullTime;
                if (input == "2") return EmploymentType.PartTime;

                Console.Write("Invalid choice. Enter 1, 2 or press Enter to keep current: ");
            }
        }

        private List<string> PromptList(string label)
        {
            List<string> items = new();

            Console.WriteLine($"Enter {label}s (type 'done' to finish):");

            while (true)
            {
                string input = Console.ReadLine();

                if (input != null && input.Trim().Equals("done", StringComparison.OrdinalIgnoreCase))
                    break;

                if (!string.IsNullOrWhiteSpace(input))
                    items.Add(input.Trim());
            }

            return items;
        }

        private List<string> PromptOptionalList(string label, List<string> currentValues)
        {
            Console.WriteLine($"Current {label}s: {string.Join(", ", currentValues)}");
            Console.WriteLine($"Enter new {label}s (type 'done' to finish, or just type 'done' to keep current values):");

            List<string> items = new();

            while (true)
            {
                string input = Console.ReadLine();

                if (input != null && input.Trim().Equals("done", StringComparison.OrdinalIgnoreCase))
                    break;

                if (!string.IsNullOrWhiteSpace(input))
                    items.Add(input.Trim());
            }

            return items.Count == 0 ? new List<string>(currentValues) : items;
        }
    }
}