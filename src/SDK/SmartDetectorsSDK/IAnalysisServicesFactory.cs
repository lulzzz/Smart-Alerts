﻿//-----------------------------------------------------------------------
// <copyright file="IAnalysisServicesFactory.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors.Mdm;

    /// <summary>
    /// An interface for exposing a factory that creates analysis services used for querying
    /// telemetry data by the Smart Detector.
    /// </summary>
    public interface IAnalysisServicesFactory
    {
        /// <summary>
        /// Creates an instance of <see cref="ITelemetryDataClient"/>, used for running queries against data in log analytics workspaces.
        /// </summary>
        /// <param name="resources">The list of resources to analyze.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <exception cref="TelemetryDataClientCreationException">A log analytics telemetry data client could not be created for the specified resources.</exception>
        /// <returns>The telemetry data client, that can be used to run queries on log analytics workspaces.</returns>
        Task<ITelemetryDataClient> CreateLogAnalyticsTelemetryDataClientAsync(IReadOnlyList<ResourceIdentifier> resources, CancellationToken cancellationToken);

        /// <summary>
        /// Creates an instance of <see cref="ITelemetryDataClient"/>, used for running queries against data in Application Insights.
        /// </summary>
        /// <param name="resources">The list of resources to analyze.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <exception cref="TelemetryDataClientCreationException">An Application Insights telemetry data client could not be created for the specified resources.</exception>
        /// <returns>The telemetry data client, that can be used to run queries on Application Insights.</returns>
        Task<ITelemetryDataClient> CreateApplicationInsightsTelemetryDataClientAsync(IReadOnlyList<ResourceIdentifier> resources, CancellationToken cancellationToken);

        /// <summary>
        /// Creates an instance of <see cref="IMdmClient"/>, used to fetch the resource metrics from MDM.
        /// </summary>
        /// <param name="resource">The resource to analyze.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The MDM client, that can be used to fetch the resource metrics from MDM.</returns>
        IMdmClient CreateMdmClientAsync(ResourceIdentifier resource, CancellationToken cancellationToken);
    }
}