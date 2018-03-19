//-----------------------------------------------------------------------
// <copyright file="AlertsPublisher.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Scheduler.Publisher
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.AzureStorage;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Scheduler.Exceptions;
    using Microsoft.Azure.Monitoring.SmartDetectors.Tools;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Newtonsoft.Json;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Contracts.Alert;

    /// <summary>
    /// This class is responsible for publishing Alerts.
    /// </summary>
    public class AlertsPublisher : IAlertsPublisher
    {
        private const string ResultEventName = "Alerts";

        private readonly ITracer tracer;
        private readonly ICloudBlobContainerWrapper containerClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="AlertsPublisher"/> class.
        /// </summary>
        /// <param name="tracer">The tracer to use.</param>
        /// <param name="storageProviderFactory">The Azure storage provider factory.</param>
        public AlertsPublisher(ITracer tracer, ICloudStorageProviderFactory storageProviderFactory)
        {
            this.tracer = Diagnostics.EnsureArgumentNotNull(() => tracer);
            this.containerClient = storageProviderFactory.GetAlertsStorageContainer();
        }

        /// <summary>
        /// Publish Alerts as events to Application Insights
        /// </summary>
        /// <param name="smartDetectorId">The Smart Detector ID</param>
        /// <param name="alerts">The Alerts to publish</param>
        /// <returns>A <see cref="Task"/> object, running the current operation</returns>
        public async Task PublishAlertsAsync(string smartDetectorId, IList<ContractsAlert> alerts)
        {
            if (alerts == null || !alerts.Any())
            {
                this.tracer.TraceInformation($"no alerts to publish for Smart Detector {smartDetectorId}");
                return;
            }

            try
            {
                var todayString = DateTime.UtcNow.ToString("yyyy-MM-dd");
                foreach (var alert in alerts)
                {
                    var blobName = $"{smartDetectorId}/{todayString}/{alert.Id}";
                    var alertString = JsonConvert.SerializeObject(alert);
                    ICloudBlob blob = await this.containerClient.UploadBlobAsync(blobName, alertString, CancellationToken.None);

                    var eventProperties = new Dictionary<string, string>
                    {
                        { "SmartDetectorId", smartDetectorId },
                        { "AlertBlobUri", blob.Uri.AbsoluteUri }
                    };

                    this.tracer.TrackEvent(ResultEventName, eventProperties);
                }
            }
            catch (StorageException e)
            {
                this.tracer.TraceError($"Failed to publish alerts to storage for {smartDetectorId} with exception: {e}");
                throw new AlertsPublishException($"Failed to publish alerts to storage for {smartDetectorId}", e);
            }

            this.tracer.TraceInformation($"{alerts.Count} Alerts for Smart Detector {smartDetectorId} were published to the results store");
        }
    }
}
