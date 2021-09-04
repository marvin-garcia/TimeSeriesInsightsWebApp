using Microsoft.Azure.TimeSeriesInsights.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TsiWebApp.Models
{
    public class TimeSeriesInsightsResult
    {
        [JsonProperty("timeSeriesId")]
        public string TimeSeriesId { get; set; }
        [JsonProperty("result")]
        public IList<QueryResultPage> QueryResultPages { get; set; }
    }
}
