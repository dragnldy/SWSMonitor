using System.Text.Json.Serialization;

namespace DataLibrary.DataSources
{
    public class MyAirTableRecordBase
    {
        public class Fields
        {
            [JsonPropertyName("URL")]
            [JsonInclude]
            public string BeachName { get; set; }
        }


        public class Record
        {
            [JsonPropertyName("id")]
            [JsonInclude]
            public string? airTableId { get; set; }

            [JsonPropertyName("createdTime")]
            [JsonInclude]
            public DateTime? created { get; set; }

            [JsonPropertyName("fields")]
            [JsonInclude]
            public Fields? fields { get; set; }
        }

        public class Root
        {
            [JsonPropertyName("records")]
            [JsonInclude]
            public List<Record> records { get; set; }
        }
    }
}
