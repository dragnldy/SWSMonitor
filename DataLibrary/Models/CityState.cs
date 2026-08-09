using Models;

namespace DataLibrary.Models;

public class CityState : ITableBase
{
    // This is actually a view not a table
    public const string TableName = "CityStates";
    public int ID { get; set; }
    public DateTime? EntryDate { get; set; }

    public string City { get; set; } = string.Empty;
    public string State { get; set; } = "WA"; // Default to Washington State
    public string Island {  get; set; } = string.Empty;

    public CityState() { }
}
