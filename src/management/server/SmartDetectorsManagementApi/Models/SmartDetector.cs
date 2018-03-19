//-----------------------------------------------------------------------
// <copyright file="SmartDetector.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi.Models
{
    using System.Collections.Generic;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Newtonsoft.Json;

    /// <summary>
    /// This class represents a Smart Detector model for the Management API responses.
    /// </summary>
    public class SmartDetector
    {
        /// <summary>
        /// Gets or sets the Smart Detector id.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the Smart Detector name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Smart Detector supported cadences (in minutes).
        /// </summary>
        [JsonProperty("supportedCadences")]
        public List<int> SupportedCadences { get; set; }

        /// <summary>
        /// Gets or sets the Smart Detector supported resource types.
        /// </summary>
        [JsonProperty("supportedResourceTypes")]
        public List<ResourceType> SupportedResourceTypes { get; set; }

        /// <summary>
        /// Gets or sets the Smart Detector configurations.
        /// </summary>
        [JsonProperty("configurations")]
        public List<SmartDetectorConfiguration> Configurations { get; set; }
    }
}
