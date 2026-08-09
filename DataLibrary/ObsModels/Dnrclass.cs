using System;

namespace Models;

    /// <summary>
    /// Represents a Dnrclass.
    /// NOTE: This class is generated from a T4 template - you should not modify it manually.
    /// </summary>
    public class Dnrclass 
    {
        public const string TableName = "Dnrclass";
        public  string? DnrClass { get; set; }

        public required int ID { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Dnrclass"/> class.
        /// </summary>
        public Dnrclass()
        {
            // Initialize properties if needed
        }
    }
