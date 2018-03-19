//-----------------------------------------------------------------------
// <copyright file="ListSmartDetectorsResponse.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi.Responses
{
    using System.Collections.Generic;
    using Models;
    using Newtonsoft.Json;

    /// <summary>
    /// This class represents the GET Management API operation for listing Smart Detectors.
    /// </summary>
    public class ListSmartDetectorsResponse
    {
        /// <summary>
        /// Gets or sets the Smart Detectors list
        /// </summary>
        [JsonProperty("smartDetectors")]
        public IList<SmartDetector> SmartDetectors { get; set; }
    }
}
