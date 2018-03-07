//-----------------------------------------------------------------------
// <copyright file="AnalysisServicesFactory.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Presentation;
    using Microsoft.Azure.Monitoring.SmartDetectors.Tools;

    /// <summary>
    /// An implementation of the <see cref="IAnalysisServicesFactory"/> interface.
    /// </summary>
    public class AnalysisServicesFactory : IAnalysisServicesFactory
    {
        private readonly ITracer tracer;
        private readonly IHttpClientWrapper httpClientWrapper;
        private readonly ICredentialsFactory credentialsFactory;
        private readonly IAzureResourceManagerClient azureResourceManagerClient;
        private readonly IQueryRunInfoProvider queryRunInfoProvider;
        private readonly TimeSpan queryTimeout;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisServicesFactory"/> class.
        /// </summary>
        /// <param name="tracer">The tracer</param>
        /// <param name="httpClientWrapper">The HTTP client wrapper.</param>
        /// <param name="credentialsFactory">The credentials factory.</param>
        /// <param name="azureResourceManagerClient">The azure resource manager client.</param>
        /// <param name="queryRunInfoProvider">The query run information provider.</param>
        public AnalysisServicesFactory(ITracer tracer, IHttpClientWrapper httpClientWrapper, ICredentialsFactory credentialsFactory, IAzureResourceManagerClient azureResourceManagerClient, IQueryRunInfoProvider queryRunInfoProvider)
        {
            this.tracer = tracer;
            this.httpClientWrapper = httpClientWrapper;
            this.credentialsFactory = credentialsFactory;
            this.azureResourceManagerClient = azureResourceManagerClient;
            this.queryRunInfoProvider = queryRunInfoProvider;

            // string timeoutString = ConfigurationReader.ReadConfig("AnalyticsQueryTimeoutInMinutes", required: true);
            string timeoutString = "15";
            this.queryTimeout = TimeSpan.FromMinutes(int.Parse(timeoutString));
        }

        /// <summary>
        /// Creates an instance of <see cref="ITelemetryDataClient"/>, used for running queries against data in log analytics workspaces.
        /// </summary>
        /// <param name="resources">The list of resources to analyze.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <exception cref="TelemetryDataClientCreationException">A log analytics telemetry data client could not be created for the specified resources.</exception>
        /// <returns>The telemetry data client, that can be used to run queries on log analytics workspaces.</returns>
        public async Task<ITelemetryDataClient> CreateLogAnalyticsTelemetryDataClientAsync(IReadOnlyList<ResourceIdentifier> resources, CancellationToken cancellationToken)
        {
            // Get the query run info, and verify it
            QueryRunInfo runInfo = await this.queryRunInfoProvider.GetQueryRunInfoAsync(resources, cancellationToken);
            this.VerifyRunInfo(runInfo, TelemetryDbType.LogAnalytics);

            // Get workspace Id (for the 1st workspace)
            ResourceIdentifier firstWorkspace = ResourceIdentifier.CreateFromResourceId(runInfo.ResourceIds[0]);
            string firstWorkspaceId = await this.azureResourceManagerClient.GetLogAnalyticsWorkspaceIdAsync(firstWorkspace, cancellationToken);

            // Create the client
            return new LogAnalyticsTelemetryDataClient(this.tracer, this.httpClientWrapper, this.credentialsFactory, firstWorkspaceId, runInfo.ResourceIds, this.queryTimeout);
        }

        /// <summary>
        /// Creates an instance of <see cref="ITelemetryDataClient"/>, used for running queries against data in Application Insights.
        /// </summary>
        /// <param name="resources">The list of resources to analyze.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <exception cref="TelemetryDataClientCreationException">An Application Insights telemetry data client could not be created for the specified resources.</exception>
        /// <returns>The telemetry data client, that can be used to run queries on Application Insights.</returns>
        public async Task<ITelemetryDataClient> CreateApplicationInsightsTelemetryDataClientAsync(IReadOnlyList<ResourceIdentifier> resources, CancellationToken cancellationToken)
        {
            // Get the query run info, and verify it
            QueryRunInfo runInfo = await this.queryRunInfoProvider.GetQueryRunInfoAsync(resources, cancellationToken);
            this.VerifyRunInfo(runInfo, TelemetryDbType.ApplicationInsights);

            // Get application Id (for the 1st application)
            ResourceIdentifier firstApplication = ResourceIdentifier.CreateFromResourceId(runInfo.ResourceIds[0]);
            string firstApplicationId = await this.azureResourceManagerClient.GetApplicationInsightsAppIdAsync(firstApplication, cancellationToken);

            // Create the client
            return new ApplicationInsightsTelemetryDataClient(this.tracer, this.httpClientWrapper, this.credentialsFactory, firstApplicationId, resources.Select(resource => resource.ToResourceId()), this.queryTimeout);
        }

        /// <summary>
        /// Creates an instance of <see cref="IMdmClient"/>, used to fetch the resource metrics from MDM.
        /// </summary>
        /// <param name="resource">The resource to analyze.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The MDM client, that can be used to fetch the resource metrics from MDM.</returns>
        public IMdmClient CreateMdmClientAsync(ResourceIdentifier resource, CancellationToken cancellationToken)
        {
            // Create the client
            return new MdmClient(this.tracer, this.credentialsFactory, resource);
        }

        /// <summary>
        /// Perform basic validations on the specified query run information.
        /// </summary>
        /// <param name="runInfo">The query run information</param>
        /// <param name="expectedType">The expected telemetry DB type</param>
        private void VerifyRunInfo(QueryRunInfo runInfo, TelemetryDbType expectedType)
        {
            // Verify the telemetry DB type
            if (runInfo.Type != expectedType)
            {
                throw new TelemetryDataClientCreationException($"Telemetry client creation failed - telemetry resource type is {runInfo.Type}");
            }

            // Verify that the resource IDs are not empty
            if (!runInfo.ResourceIds.Any())
            {
                throw new TelemetryDataClientCreationException("Telemetry client creation failed - no resources found");
            }
        }
    }
}