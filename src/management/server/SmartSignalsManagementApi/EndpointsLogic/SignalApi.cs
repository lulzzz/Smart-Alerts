//-----------------------------------------------------------------------
// <copyright file="SignalApi.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.EndpointsLogic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance;
    using Microsoft.Azure.Monitoring.SmartDetectors.Package;
    using Microsoft.Azure.Monitoring.SmartDetectors.Tools;
    using Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.Models;
    using Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.Responses;

    /// <summary>
    /// This class is the logic for the /signal endpoint.
    /// </summary>
    public class SignalApi : ISignalApi
    {
        private readonly ISmartDetectorRepository smartDetectorRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalApi"/> class.
        /// </summary>
        /// <param name="smartDetectorRepository">The Smart Detector repository.</param>
        public SignalApi(ISmartDetectorRepository smartDetectorRepository)
        {
            Diagnostics.EnsureArgumentNotNull(() => smartDetectorRepository);

            this.smartDetectorRepository = smartDetectorRepository;
        }

        /// <summary>
        /// List all the smart signals.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The smart signals.</returns>
        /// <exception cref="SmartSignalsManagementApiException">This exception is thrown when we failed to retrieve smart signals.</exception>
        public async Task<ListSmartSignalsResponse> GetAllSmartSignalsAsync(CancellationToken cancellationToken)
        {
            try
            {
                IList<SmartDetectorManifest> smartDetectorManifests = await this.smartDetectorRepository.ReadAllSmartDetectorsManifestsAsync(cancellationToken);

                // Convert smart detectors to the required response
                var signals = smartDetectorManifests.Select(manifest => new Signal
                {
                   Id = manifest.Id,
                   Name = manifest.Name,
                   SupportedCadences = new List<int>(manifest.SupportedCadencesInMinutes),
                   SupportedResourceTypes = new List<ResourceType>(manifest.SupportedResourceTypes),
                   Configurations = new List<SignalConfiguration>()
                }).ToList();

                return new ListSmartSignalsResponse()
                {
                    Signals = signals
                };
            }
            catch (Exception e) 
            {
                throw new SmartSignalsManagementApiException("Failed to get smart signals", e, HttpStatusCode.InternalServerError);
            }
        }
    }
}
