//-----------------------------------------------------------------------
// <copyright file="QueryParameters.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Mdm
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// MDM properties to be used when fetching data from MDM. All fields are optional.
    /// <see href="https://docs.microsoft.com/en-us/powershell/module/azurerm.insights/get-azurermmetricdefinition?view=azurermps-5.4.0">Use Get-AzureRmMetricDefinition, to fetch for available metric names, granularity, etc.</see>
    /// </summary>
    public class QueryParameters
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
        /// Gets a string representation of the timespan from which we want to fetch data.
        /// </summary>
        public string TimeRange
        {
            get
            {
                const string TimeFormat = "yyyy-MM-ddTHH:mm:ssZ";

                string timespan = null;
                if (this.StartTime.HasValue && this.EndTime.HasValue)
                {
                    timespan = $"{this.StartTime.Value.ToString(TimeFormat)}/{this.EndTime.Value.ToString(TimeFormat)}";
                }

                return timespan;
            }
        }

        /// <summary>
        /// Gets or sets the resolution\ granularity of the results
        /// </summary>
        public TimeSpan? Interval { get; set; }

        /// <summary>
        /// Gets or sets the name of the resource metric to be fetched. E.g. those are the metrics for azure storage queues: QueueMessageCount, QueueCapacity, QueueCount, QueueMessageCount, Transactions, Ingress, Egress.
        /// Use the MDM client to fetch all metric definitions, including the existing metric names. 
        /// Fetch multiple metrics by using comma separated string. E.g: "QueueMessageCount, QueueCapacity"
        /// </summary>
        public string MetricName { get; set; }

        /// <summary>
        /// Gets or sets the data aggregation types to perform on the fetched data
        /// </summary>
        public List<Aggregation> Aggregations { get; set; }

        /// <summary>
        /// Gets or sets the field to order the fetched results by
        /// </summary>
        public string Orderby { get; set; }

        /// <summary>
        /// Gets or sets the amount of results to be fetched
        /// </summary>
        public int? Top { get; set; }

        /// <summary>
        /// Gets or sets the filter to be used, based on the metric's dimensions. 
        /// E.g. for Queues: <c>"ApiName eq 'GetMessage' or ApiName eq 'GetMessages'"</c>
        /// The result for each dimension will be returned in a different TimeSeries.
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// Returns a string that represents the MDM query parameters.
        /// </summary>
        /// <returns>A string that represents the MDM query parameters</returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
