//-----------------------------------------------------------------------
// <copyright file="AzureSubscriptionResources.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.Models
{
    using System.Collections.Generic;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Clients;
    using Newtonsoft.Json;

    /// <summary>
    /// This class represents the model of the results for the GET /resource operation.
    /// </summary>
    public class AzureSubscriptionResources
    {
        /// <summary>
        /// Gets or sets the Azure subscription
        /// </summary>
        [JsonProperty("subscription")]
        public AzureSubscription Subscription { get; set; }

        /// <summary>
        /// Gets or sets the Azure subscription resources
        /// </summary>
        [JsonProperty("resources")]
        public List<ResourceIdentifier> Resources { get; set; }
    }
}
