//-----------------------------------------------------------------------
// <copyright file="MdmQueryResult.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Mdm
{
    using System.Collections.Generic;

    /// <summary>
    /// An object which represents an MDM query result for a single metric.
    /// </summary>
    public class MdmQueryResult
    {
        /// <summary>
        /// Gets or sets the metric's name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the metric's value unit (Seconds, bytes, etc.)
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Gets or sets the metric's time series.
        /// A timeseries is created per dimension value, if a filter by dimension's value is set
        /// E.g., if we set <c>filter = "ApiName eq 'PutMessage' or ApiName eq 'GetMessages'"</c>, the result will contain 2 time series.
        /// </summary>
        public IList<MetricTimeSeries> Timeseries { get; set; }
    }
}