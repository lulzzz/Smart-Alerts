//-----------------------------------------------------------------------
// <copyright file="SignalResultPublisherTest.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartSignalSchedulerTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.AzureStorage;
    using Microsoft.Azure.Monitoring.SmartSignals.Scheduler.Exceptions;
    using Microsoft.Azure.Monitoring.SmartSignals.Scheduler.Publisher;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Moq;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Contracts.Alert;

    [TestClass]
    public class SignalResultPublisherTest
    {
        private Mock<ITracer> tracerMock;
        private Mock<ICloudBlobContainerWrapper> containerMock;

        private SmartSignalResultPublisher publisher;

        [TestInitialize]
        public void Setup()
        {
            this.tracerMock = new Mock<ITracer>();
            this.containerMock = new Mock<ICloudBlobContainerWrapper>();

            var storageProviderFactoryMock = new Mock<ICloudStorageProviderFactory>();
            storageProviderFactoryMock.Setup(m => m.GetAlertsStorageContainer()).Returns(this.containerMock.Object);

            this.publisher = new SmartSignalResultPublisher(this.tracerMock.Object, storageProviderFactoryMock.Object);
        }

        [TestMethod]
        public async Task WhenNoResultsToPublishThenResultStoreIsNotCalled()
        {
            await this.publisher.PublishSignalResultItemsAsync("signalId", new List<ContractsAlert>());

            this.containerMock.Verify(m => m.UploadBlobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            this.tracerMock.Verify(m => m.TrackEvent(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<IDictionary<string, double>>()), Times.Never);
        }

        [TestMethod]
        [ExpectedException(typeof(SignalResultPublishException))]
        public async Task WhenStorageExceptionIsThrownThenPublisherExceptionIsThrown()
        {
            // Setup mock to throw storage exception when correct blob name is specified
            var signalId = "signalId";
            var todayString = DateTime.UtcNow.ToString("yyyy-MM-dd");
            var blobName = $"{signalId}/{todayString}/id1";
            this.containerMock.Setup(m => m.UploadBlobAsync(blobName, It.IsAny<string>(), It.IsAny<CancellationToken>())).Throws(new StorageException());

            var alerts = new List<ContractsAlert>
            {
                this.CreateContractsAlert("id1", "title1", "resource1", signalId, 10),
                this.CreateContractsAlert("id2", "title2", "resource2", signalId, 10)
            };
            await this.publisher.PublishSignalResultItemsAsync("signalId", alerts);
        }

        [TestMethod]
        public async Task WhenPublishingResultsThenResultStoreIsCalledAccordingly()
        {
            // Setup mock to return blob with URI when correct blob name is specified
            var signalId = "signalId";
            var todayString = DateTime.UtcNow.ToString("yyyy-MM-dd");
            var blobName1 = $"{signalId}/{todayString}/id1";
            var blobName2 = $"{signalId}/{todayString}/id2";

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
                this.CreateContractsAlert("id1", "title1", "resource1", signalId, 10),
                this.CreateContractsAlert("id2", "title2", "resource2", signalId, 10)
            };
            await this.publisher.PublishSignalResultItemsAsync(signalId, alerts);

            this.tracerMock.Verify(m => m.TrackEvent("SmartSignalResult", It.Is<IDictionary<string, string>>(properties => properties["SignalId"] == signalId && properties["ResultItemBlobUri"] == blobUri1.AbsoluteUri), It.IsAny<IDictionary<string, double>>()), Times.Once);
            this.tracerMock.Verify(m => m.TrackEvent("SmartSignalResult", It.Is<IDictionary<string, string>>(properties => properties["SignalId"] == signalId && properties["ResultItemBlobUri"] == blobUri2.AbsoluteUri), It.IsAny<IDictionary<string, double>>()), Times.Once);
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
