//-----------------------------------------------------------------------
// <copyright file="MetricValues.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Mdm
{
    using System;

    /// <summary>
    /// The aggregated values of a metric in a given timestamp
    /// </summary>
    public class MetricValues
    {
        /// <summary>
        /// Gets or sets the timestamp of the metric
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Gets or sets the aggregated average value 
        /// </summary>
        public double? Average { get; set; }

        /// <summary>
        /// Gets or sets the aggregated minimum value
        /// </summary>
        public double? Minimum { get; set; }

        /// <summary>
        /// Gets or sets the aggregated maximum value
        /// </summary>
        public double? Maximum { get; set; }

        /// <summary>
        /// Gets or sets the aggregated total value (sum)
        /// </summary>
        public double? Total { get; set; }

        /// <summary>
        /// Gets or sets the aggregated count
        /// </summary>
        public long? Count { get; set; }
    }
}