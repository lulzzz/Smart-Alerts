//-----------------------------------------------------------------------
// <copyright file="AnalyticsQuery.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliancEmulator.Models
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliancEmulator.ViewModels;

    /// <summary>
    /// Represents an Analytics query.
    /// </summary>
    public class AnalyticsQuery
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsQuery"/> class.
        /// </summary>
        /// <param name="description">The query description</param>
        /// <param name="query">The query</param>
        public AnalyticsQuery(string description, string query)
        {
            this.Description = description;
            this.Query = query;
        }

        /// <summary>
        /// Gets the query description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the query.
        /// </summary>
        public string Query { get; }
    }
}
