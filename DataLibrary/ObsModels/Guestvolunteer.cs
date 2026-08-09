using System;

namespace DataLibrary;

    /// <summary>
    /// Represents a Guestvolunteers.
    /// NOTE: This class is generated from a T4 template - you should not modify it manually.
    /// </summary>
    public class Guestvolunteer 
    {
        public const string TableName = "Guestvolunteers";
        public  string? Address { get; set; }

        public  string? City { get; set; }

        public  int? County { get; set; }

        public  string? Email { get; set; }

        public  string? FirstLast { get; set; }

        public  string? FirstName { get; set; }

        public  string? FormerOccupation { get; set; }

        public  string? GuestNotes { get; set; }

        public required int ID { get; set; }

        public  string? LastName { get; set; }

        public  string? Occupation { get; set; }

        public  string? Phone { get; set; }

        public  string? Projects { get; set; }

        public  string? SpecialInterests { get; set; }

        public  string? State { get; set; }

        public  double? Zip { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Guestvolunteer"/> class.
        /// </summary>
        public Guestvolunteer()
        {
            // Initialize properties if needed
        }
    }
