using System;

namespace Models;

    /// <summary>
    /// Represents a Allsurfaceepifaunadata.
    /// NOTE: This class is generated from a T4 template - you should not modify it manually.
    /// </summary>
    public class Allsurfaceepifaunadata 
    {
        public const string TableName = "Allsurfaceepifaunadata";
        public  int? Upload_id { get; set; }

        public  string? Uploader { get; set; }

        public  string? Organization { get; set; }

        public  string? Upload_date { get; set; }

        public  string? Upload_change_date { get; set; }

        public  string? Subbasin { get; set; }

        public  string? Site { get; set; }

        public  double? Latitude { get; set; }

        public  double? Longitude { get; set; }

        public  string? Strata { get; set; }

        public  string? Date { get; set; }

        public  string? Time { get; set; }

        public  double? Surface_area_m2 { get; set; }

        public  int? Elevation { get; set; }

        public  int? Replicate { get; set; }

        public  string? Species { get; set; }

        public  string? Common_Name { get; set; }

        public  string? Count_before_loose_Ulva_removal { get; set; }

        public  string? Count_after_loose_Ulva_removal { get; set; }

        public  string? Count_after_Eelgrass_comb_back { get; set; }

        public  string? Percent_before_loose_Ulva_removal { get; set; }

        public  string? Percent_after_loose_Ulva_removal { get; set; }

        public  string? Percent_after_Eelgrass_comb_back { get; set; }

        public  string? Notes { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Allsurfaceepifaunadata"/> class.
        /// </summary>
        public Allsurfaceepifaunadata()
        {
            // Initialize properties if needed
        }
    }
