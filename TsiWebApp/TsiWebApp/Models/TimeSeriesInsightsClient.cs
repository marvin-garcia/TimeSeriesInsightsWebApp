using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Rest;
using Microsoft.Azure;
using Microsoft.Azure.TimeSeriesInsights;
using Microsoft.Azure.TimeSeriesInsights.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net.Http;

namespace TsiWebApp.Models
{
    public interface ITimeSeriesInsightsClient
    {
        Task InitializeAsync();
        TimeSeriesInsightsRequest GetRequest(string sensorType, string since);
        Task<List<Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, object>>>>>> GetEventsAsync(TimeSeriesInsightsRequest timeSeriesInsightsRequest, TimeSeriesInsightsClient.DataFormat dataFormat, bool ignoreNull = false);
    }

    public class TimeSeriesInsightsClient : ITimeSeriesInsightsClient
    {
        private readonly string _resourceUri;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _aadLoginUrl;
        private readonly string _tenantId;
        private readonly string _environmentFqdn;
        private readonly HttpClient HttpClient;
        private readonly ILogger<ITimeSeriesInsightsClient> _logger;
        private Microsoft.Azure.TimeSeriesInsights.TimeSeriesInsightsClient Client { get; set; }

        /// <summary>
        /// Determines how to plot different data streams
        /// </summary>
        public enum DataFormat
        {
            Overlapped,
            Separate,
        }

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="configuration"></param>
        public TimeSeriesInsightsClient(IConfiguration configuration, HttpClient httpClient, ILogger<ITimeSeriesInsightsClient> logger)
        {
            _logger = logger;
            HttpClient = httpClient;
            _resourceUri = configuration["RESOURCE_URI"];
            _clientId = configuration["CLIENT_ID"];
            _clientSecret = configuration["CLIENT_SECRET"];
            _aadLoginUrl = configuration["AAD_LOGIN_URL"];
            _tenantId = configuration["TENANT_ID"];
            _environmentFqdn = configuration["TSI_ENV_FQDN"];
        }

        /// <summary>
        /// Initialize client, authenticates with Azure Active Directory using service principal credentials
        /// </summary>
        /// <returns></returns>
        public async Task InitializeAsync()
        {
            try
            {
                AuthenticationContext context = new AuthenticationContext($"{new Uri(_aadLoginUrl)}/{_tenantId}", TokenCache.DefaultShared);
                AuthenticationResult authenticationResult = await context.AcquireTokenAsync(_resourceUri, new ClientCredential(_clientId, _clientSecret));

                TokenCloudCredentials tokenCloudCredentials = new TokenCloudCredentials(authenticationResult.AccessToken);
                ServiceClientCredentials serviceClientCredentials = new TokenCredentials(tokenCloudCredentials.Token);

                this.Client = new Microsoft.Azure.TimeSeriesInsights.TimeSeriesInsightsClient(credentials: serviceClientCredentials)
                {
                    EnvironmentFqdn = _environmentFqdn,
                };
            }
            catch (Exception e)
            {
                this._logger.LogError(e.ToString());
                throw e;
            }
        }

        /// <summary>
        /// Formulates data request given sensorType and since parameters
        /// </summary>
        /// <param name="sensorType">Sensor type: hvac, temp, lighting, occupancy</param>
        /// <param name="since">Only return logs since this time, as a duration: 1h, 20m, 2h30m</param>
        /// <returns></returns>
        public TimeSeriesInsightsRequest GetRequest(string sensorType, string since)
        {
            try
            {
                var timeSeriesRequest = new TimeSeriesInsightsRequest()
                {
                    TimeSeriesId = Enumerable.Range(1, 4).Select(x => { return $"{sensorType.ToLower()}sensor{x}"; }).ToArray(),
                    Since = since,
                    Properties = new Property[]
                    {
                    new Property()
                    {
                        Name = sensorType switch
                        {
                            "hvac" => "airflow",
                            "lighting" => "State",
                            "temp" => "temperature",
                            "occupancy" => "IsOccupied",
                            _ => throw new Exception($"sensor type '{sensorType}' is not defined"),
                        },
                        Type = sensorType switch
                        {
                            "hvac" => "Long",
                            "lighting" => "Long",
                            "temp" => "Double",
                            "occupancy" => "Long",
                            _ => throw new Exception($"sensor type '{sensorType}' is not defined"),
                        }
                    }
                    }
                };

                return timeSeriesRequest;
            }
            catch (Exception e)
            {
                this._logger.LogError(e.ToString());
                throw e;
            }
        }

