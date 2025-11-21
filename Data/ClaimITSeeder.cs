// Data/ClaimITSeeder.cs
using Microsoft.EntityFrameworkCore;
using ClaimIT.Models;
using System.Text.Json;

namespace ClaimIT.Data
{
    public static class ClaimITSeeder
    {
        private static readonly Random Rand = new(42);

        public static void Seed(ModelBuilder modelBuilder)
        {
            var claims = new List<object>();

            for (int i = 1; i <= 380; i++)
            {
                var first = GetRandom(new[] { "Sarah", "Michael", "Thandi", "Pieter", "Aisha", "Lerato", "David", "Fatima", "Johan", "Naledi" });
                var last = GetRandom(new[] { "Johnson", "Chen", "Mokoena", "van der Merwe", "Patel", "Nkosi", "Goldberg", "Abrahams", "Botha", "Zulu" });
                var title = Rand.Next(20) == 0 ? "Prof " : Rand.Next(5) == 0 ? "Dr " : "";
                var name = title + first + " " + last;
                var email = $"{first.ToLower()}.{last.ToLower()}@faculty.ac.za";

                var hourlyRate = Rand.Next(100) switch
                {
                    >= 90 => 950m + Rand.Next(150),
                    >= 60 => 750m + Rand.Next(200),
                    _ => 620m + Rand.Next(180)
                };

                var whole = Rand.Next(4, 52);
                var fraction = Rand.Next(0, 4) * 0.25m;  // 0.00, 0.25, 0.50, 0.75
                var hoursWorked = whole + fraction;

                var status = (i % 17) switch
                {
                    0 or 1 => "Approved",
                    2 => "Rejected",
                    3 or 4 => "Verified",
                    _ => "Pending"
                };

                var hasDocs = Rand.Next(5) > 0;
                var docCount = hasDocs ? Rand.Next(1, 5) : 0;

                claims.Add(new
                {
                    Id = i,
                    LecturerName = name,
                    LecturerEmail = email,
                    HoursWorked = hoursWorked,
                    HourlyRate = hourlyRate,
                    SubmittedDate = DateTime.Today.AddDays(-Rand.Next(1, 90))
                        .AddHours(Rand.Next(8, 19))
                        .AddMinutes(Rand.Next(0, 60)),
                    Status = status,
                    DocumentNamesJson = docCount > 0 ? "[\"Timesheet.pdf\",\"Syllabus.pdf\"]" : null,
                    DocumentPathsJson = docCount > 0 ? $"[\"/uploads/doc{i}a.pdf\",\"/uploads/doc{i}b.pdf\"]" : null
                });
            }

            modelBuilder.Entity<Claim>().HasData(claims);
        }

        private static T GetRandom<T>(T[] array) => array[Rand.Next(array.Length)];
    }
}