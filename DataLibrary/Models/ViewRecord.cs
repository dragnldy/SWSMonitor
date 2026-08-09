using DataLibrary.Crud;

namespace DataLibrary.Models;

public class ViewRecord
{
    public static string AvailableViewsTableName = "availableviews";
    public static string AvailableViewName = "availableview";
    public string ViewName { get; set; } = "";

    // Only used when the view is a summary of multiple tables, this dictionary will hold the counts of records for each table.
    public Dictionary<string,int> RecordCounts { get; set; } = new Dictionary<string, int>();
    public List<DataRecord> Records { get; set; } = new List<DataRecord>();

}