        /// <summary>
        /// Gets event data from TSI and formats it to pass it to the Javascript SDK
        /// </summary>
        /// <param name="timeSeriesInsightsRequest"></param>
        /// <param name="dataFormat"></param>
        /// <param name="ignoreNull"></param>
        /// <returns></returns>
        public async Task<List<Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, object>>>>>> GetEventsAsync(TimeSeriesInsightsRequest timeSeriesInsightsRequest, DataFormat dataFormat, bool ignoreNull = false)
        {
            try
            {
                var timeSeriesInsightsResults = await this.GetEventsAsync(timeSeriesInsightsRequest);


                var data = new List<Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, object>>>>>();

                if (dataFormat == DataFormat.Overlapped)
                    data = this.ConvertToOverlappedSensorData(timeSeriesInsightsResults, ignoreNull);
                else if (dataFormat == DataFormat.Separate)
                    data = this.ConvertToSeparateSensorData(timeSeriesInsightsResults, ignoreNull);

                return data;
            }
            catch (Exception e)
            {
                this._logger.LogError(e.ToString());
                throw e;
            }
        }

        /// <summary>
        /// Gets raw data from TSI
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task<List<TimeSeriesInsightsResult>> GetEventsAsync(TimeSeriesInsightsRequest request)
        {
            try
            {
                DateTime to = DateTime.UtcNow;
                DateTime from = to;
                Match match = Regex.Match(request.Since, @"((\d+?)h)?((\d+?)m)?((\d+?)s)?");

                if (!string.IsNullOrEmpty(match.Groups[2].Value))
                    from = from.AddHours(-1 * Convert.ToInt32(match.Groups[2].Value));

                if (!string.IsNullOrEmpty(match.Groups[4].Value))
                    from = from.AddMinutes(-1 * Convert.ToInt32(match.Groups[4].Value));

                if (!string.IsNullOrEmpty(match.Groups[6].Value))
                    from = from.AddSeconds(-1 * Convert.ToInt32(match.Groups[6].Value));

                List<TimeSeriesInsightsResult> timeSeriesInsightsResults = new List<TimeSeriesInsightsResult>();
                foreach (var id in request.TimeSeriesId)
                {
                    List<QueryResultPage> queryResultPages = new List<QueryResultPage>() { };
                    QueryRequest queryRequest = new QueryRequest(
                        getEvents: new GetEvents(
                            timeSeriesId: new string[] { id }, // have to query each time series id at a time because if ids and timestamps don't match, API returns an error
                            searchSpan: new DateTimeRange()
                            {
                                FromProperty = from,
                                To = to,
                            },
                            projectedProperties: request.Properties.Select(x => { return new EventProperty(x.Name, x.Type); }).ToArray(),
                            filter: null));

                    string continuationToken;
                    do
                    {
                        QueryResultPage queryResponse = await this.Client.Query.ExecuteAsync(queryRequest);
                        queryResultPages.Add(queryResponse);

                        continuationToken = queryResponse.ContinuationToken;
                    }
                    while (continuationToken != null);

                    timeSeriesInsightsResults.Add(new TimeSeriesInsightsResult()
                    {
                        TimeSeriesId = id,
                        QueryResultPages = queryResultPages,
                    });
                }

                return timeSeriesInsightsResults;
            }
            catch (Exception e)
            {
                this._logger.LogError(e.ToString());
                throw e;
            }
        }

