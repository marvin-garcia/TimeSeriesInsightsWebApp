// <auto-generated>
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.
//
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Microsoft.Azure.TimeSeriesInsights.Models
{
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Event availability information when environment contains events.
    /// Contains time range of events and approximate distribution of events
    /// over time.
    /// </summary>
    public partial class Availability
    {
        /// <summary>
        /// Initializes a new instance of the Availability class.
        /// </summary>
        public Availability()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the Availability class.
        /// </summary>
        /// <param name="range">Minimum and maximum values of event timestamp
        /// ($ts) property.</param>
        /// <param name="intervalSize">Interval size for the returned
        /// distribution of the events. Returned interval is selected to return
        /// a reasonable number of points. All intervals are the same size. On
        /// the wire interval is specified in ISO-8601 duration format. One
        /// month is always converted to 30 days, and one year is always 365
        /// days. Examples: 1 minute is "PT1M", 1 millisecond is "PT0.001S".
        /// For more information, see
        /// https://www.w3.org/TR/xmlschema-2/#duration</param>
        public Availability(DateTimeRange range = default(DateTimeRange), System.TimeSpan? intervalSize = default(System.TimeSpan?), IDictionary<string, int?> distribution = default(IDictionary<string, int?>))
        {
            Range = range;
            IntervalSize = intervalSize;
            Distribution = distribution;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets minimum and maximum values of event timestamp ($ts) property.
        /// </summary>
        [JsonProperty(PropertyName = "range")]
        public DateTimeRange Range { get; private set; }

        /// <summary>
        /// Gets interval size for the returned distribution of the events.
        /// Returned interval is selected to return a reasonable number of
        /// points. All intervals are the same size. On the wire interval is
        /// specified in ISO-8601 duration format. One month is always
        /// converted to 30 days, and one year is always 365 days. Examples: 1
        /// minute is "PT1M", 1 millisecond is "PT0.001S". For more
        /// information, see https://www.w3.org/TR/xmlschema-2/#duration
        /// </summary>
        [JsonProperty(PropertyName = "intervalSize")]
        public System.TimeSpan? IntervalSize { get; private set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "distribution")]
        public IDictionary<string, int?> Distribution { get; set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="Rest.ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
            if (Range != null)
            {
                Range.Validate();
            }
        }
    }
}
