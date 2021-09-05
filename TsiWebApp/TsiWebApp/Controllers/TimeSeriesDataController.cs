using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TsiWebApp.Models;

namespace TsiWebApp.Controllers
{
    public class TimeSeriesDataController : Controller
    {
        private readonly ITimeSeriesInsightsClient _tsiClient;
        private readonly ILogger<TimeSeriesDataController> _logger;

        public TimeSeriesDataController(ITimeSeriesInsightsClient timeSeriesInsightsClient, ILogger<TimeSeriesDataController> logger) 
        {
            this._logger = logger;

            //string resourceUri = configuration["RESOURCE_URI"];
            //string clientId = configuration["CLIENT_ID"];
            //string clientSecret = configuration["CLIENT_SECRET"];
            //string aadLoginUrl = configuration["AAD_LOGIN_URL"];
            //string tenantId = configuration["TENANT_ID"];
            //string environmentFqdn = configuration["TSI_ENV_FQDN"];
            //this._tsiClient = new TimeSeriesInsightsClient(resourceUri, clientId, clientSecret, aadLoginUrl, tenantId, environmentFqdn);

            this._tsiClient = timeSeriesInsightsClient;
            //this._tsiClient.InitializeAsync().Wait();
        }

        /// <summary>
        /// Display TSI data on HTML format
        /// </summary>
        /// <param name="sensorType">Sensor type: hvac, temp, lighting, occupancy</param>
        /// <param name="since">Only return logs since this time, as a duration: 1h, 20m, 2h30m</param>
        /// <param name="dataFormat">How to display all the data streams: overlapped, separate</param>
        /// <param name="ignoreNull">Whether to ignore null data points</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Index(string sensorType, string since, TimeSeriesInsightsClient.DataFormat dataFormat, bool ignoreNull = true)
        {
            try
            {
                await this._tsiClient.InitializeAsync();
                var timeSeriesInsightsRequest = this._tsiClient.GetRequest(sensorType, since);
                var data = await this._tsiClient.GetEventsAsync(timeSeriesInsightsRequest, dataFormat, ignoreNull);

                string serializedData = JsonConvert.SerializeObject(data);
                ViewData["Data"] = serializedData;

                return View();
            }
            catch (Exception e)
            {
                ViewData["Error"] = e.ToString();
                this._logger.LogError(e.ToString());
                return View("Error");
            }
        }

        /// <summary>
        /// Return TSI data
        /// </summary>
        /// <param name="timeSeriesInsightsRequest">Time series insights request. See https://docs.microsoft.com/en-us/rest/api/time-series-insights/dataaccessgen2/query/execute#getevents for more details on how to create the payload</param>
        /// <param name="dataFormat">How to display all the data streams: overlapped, separate</param>
        /// <param name="ignoreNull">Whether to ignore null data points</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Index([FromBody] TimeSeriesInsightsRequest timeSeriesInsightsRequest, TimeSeriesInsightsClient.DataFormat dataFormat, bool ignoreNull = true)
        {
            try
            {
                await this._tsiClient.InitializeAsync();
                var data = await this._tsiClient.GetEventsAsync(timeSeriesInsightsRequest, dataFormat, ignoreNull);

                ViewData["Data"] = JsonConvert.SerializeObject(data);
                return View();
            }
            catch (Exception e)
            {
                ViewData["Error"] = e.ToString();
                this._logger.LogError(e.ToString());
                return View("Error");
            }
        }
    }
}
