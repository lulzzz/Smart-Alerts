//-----------------------------------------------------------------------
// <copyright file="MetricTimeSeries.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Mdm
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a time series of metric values
    /// </summary>
    public class MetricTimeSeries
    {
        /// <summary>
        /// Gets or sets the data points of metric values
        /// </summary>
        public IList<MetricValues> Data { get; set; }

        /// <summary>
        /// Gets or sets the meta data regarding the list of data points
        /// </summary>
        public IList<string> MetaData { get; set; }
    }
}
