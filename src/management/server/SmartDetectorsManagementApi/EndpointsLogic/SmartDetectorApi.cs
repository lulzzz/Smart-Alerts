//-----------------------------------------------------------------------
// <copyright file="SmartDetectorApi.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi.EndpointsLogic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Contracts;
    using Microsoft.Azure.Monitoring.SmartDetectors.Package;
    using Microsoft.Azure.Monitoring.SmartDetectors.Tools;

    /// <summary>
    /// This class is the logic for the /smartDetector endpoint.
    /// </summary>
    public class SmartDetectorApi : ISmartDetectorApi
    {
        private readonly ISmartDetectorRepository smartDetectorRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartDetectorApi"/> class.
        /// </summary>
        /// <param name="smartDetectorRepository">The Smart Detector repository.</param>
        public SmartDetectorApi(ISmartDetectorRepository smartDetectorRepository)
        {
            Diagnostics.EnsureArgumentNotNull(() => smartDetectorRepository);

            this.smartDetectorRepository = smartDetectorRepository;
        }

        /// <summary>
        /// List all the Smart Detectors.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The Smart Detectors.</returns>
        /// <exception cref="SmartDetectorsManagementApiException">This exception is thrown when we failed to retrieve Smart Detectors.</exception>
        public async Task<ListSmartDetectorsResponse> GetAllSmartDetectorsAsync(CancellationToken cancellationToken)
        {
            try
            {
                IList<SmartDetectorManifest> smartDetectorManifests = await this.smartDetectorRepository.ReadAllSmartDetectorsManifestsAsync(cancellationToken);

                // Convert Smart Detectors to the required response
                var detectors = smartDetectorManifests.Select(manifest => new SmartDetector
                {
                   Id = manifest.Id,
                   Name = manifest.Name,
                   SupportedCadences = new List<int>(manifest.SupportedCadencesInMinutes),

                   // We need to cast the resource type since the resource type objects are duplicated both in the SDK and in the contracts package so they have different namespaces
                   SupportedResourceTypes = manifest.SupportedResourceTypes.Select(resourceType => (ResourceType)resourceType).ToList(),
                   Configurations = new List<SmartDetectorConfiguration>()
                }).ToList();

                return new ListSmartDetectorsResponse()
                {
                    SmartDetectors = detectors
                };
            }
            catch (Exception e) 
            {
                throw new SmartDetectorsManagementApiException("Failed to get Smart Detectors", e, HttpStatusCode.InternalServerError);
            }
        }
    }
}
