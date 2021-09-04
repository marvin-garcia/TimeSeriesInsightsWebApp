using Newtonsoft.Json;

namespace TsiWebApp.Models
{
    public class TimeSeriesInsightsRequest
    {
        [JsonProperty("properties")]
        public Property[] Properties { get; set; }
        [JsonProperty("timeSeriesId")]
        public string[] TimeSeriesId { get; set; }
        [JsonProperty("since")]
        public string Since { get; set; }
    }

    public class Property
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
