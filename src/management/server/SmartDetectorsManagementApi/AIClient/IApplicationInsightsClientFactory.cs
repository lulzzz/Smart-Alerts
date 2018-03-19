//-----------------------------------------------------------------------
// <copyright file="IApplicationInsightsClientFactory.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi.AIClient
{
    /// <summary>
    /// An interface for exposing a factory that creates Application Insights clients.
    /// </summary>
    public interface IApplicationInsightsClientFactory
    {
        /// <summary>
        /// Creates a new Application Insights Client instance.
        /// </summary>
        /// <returns>A Application Insights client instance.</returns>
        IApplicationInsightsClient GetApplicationInsightsClient();
    }
}
