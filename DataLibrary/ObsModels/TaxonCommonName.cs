using System;

namespace Models;

    /// <summary>
    /// Represents a TaxonCommonNames.
    /// NOTE: This class is generated from a T4 template - you should not modify it manually.
    /// </summary>
    public class TaxonCommonName 
    {
        public const string TableName = "TaxonCommonNames";
        public  string? Comments { get; set; }

        public  string? Simplified { get; set; }

        public  string? CommonName { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Models.TaxonCommonName"/> class.
    /// </summary>
        public TaxonCommonName()
        {
            // Initialize properties if needed
        }
    }
