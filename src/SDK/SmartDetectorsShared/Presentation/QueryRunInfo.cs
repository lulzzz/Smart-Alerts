//-----------------------------------------------------------------------
// <copyright file="QueryRunInfo.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Presentation
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// This class provides information on how to run queries that are part of a <see cref="AlertPresentation"/> object. 
    /// </summary>
    public class QueryRunInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryRunInfo"/> class.
        /// </summary>
        /// <param name="type">The telemetry database type</param>
        /// <param name="resourceIds">The telemetry resource Ids</param>
        public QueryRunInfo(TelemetryDbType type, IReadOnlyList<string> resourceIds)
        {
            this.Type = type;
            this.ResourceIds = resourceIds;
        }

        /// <summary>
        /// Gets the telemetry database type
        /// </summary>
        [JsonProperty("type")]
        public TelemetryDbType Type { get; }

        /// <summary>
        /// Gets the telemetry resource Ids
        /// </summary>
        [JsonProperty("resourceIds")]
        public IReadOnlyList<string> ResourceIds { get; }
    }
}
