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
        /// Initializes a new instance of the <see cref="MetricValues"/> class 
        /// </summary>
        /// <param name="timeStamp">The timestamp of the metric</param>
        /// <param name="average">The aggregated average value</param>
        /// <param name="minimum">The aggregated minimum value</param>
        /// <param name="maximum">The aggregated maximum value</param>
        /// <param name="total">The aggregated total value (sum)</param>
        /// <param name="count">The aggregated count</param>
        public MetricValues(DateTime timeStamp, double? average = null, double? minimum = null, double? maximum = null, double? total = null, long? count = null)
        {
            this.TimeStamp = timeStamp;
            this.Average = average;
            this.Minimum = minimum;
            this.Maximum = maximum;
            this.Total = total;
            this.Count = count;
        }

        /// <summary>
        /// Gets the timestamp of the metric
        /// </summary>
        public DateTime TimeStamp { get; }

        /// <summary>
        /// Gets the aggregated average value 
        /// </summary>
        public double? Average { get; }

        /// <summary>
        /// Gets the aggregated minimum value
        /// </summary>
        public double? Minimum { get; }

        /// <summary>
        /// Gets the aggregated maximum value
        /// </summary>
        public double? Maximum { get; }

        /// <summary>
        /// Gets the aggregated total value (sum)
        /// </summary>
        public double? Total { get; }

        /// <summary>
        /// Gets the aggregated count
        /// </summary>
        public long? Count { get; }
    }
}