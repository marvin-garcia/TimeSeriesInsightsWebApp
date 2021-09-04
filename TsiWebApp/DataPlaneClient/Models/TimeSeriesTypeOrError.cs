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
    using System.Linq;

    /// <summary>
    /// Result of a batch operation on a particular time series type. Type
    /// object is set when operation is successful and error object is set when
    /// operation is unsuccessful.
    /// </summary>
    public partial class TimeSeriesTypeOrError
    {
        /// <summary>
        /// Initializes a new instance of the TimeSeriesTypeOrError class.
        /// </summary>
        public TimeSeriesTypeOrError()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the TimeSeriesTypeOrError class.
        /// </summary>
        /// <param name="timeSeriesType">Time series type object - set when the
        /// operation is successful.</param>
        /// <param name="error">Error object - set when the operation is
        /// unsuccessful.</param>
        public TimeSeriesTypeOrError(TimeSeriesType timeSeriesType = default(TimeSeriesType), TsiErrorBody error = default(TsiErrorBody))
        {
            TimeSeriesType = timeSeriesType;
            Error = error;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets time series type object - set when the operation is
        /// successful.
        /// </summary>
        [JsonProperty(PropertyName = "timeSeriesType")]
        public TimeSeriesType TimeSeriesType { get; private set; }

        /// <summary>
        /// Gets error object - set when the operation is unsuccessful.
        /// </summary>
        [JsonProperty(PropertyName = "error")]
        public TsiErrorBody Error { get; private set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="Rest.ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
            if (TimeSeriesType != null)
            {
                TimeSeriesType.Validate();
            }
        }
    }
}
