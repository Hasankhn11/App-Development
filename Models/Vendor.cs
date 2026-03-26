using System;
using System.Collections.Generic;

namespace FestivalManagementSystem.Models
{
    // Represents a vendor such as a food or merchandise stall.
    public class Vendor : Person
    {
        public string CompanyName { get; set; }
        public List<string> Categories { get; set; } = new();

        public Vendor(string name, string email, string telephone, string companyName)
            : base(name, email, telephone)
        {
            if (string.IsNullOrWhiteSpace(companyName))
                throw new ArgumentException("Company name cannot be empty.");

            CompanyName = companyName.Trim();
        }

        public override string GetRole() => "Vendor";

        public override string GetDetails()
        {
            string categoryText = Categories.Count > 0 ? string.Join(", ", Categories) : "None";
            return $"Company: {CompanyName}, Categories: {categoryText}";
        }
    }
}