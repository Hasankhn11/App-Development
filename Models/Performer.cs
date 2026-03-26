using System;
using System.Collections.Generic;

namespace FestivalManagementSystem.Models
{
    // Represents a performer such as an artist or DJ.
    public class Performer : Person
    {
        public decimal Fee { get; set; }
        public List<string> Genres { get; set; } = new();

        public Performer(string name, string email, string telephone, decimal fee)
            : base(name, email, telephone)
        {
            Fee = fee;
        }

        public override string GetRole() => "Performer";

        public override string GetDetails()
        {
            string genreText = Genres.Count > 0 ? string.Join(", ", Genres) : "None";
            return $"Fee: {Fee:C}, Genres: {genreText}";
        }
    }
}