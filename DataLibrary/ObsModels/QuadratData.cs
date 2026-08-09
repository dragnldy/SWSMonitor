using System;

namespace Models;

    /// <summary>
    /// Represents a QuadratData.
    /// NOTE: This class is generated from a T4 template - you should not modify it manually.
    /// </summary>
    public class QuadratData 
    {
        public const string TableName = "QuadratData";
        public  short? ActualNumber { get; set; }

        public  int? BeachId { get; set; }

        public required string BeachName { get; set; }

        public  int? CommonName { get; set; }

        public required DateTime Date { get; set; }

        public  bool? Dense { get; set; }

        public  DateTime? Entrydate { get; set; }

        public required int ID { get; set; }

        public  Single? PercentObserved { get; set; }

        public required string Quadrat { get; set; }

        public  int? QuadratDescription { get; set; }

        public  int? QuadratLeadId { get; set; }

        public  string? QuadratNotes { get; set; }

        public required string Species { get; set; }

        public  int? SpeciesLinkId { get; set; }

        public required string Tide { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuadratData"/> class.
        /// </summary>
        public QuadratData()
        {
            // Initialize properties if needed
        }
    }