        /// <summary>
        /// Converts raw TSI data to overlapped mode
        /// </summary>
        /// <param name="timeSeriesInsightsResults"></param>
        /// <param name="ignoreNull"></param>
        /// <returns></returns>
        private List<Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, object>>>>> ConvertToOverlappedSensorData(List<TimeSeriesInsightsResult> timeSeriesInsightsResults, bool ignoreNull = false)
        {
            try
            {
                var dataGroup = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, object>>>>();
                var sensorData = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>();
                foreach (var timeSeriesInsightsResult in timeSeriesInsightsResults)
                {
                    var timestampData = new Dictionary<string, Dictionary<string, object>>();
                    for (int queryResultIndex = 0; queryResultIndex < timeSeriesInsightsResult.QueryResultPages.Count(); queryResultIndex++)
                    {
                        for (int timestampIndex = 0; timestampIndex < timeSeriesInsightsResult.QueryResultPages[queryResultIndex].Timestamps.Count(); timestampIndex++)
                        {
                            string timestamp = timeSeriesInsightsResult.QueryResultPages[queryResultIndex].Timestamps[timestampIndex].Value.ToString("yyyy-MM-ddThh:mm:ss.fffZ");

                            var propertyData = new Dictionary<string, object>();
                            for (int propertyIndex = 0; propertyIndex < timeSeriesInsightsResult.QueryResultPages[queryResultIndex].Properties.Count(); propertyIndex++)
                            {
                                string propertyName = timeSeriesInsightsResult.QueryResultPages[queryResultIndex].Properties[propertyIndex].Name;
                                object propertyValue = timeSeriesInsightsResult.QueryResultPages[queryResultIndex].Properties[propertyIndex].Values[timestampIndex];

                                if (propertyValue != null || !ignoreNull)
                                    propertyData.Add(propertyName, propertyValue);
                            }

                            if (propertyData.Count() > 0)
                                timestampData.Add(timestamp, propertyData);
                        }
                    }

                    sensorData.Add(timeSeriesInsightsResult.TimeSeriesId, timestampData);
                }

                dataGroup.Add("Sensors", sensorData);

                var dataArray = new List<Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, object>>>>>() { dataGroup };

                return dataArray;
            }
            catch (Exception e)
            {
                this._logger.LogError(e.ToString());
                throw e;
            }
        }

        /// <summary>
        /// Converts raw TSI data to separate mode
        /// </summary>
        /// <param name="timeSeriesInsightsResults"></param>
        /// <param name="ignoreNull"></param>
        /// <returns></returns>
        private List<Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, object>>>>> ConvertToSeparateSensorData(List<TimeSeriesInsightsResult> timeSeriesInsightsResults, bool ignoreNull = false)
        {
            try
            {
                var dataGroup = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, object>>>>();
                foreach (var timeSeriesInsightsResult in timeSeriesInsightsResults)
                {
                    var sensorData = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>();

                    var timestampData = new Dictionary<string, Dictionary<string, object>>();
                    for (int queryResultIndex = 0; queryResultIndex < timeSeriesInsightsResult.QueryResultPages.Count(); queryResultIndex++)
                    {
                        for (int timestampIndex = 0; timestampIndex < timeSeriesInsightsResult.QueryResultPages[queryResultIndex].Timestamps.Count(); timestampIndex++)
                        {
                            string timestamp = timeSeriesInsightsResult.QueryResultPages[queryResultIndex].Timestamps[timestampIndex].Value.ToString("yyyy-MM-ddThh:mm:ss.fffZ");

                            var propertyData = new Dictionary<string, object>();
                            for (int propertyIndex = 0; propertyIndex < timeSeriesInsightsResult.QueryResultPages[queryResultIndex].Properties.Count(); propertyIndex++)
                            {
                                string propertyName = timeSeriesInsightsResult.QueryResultPages[queryResultIndex].Properties[propertyIndex].Name;
                                object propertyValue = timeSeriesInsightsResult.QueryResultPages[queryResultIndex].Properties[propertyIndex].Values[timestampIndex];

                                if (propertyValue != null || !ignoreNull)
                                    propertyData.Add(propertyName, propertyValue);
                            }

                            if (propertyData.Count() > 0)
                                timestampData.Add(timestamp, propertyData);
                        }
                    }

                    sensorData.Add(timeSeriesInsightsResult.TimeSeriesId, timestampData);
                    dataGroup.Add(timeSeriesInsightsResult.TimeSeriesId, sensorData);
                }

                var dataArray = new List<Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, object>>>>>() { dataGroup };

                return dataArray;
            }
            catch (Exception e)
            {
                this._logger.LogError(e.ToString());
                throw e;
            }
        }
    }
}
