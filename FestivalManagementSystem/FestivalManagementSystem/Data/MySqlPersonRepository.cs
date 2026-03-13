using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using FestivalManagementSystem.Interfaces;
using FestivalManagementSystem.Models;

namespace FestivalManagementSystem.Data
{
    public class MySqlPersonRepository : IPersonRepository
    {
        private readonly string _connectionString;

        public MySqlPersonRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private MySqlConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        public void Add(Person person)
        {
            using MySqlConnection conn = CreateConnection();
            conn.Open();

            using MySqlTransaction transaction = conn.BeginTransaction();

            try
            {
                string insertPersonSql = @"
                    INSERT INTO Persons (Name, Telephone, Email, Role)
                    VALUES (@Name, @Telephone, @Email, @Role);
                    SELECT LAST_INSERT_ID();";

                int personId;

                using (MySqlCommand cmd = new MySqlCommand(insertPersonSql, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@Name", person.Name);
                    cmd.Parameters.AddWithValue("@Telephone", person.Telephone);
                    cmd.Parameters.AddWithValue("@Email", person.Email);
                    cmd.Parameters.AddWithValue("@Role", person.GetRole());

                    personId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                person.SetId(personId);

                if (person is Performer performer)
                {
                    string insertPerformerSql = @"
                        INSERT INTO Performers (PersonID, Fee)
                        VALUES (@PersonID, @Fee);";

                    using (MySqlCommand cmd = new MySqlCommand(insertPerformerSql, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@PersonID", personId);
                        cmd.Parameters.AddWithValue("@Fee", performer.Fee);
                        cmd.ExecuteNonQuery();
                    }

                    foreach (string genre in performer.Genres)
                    {
                        int genreId = GetOrCreateGenreId(genre, conn, transaction);

                        string linkSql = @"
                            INSERT INTO PerformerGenres (PersonID, GenreID)
                            VALUES (@PersonID, @GenreID);";

                        using MySqlCommand cmd = new MySqlCommand(linkSql, conn, transaction);
                        cmd.Parameters.AddWithValue("@PersonID", personId);
                        cmd.Parameters.AddWithValue("@GenreID", genreId);
                        cmd.ExecuteNonQuery();
                    }
                }
                else if (person is Crew crew)
                {
                    string insertCrewSql = @"
                        INSERT INTO Crew (PersonID, HourlyRate, EmploymentType, WeeklyHours)
                        VALUES (@PersonID, @HourlyRate, @EmploymentType, @WeeklyHours);";

                    using MySqlCommand cmd = new MySqlCommand(insertCrewSql, conn, transaction);
                    cmd.Parameters.AddWithValue("@PersonID", personId);
                    cmd.Parameters.AddWithValue("@HourlyRate", crew.HourlyRate);
                    cmd.Parameters.AddWithValue("@EmploymentType", crew.EmploymentType.ToString());
                    cmd.Parameters.AddWithValue("@WeeklyHours", crew.WeeklyHours);
                    cmd.ExecuteNonQuery();
                }
                else if (person is Vendor vendor)
                {
                    string insertVendorSql = @"
                        INSERT INTO Vendors (PersonID, VendorName)
                        VALUES (@PersonID, @VendorName);";

                    using (MySqlCommand cmd = new MySqlCommand(insertVendorSql, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@PersonID", personId);
                        cmd.Parameters.AddWithValue("@VendorName", vendor.CompanyName);
                        cmd.ExecuteNonQuery();
                    }

                    foreach (string category in vendor.Categories)
                    {
                        int categoryId = GetOrCreateCategoryId(category, conn, transaction);

                        string linkSql = @"
                            INSERT INTO VendorCategories (PersonID, CategoryID)
                            VALUES (@PersonID, @CategoryID);";

                        using MySqlCommand cmd = new MySqlCommand(linkSql, conn, transaction);
                        cmd.Parameters.AddWithValue("@PersonID", personId);
                        cmd.Parameters.AddWithValue("@CategoryID", categoryId);
                        cmd.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public List<Person> GetAll()
        {
            List<Person> people = new();

            using MySqlConnection conn = CreateConnection();
            conn.Open();

            string sql = "SELECT PersonID, Name, Telephone, Email, Role FROM Persons ORDER BY PersonID;";

            using MySqlCommand cmd = new MySqlCommand(sql, conn);
            using MySqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                int id = reader.GetInt32("PersonID");
                string name = reader.GetString("Name");
                string telephone = reader.GetString("Telephone");
                string email = reader.GetString("Email");
                string role = reader.GetString("Role");

                if (role == "Performer")
                    people.Add(GetPerformerById(id, name, email, telephone));
                else if (role == "Crew")
                    people.Add(GetCrewById(id, name, email, telephone));
                else if (role == "Vendor")
                    people.Add(GetVendorById(id, name, email, telephone));
            }

            return people;
        }

        public Person GetById(int id)
        {
            foreach (Person person in GetAll())
            {
                if (person.PersonId == id)
                    return person;
            }

            return null;
        }

        public void Update(Person person)
        {
            using MySqlConnection conn = CreateConnection();
            conn.Open();

            using MySqlTransaction transaction = conn.BeginTransaction();

            try
            {
                string updatePersonSql = @"
                    UPDATE Persons
                    SET Name = @Name, Telephone = @Telephone, Email = @Email, Role = @Role
                    WHERE PersonID = @PersonID;";

                using (MySqlCommand cmd = new MySqlCommand(updatePersonSql, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@Name", person.Name);
                    cmd.Parameters.AddWithValue("@Telephone", person.Telephone);
                    cmd.Parameters.AddWithValue("@Email", person.Email);
                    cmd.Parameters.AddWithValue("@Role", person.GetRole());
                    cmd.Parameters.AddWithValue("@PersonID", person.PersonId);
                    cmd.ExecuteNonQuery();
                }

                if (person is Performer performer)
                {
                    string updateSql = "UPDATE Performers SET Fee = @Fee WHERE PersonID = @PersonID;";
                    using (MySqlCommand cmd = new MySqlCommand(updateSql, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@Fee", performer.Fee);
                        cmd.Parameters.AddWithValue("@PersonID", performer.PersonId);
                        cmd.ExecuteNonQuery();
                    }

                    string deleteLinks = "DELETE FROM PerformerGenres WHERE PersonID = @PersonID;";
                    using (MySqlCommand cmd = new MySqlCommand(deleteLinks, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@PersonID", performer.PersonId);
                        cmd.ExecuteNonQuery();
                    }

                    foreach (string genre in performer.Genres)
                    {
                        int genreId = GetOrCreateGenreId(genre, conn, transaction);

                        string insertLink = "INSERT INTO PerformerGenres (PersonID, GenreID) VALUES (@PersonID, @GenreID);";
                        using MySqlCommand cmd = new MySqlCommand(insertLink, conn, transaction);
                        cmd.Parameters.AddWithValue("@PersonID", performer.PersonId);
                        cmd.Parameters.AddWithValue("@GenreID", genreId);
                        cmd.ExecuteNonQuery();
                    }
                }
                else if (person is Crew crew)
                {
                    string updateSql = @"
                        UPDATE Crew
                        SET HourlyRate = @HourlyRate, EmploymentType = @EmploymentType, WeeklyHours = @WeeklyHours
                        WHERE PersonID = @PersonID;";

                    using MySqlCommand cmd = new MySqlCommand(updateSql, conn, transaction);
                    cmd.Parameters.AddWithValue("@HourlyRate", crew.HourlyRate);
                    cmd.Parameters.AddWithValue("@EmploymentType", crew.EmploymentType.ToString());
                    cmd.Parameters.AddWithValue("@WeeklyHours", crew.WeeklyHours);
                    cmd.Parameters.AddWithValue("@PersonID", crew.PersonId);
                    cmd.ExecuteNonQuery();
                }
                else if (person is Vendor vendor)
                {
                    string updateSql = "UPDATE Vendors SET VendorName = @VendorName WHERE PersonID = @PersonID;";
                    using (MySqlCommand cmd = new MySqlCommand(updateSql, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@VendorName", vendor.CompanyName);
                        cmd.Parameters.AddWithValue("@PersonID", vendor.PersonId);
                        cmd.ExecuteNonQuery();
                    }

                    string deleteLinks = "DELETE FROM VendorCategories WHERE PersonID = @PersonID;";
                    using (MySqlCommand cmd = new MySqlCommand(deleteLinks, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@PersonID", vendor.PersonId);
                        cmd.ExecuteNonQuery();
                    }

                    foreach (string category in vendor.Categories)
                    {
                        int categoryId = GetOrCreateCategoryId(category, conn, transaction);

                        string insertLink = "INSERT INTO VendorCategories (PersonID, CategoryID) VALUES (@PersonID, @CategoryID);";
                        using MySqlCommand cmd = new MySqlCommand(insertLink, conn, transaction);
                        cmd.Parameters.AddWithValue("@PersonID", vendor.PersonId);
                        cmd.Parameters.AddWithValue("@CategoryID", categoryId);
                        cmd.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void Delete(int id)
        {
            using MySqlConnection conn = CreateConnection();
            conn.Open();

            string sql = "DELETE FROM Persons WHERE PersonID = @PersonID;";
            using MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@PersonID", id);
            cmd.ExecuteNonQuery();
        }

        public bool EmailExists(string email)
        {
            using MySqlConnection conn = CreateConnection();
            conn.Open();

            string sql = "SELECT COUNT(*) FROM Persons WHERE Email = @Email;";
            using MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Email", email);

            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public bool EmailExistsExcept(string email, int excludedPersonId)
        {
            using MySqlConnection conn = CreateConnection();
            conn.Open();

            string sql = "SELECT COUNT(*) FROM Persons WHERE Email = @Email AND PersonID <> @PersonID;";
            using MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@PersonID", excludedPersonId);

            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        private Performer GetPerformerById(int id, string name, string email, string telephone)
        {
            using MySqlConnection conn = CreateConnection();
            conn.Open();

            decimal fee = 0;
            List<string> genres = new();

            using (MySqlCommand cmd = new MySqlCommand("SELECT Fee FROM Performers WHERE PersonID = @PersonID;", conn))
            {
                cmd.Parameters.AddWithValue("@PersonID", id);
                fee = Convert.ToDecimal(cmd.ExecuteScalar());
            }

            string genreSql = @"
                SELECT g.GenreName
                FROM PerformerGenres pg
                JOIN Genres g ON pg.GenreID = g.GenreID
                WHERE pg.PersonID = @PersonID;";

            using (MySqlCommand cmd = new MySqlCommand(genreSql, conn))
            {
                cmd.Parameters.AddWithValue("@PersonID", id);
                using MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                    genres.Add(reader.GetString("GenreName"));
            }

            Performer performer = new Performer(name, email, telephone, fee);
            performer.SetId(id);
            performer.Genres = genres;
            return performer;
        }

        private Crew GetCrewById(int id, string name, string email, string telephone)
        {
            using MySqlConnection conn = CreateConnection();
            conn.Open();

            string sql = "SELECT HourlyRate, EmploymentType, WeeklyHours FROM Crew WHERE PersonID = @PersonID;";
            using MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@PersonID", id);

            using MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                Crew crew = new Crew(
                    name,
                    email,
                    telephone,
                    reader.GetDecimal("HourlyRate"),
                    Enum.Parse<EmploymentType>(reader.GetString("EmploymentType")),
                    reader.GetInt32("WeeklyHours")
                );
                crew.SetId(id);
                return crew;
            }

            return null;
        }

        private Vendor GetVendorById(int id, string name, string email, string telephone)
        {
            using MySqlConnection conn = CreateConnection();
            conn.Open();

            string vendorName = "";
            List<string> categories = new();

            using (MySqlCommand cmd = new MySqlCommand("SELECT VendorName FROM Vendors WHERE PersonID = @PersonID;", conn))
            {
                cmd.Parameters.AddWithValue("@PersonID", id);
                vendorName = Convert.ToString(cmd.ExecuteScalar());
            }

            string categorySql = @"
                SELECT c.CategoryName
                FROM VendorCategories vc
                JOIN Categories c ON vc.CategoryID = c.CategoryID
                WHERE vc.PersonID = @PersonID;";

            using (MySqlCommand cmd = new MySqlCommand(categorySql, conn))
            {
                cmd.Parameters.AddWithValue("@PersonID", id);
                using MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                    categories.Add(reader.GetString("CategoryName"));
            }

            Vendor vendor = new Vendor(name, email, telephone, vendorName);
            vendor.SetId(id);
            vendor.Categories = categories;
            return vendor;
        }

        private int GetOrCreateGenreId(string genreName, MySqlConnection conn, MySqlTransaction transaction)
        {
            string selectSql = "SELECT GenreID FROM Genres WHERE GenreName = @GenreName;";
            using (MySqlCommand cmd = new MySqlCommand(selectSql, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@GenreName", genreName);
                object result = cmd.ExecuteScalar();

                if (result != null)
                    return Convert.ToInt32(result);
            }

            string insertSql = "INSERT INTO Genres (GenreName) VALUES (@GenreName); SELECT LAST_INSERT_ID();";
            using (MySqlCommand cmd = new MySqlCommand(insertSql, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@GenreName", genreName);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private int GetOrCreateCategoryId(string categoryName, MySqlConnection conn, MySqlTransaction transaction)
        {
            string selectSql = "SELECT CategoryID FROM Categories WHERE CategoryName = @CategoryName;";
            using (MySqlCommand cmd = new MySqlCommand(selectSql, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@CategoryName", categoryName);
                object result = cmd.ExecuteScalar();

                if (result != null)
                    return Convert.ToInt32(result);
            }

            string insertSql = "INSERT INTO Categories (CategoryName) VALUES (@CategoryName); SELECT LAST_INSERT_ID();";
            using (MySqlCommand cmd = new MySqlCommand(insertSql, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@CategoryName", categoryName);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
    }
}