using DataLibrary.DataSources;
using DataLibrary.DataSources.ApiClients;
using DataLibrary.Models;

namespace DataLibrary.Crud;

public static class ViewRecordCrud
{
    public static async Task<ViewRecord> ReadView(IDataSourceConfig? config, string viewName = "")
    {
        bool isAvailableView = viewName.Equals(ViewRecord.AvailableViewName, StringComparison.OrdinalIgnoreCase);

        if (config == null || string.IsNullOrEmpty(viewName))
        {
            return new ViewRecord() { ViewName = viewName };
        }

        if (isAvailableView) viewName = ViewRecord.AvailableViewsTableName;


        if (config is MySqlConfig)
        {
            var viewRecord = new ViewRecord() { ViewName = viewName };
            IEnumerable<DataRecord> results = await MySqlHelperUtils.ReadTable(config, viewName);
            viewRecord.Records = results.ToList();
            if (viewName.Equals(ViewRecord.AvailableViewsTableName, StringComparison.OrdinalIgnoreCase))
            {
                viewRecord.RecordCounts = new Dictionary<string, int>();
                foreach (var record in viewRecord.Records)
                {
                    if (record.Fields.TryGetValue("AvailableView", out var viewNameToCount))
                    {
                        string dynamicViewName = viewNameToCount?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(dynamicViewName))
                        { 
                            int recordCountValue = await MySqlHelperUtils.GetTableRecordCount(config, dynamicViewName); 
                            viewRecord.RecordCounts[dynamicViewName] = recordCountValue;
                        }
                    }
                }
            }
            return viewRecord;
        }
        else if (config is ApiClientConfig apiConfig)
        {
            ViewRecordApiClient client = new ViewRecordApiClient(apiConfig);
            var results = await client.GetAllViewRecordsAsJsonAsync(viewName);
            return results ?? new ViewRecord() { ViewName = viewName, Records = new List<DataRecord>() };
        }
        throw new NotImplementedException();
    }
}
