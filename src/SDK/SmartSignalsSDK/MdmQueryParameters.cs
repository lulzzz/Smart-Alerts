//-----------------------------------------------------------------------
// <copyright file="MdmQueryParameters.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals
{
    using System;
    using Microsoft.Azure.Management.Monitor.Fluent.Models;

    /// <summary>
    /// MDM properties to be used when fetching data from MDM. All fields are optional
    /// </summary>
    public class MdmQueryParameters
    {
        /// <summary>
        /// Gets or sets the start time of the time range
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time of the time range
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Gets a string representation of the timespan from which we want to fetch data
        /// </summary>
        public string TimeRange
        {
            get
            {
                string timeFormat = "yyyy-MM-ddTHH:mm:ssZ";

                string timespan = null;
                if (this.StartTime.HasValue && this.EndTime.HasValue)
                {
                    timespan = $"{this.StartTime.Value.ToString(timeFormat)}/{this.EndTime.Value.ToString(timeFormat)}";
                }

                return timespan;
            }
        }

        /// <summary>
        /// Gets or sets the resolution\ granularity of the results
        /// </summary>
        public TimeSpan? Interval { get; set; }

        /// <summary>
        /// Gets or sets the name of the resource metric to be fetched. E.g. for azure storage queues: QueueMessageCount, QueueCapacity, QueueCount, QueueMessageCount, Transactions, Ingress, Egress.
        /// Use the MDM client to fetch all metric definitions, including the metric names. 
        /// </summary>
        public string MetricName { get; set; }

        /// <summary>
        /// Gets or sets the data aggregation type to perform on the fetched data
        /// </summary>
        public AggregationType? Aggregation { get; set; }

        /// <summary>
        /// Gets or sets the field to order the fetched results by
        /// </summary>
        public string Orderby { get; set; }

        /// <summary>
        /// Gets or sets the amount of results to be fetched
        /// </summary>
        public int? Top { get; set; }

        /// <summary>
        /// Gets or sets the result type: Data or meta data
        /// </summary>
        public ResultType? ResultType { get; set; }

        /// <summary>
        /// Gets or sets the filter to be used, based on the metric's dimensions. E.g. for Queues: <c>"ApiName eq 'GetMessage' or ApiName eq 'GetMessages'"</c>
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// Returns a string that represents the MDM query parameters.
        /// </summary>
        /// <returns>A string that represents the MDM query parameters</returns>
        public override string ToString()
        {
            return $"StartTime - '{this.StartTime}', EndTime - '{this.EndTime}', Interval - '{this.Interval}', MetricName - '{this.MetricName}', " +
                   $"Aggregation - '{this.Aggregation}' Top - '{this.Top}', Orderby - '{this.Orderby}', Filter - '{this.Filter}', ResultType - '{this.ResultType}'";
        }
    }
}
