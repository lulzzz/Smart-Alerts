//-----------------------------------------------------------------------
// <copyright file="AlertsPublisherTest.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartDetectorSchedulerTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.AzureStorage;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Scheduler.Exceptions;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Scheduler.Publisher;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Moq;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Contracts.Alert;

    [TestClass]
    public class AlertsPublisherTest
    {
        private Mock<ITracer> tracerMock;
        private Mock<ICloudBlobContainerWrapper> containerMock;

        private AlertsPublisher publisher;

        [TestInitialize]
        public void Setup()
        {
            this.tracerMock = new Mock<ITracer>();
            this.containerMock = new Mock<ICloudBlobContainerWrapper>();

            var storageProviderFactoryMock = new Mock<ICloudStorageProviderFactory>();
            storageProviderFactoryMock.Setup(m => m.GetAlertsStorageContainer()).Returns(this.containerMock.Object);

            this.publisher = new AlertsPublisher(this.tracerMock.Object, storageProviderFactoryMock.Object);
        }

        [TestMethod]
        public async Task WhenNoAlertsToPublishThenResultStoreIsNotCalled()
        {
            await this.publisher.PublishAlertsAsync("smartDetectorId", new List<ContractsAlert>());

            this.containerMock.Verify(m => m.UploadBlobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            this.tracerMock.Verify(m => m.TrackEvent(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<IDictionary<string, double>>()), Times.Never);
        }

        [TestMethod]
        [ExpectedException(typeof(AlertsPublishException))]
        public async Task WhenStorageExceptionIsThrownThenPublisherExceptionIsThrown()
        {
            // Setup mock to throw storage exception when correct blob name is specified
            var smartDetectorId = "smartDetectorId";
            var todayString = DateTime.UtcNow.ToString("yyyy-MM-dd");
            var blobName = $"{smartDetectorId}/{todayString}/id1";
            this.containerMock.Setup(m => m.UploadBlobAsync(blobName, It.IsAny<string>(), It.IsAny<CancellationToken>())).Throws(new StorageException());

            var alerts = new List<ContractsAlert>
            {
                this.CreateContractsAlert("id1", "title1", "resource1", smartDetectorId, 10),
                this.CreateContractsAlert("id2", "title2", "resource2", smartDetectorId, 10)
            };
            await this.publisher.PublishAlertsAsync("smartDetectorId", alerts);
        }

        [TestMethod]
        public async Task WhenPublishingAlertsThenResultStoreIsCalledAccordingly()
        {
            // Setup mock to return blob with URI when correct blob name is specified
            var smartDetectorId = "smartDetectorId";
            var todayString = DateTime.UtcNow.ToString("yyyy-MM-dd");
            var blobName1 = $"{smartDetectorId}/{todayString}/id1";
            var blobName2 = $"{smartDetectorId}/{todayString}/id2";

            var blobMock1 = new Mock<ICloudBlob>();
            var blobUri1 = new Uri($"https://storage.blob.core.windows.net/result/{blobName1}");
            blobMock1.Setup(m => m.Uri).Returns(blobUri1);

            var blobMock2 = new Mock<ICloudBlob>();
            var blobUri2 = new Uri($"https://storage.blob.core.windows.net/result/{blobName2}");
            blobMock2.Setup(m => m.Uri).Returns(blobUri2);

            this.containerMock.Setup(m => m.UploadBlobAsync(blobName1, It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(blobMock1.Object);
            this.containerMock.Setup(m => m.UploadBlobAsync(blobName2, It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(blobMock2.Object);

            var alerts = new List<ContractsAlert>
            {
                this.CreateContractsAlert("id1", "title1", "resource1", smartDetectorId, 10),
                this.CreateContractsAlert("id2", "title2", "resource2", smartDetectorId, 10)
            };
            await this.publisher.PublishAlertsAsync(smartDetectorId, alerts);

            this.tracerMock.Verify(m => m.TrackEvent("Alerts", It.Is<IDictionary<string, string>>(properties => properties["SmartDetectorId"] == smartDetectorId && properties["AlertBlobUri"] == blobUri1.AbsoluteUri), It.IsAny<IDictionary<string, double>>()), Times.Once);
            this.tracerMock.Verify(m => m.TrackEvent("Alerts", It.Is<IDictionary<string, string>>(properties => properties["SmartDetectorId"] == smartDetectorId && properties["AlertBlobUri"] == blobUri2.AbsoluteUri), It.IsAny<IDictionary<string, double>>()), Times.Once);
        }

        private ContractsAlert CreateContractsAlert(string id, string title, string resourceId, string detectorId, int analysisWindowSizeInMinutes)
        {
            return new ContractsAlert
            {
                Id = id,
                Title = title,
                ResourceId = resourceId,
                SmartDetectorId = detectorId,
                SmartDetectorName = detectorId,
                AnalysisTimestamp = DateTime.UtcNow,
                AnalysisWindowSizeInMinutes = analysisWindowSizeInMinutes
            };
        }
    }
}
