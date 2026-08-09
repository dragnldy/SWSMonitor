using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLibrary.ModelExtensions;

public class SurveySitex
{
    public required int ID { get; set; }
    public required string Name { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public string? Island { get; set; }
    public string? Directions { get; set; }
    public string? IsMonitored { get; set; }

    public string? TideChart { get; set; }
    public string? VertRef { get; set; }

    public bool? Bulkhead { get; set; }

    public string? BulkHeadConstruction { get; set; }

    public string? ProfileDirections { get; set; }

    public decimal? ProfileLineStart { get; set; }

    public int? SurveyWidth { get; set; }
    }
