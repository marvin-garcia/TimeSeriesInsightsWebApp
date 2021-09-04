// <auto-generated>
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.
//
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Microsoft.Azure.TimeSeriesInsights
{
    using Microsoft.Rest;
    using Models;
    using Newtonsoft.Json;

    /// <summary>
    /// Time Series Insights environment data plane client for Gen2 (GA L1 SKU)
    /// environments.
    /// </summary>
    public partial interface ITimeSeriesInsightsClient : System.IDisposable
    {
        /// <summary>
        /// The base URI of the service.
        /// </summary>

        /// <summary>
        /// Gets or sets json serialization settings.
        /// </summary>
        JsonSerializerSettings SerializationSettings { get; }

        /// <summary>
        /// Gets or sets json deserialization settings.
        /// </summary>
        JsonSerializerSettings DeserializationSettings { get; }

        /// <summary>
        /// Version of the API to be used with the client request. Currently
        /// supported version is "2020-07-31".
        /// </summary>
        string ApiVersion { get; set; }

        /// <summary>
        /// Per environment FQDN, for example
        /// 10000000-0000-0000-0000-100000000109.env.timeseries.azure.com. You
        /// can obtain this domain name from the response of the Get
        /// Environments API, Azure portal, or Azure Resource Manager.
        /// </summary>
        string EnvironmentFqdn { get; set; }

        /// <summary>
        /// Subscription credentials which uniquely identify client
        /// subscription.
        /// </summary>
        ServiceClientCredentials Credentials { get; }


        /// <summary>
        /// Gets the IQuery.
        /// </summary>
        IQuery Query { get; }

        /// <summary>
        /// Gets the IModelSettings.
        /// </summary>
        IModelSettings ModelSettings { get; }

        /// <summary>
        /// Gets the ITimeSeriesInstances.
        /// </summary>
        ITimeSeriesInstances TimeSeriesInstances { get; }

        /// <summary>
        /// Gets the ITimeSeriesTypes.
        /// </summary>
        ITimeSeriesTypes TimeSeriesTypes { get; }

        /// <summary>
        /// Gets the ITimeSeriesHierarchies.
        /// </summary>
        ITimeSeriesHierarchies TimeSeriesHierarchies { get; }

    }
}