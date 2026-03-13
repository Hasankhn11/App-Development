using System;

namespace FestivalManagementSystem.Models
{
    // Employment types allowed for crew members.
    public enum EmploymentType
    {
        FullTime,
        PartTime
    }

    // Represents a crew member working at the festival.
    public class Crew : Person
    {
        public decimal HourlyRate { get; set; }
        public EmploymentType EmploymentType { get; set; }
        public int WeeklyHours { get; set; }

        public Crew(string name, string email, string telephone,
                    decimal hourlyRate, EmploymentType employmentType, int weeklyHours)
            : base(name, email, telephone)
        {
            HourlyRate = hourlyRate;
            EmploymentType = employmentType;
            WeeklyHours = weeklyHours;
        }

        public override string GetRole() => "Crew";

        public override string GetDetails()
        {
            return $"Hourly Rate: {HourlyRate:C}, Type: {EmploymentType}, Weekly Hours: {WeeklyHours}";
        }
    }
}