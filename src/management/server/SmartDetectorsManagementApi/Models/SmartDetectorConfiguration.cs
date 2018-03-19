//-----------------------------------------------------------------------
// <copyright file="SmartDetectorConfiguration.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// This class represents a Smart Detector configuration for the Management API responses.
    /// </summary>
    public class SmartDetectorConfiguration
    {
        /// <summary>
        /// Gets or sets the Smart Detector configuration id.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the Smart Detector configuration name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Smart Detector configuration type.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
