//-----------------------------------------------------------------------
// <copyright file="ApplicationInsightsClientFactory.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi.AIClient
{
    using Microsoft.Azure.Monitoring.SmartDetectors.Clients;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance;

    /// <summary>
    /// An implementation of the <see cref="IApplicationInsightsClientFactory"/> interface.
    /// </summary>
    public class ApplicationInsightsClientFactory : IApplicationInsightsClientFactory
    {
        private readonly ICredentialsFactory credentialsFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationInsightsClientFactory"/> class.
        /// </summary>
        /// <param name="credentialsFactory">The credentials factory.</param>
        public ApplicationInsightsClientFactory(ICredentialsFactory credentialsFactory)
        {
            this.credentialsFactory = credentialsFactory;
        }

        /// <summary>
        /// Creates a new Application Insights Client instance.
        /// </summary>
        /// <returns>A Application Insights client instance.</returns>
        public IApplicationInsightsClient GetApplicationInsightsClient()
        {
            string applicationId = ConfigurationReader.ReadConfig("TelemetryApplicationId", required: true);

            return new ApplicationInsightsClient(applicationId, this.credentialsFactory);
        }
    }
}
