//-----------------------------------------------------------------------
// <copyright file="SignalResultApiTests.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ManagementApiTests.EndpointsLogic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.AzureStorage;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Contracts;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi.AIClient;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi.EndpointsLogic;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi.Responses;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Storage;
    using Moq;
    using Newtonsoft.Json;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Contracts.Alert;

    [TestClass]
    public class SignalResultApiTests
    {
        private readonly DateTime startTime = new DateTime(2018, 1, 1);
        private readonly DateTime endTime = new DateTime(2018, 1, 2);

        private Mock<IApplicationInsightsClient> applicationInsightClientMock;
        private Mock<IApplicationInsightsClientFactory> applicationInsightsFactoryMock;
        private Mock<ICloudStorageProviderFactory> storageProviderFactory;
        private Mock<ICloudBlobContainerWrapper> signalResultContainerMock;

        private IAlertsApi signalResultApi;

        [TestInitialize]
        public void Initialize()
        {
            this.applicationInsightClientMock = new Mock<IApplicationInsightsClient>();
            this.applicationInsightsFactoryMock = new Mock<IApplicationInsightsClientFactory>();
            this.storageProviderFactory = new Mock<ICloudStorageProviderFactory>();
            this.signalResultContainerMock = new Mock<ICloudBlobContainerWrapper>();

            this.applicationInsightsFactoryMock.Setup(factory => factory.GetApplicationInsightsClient()).Returns(this.applicationInsightClientMock.Object);
            this.storageProviderFactory.Setup(factory => factory.GetAlertsStorageContainer()).Returns(this.signalResultContainerMock.Object);

            this.signalResultApi = new AlertsApi(this.storageProviderFactory.Object, this.applicationInsightsFactoryMock.Object);
        }

        [TestMethod]
        public async Task WhenGettingAllSignalsResultsHappyFlow()
        {
            this.applicationInsightClientMock.Setup(ai => ai.GetCustomEventsAsync(It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
                                             .ReturnsAsync(this.GetApplicationInsightsEvents());
            this.signalResultContainerMock.Setup(src => src.DownloadBlobContentAsync(It.IsAny<string>())).ReturnsAsync(this.GetSmartSignalResultItemPresentation());

            ListAlertsResponse response = await this.signalResultApi.GetAllAlertsAsync(this.startTime, this.endTime, CancellationToken.None);

            this.signalResultContainerMock.Verify(src => src.DownloadBlobContentAsync(It.IsAny<string>()), Times.Once);
            Assert.AreEqual(1, response.Alerts.Count);
            Assert.AreEqual("someId", response.Alerts.First().Id);
            Assert.AreEqual("someTitle", response.Alerts.First().Title);
        }

        [TestMethod]
        public async Task WhenGettingAllSignalsResultsButFailedToQueryApplicationInsightsThenThrowException()
        {
            this.applicationInsightClientMock.Setup(ai => ai.GetCustomEventsAsync(It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
                                             .ThrowsAsync(new ApplicationInsightsClientException("some message"));

            try
            {
                await this.signalResultApi.GetAllAlertsAsync(this.startTime, this.endTime, CancellationToken.None);
            }
            catch (SmartDetectorsManagementApiException)
            {
                return;
            }

            Assert.Fail("A management exception should have been thrown in case failing to query Application Insights");
        }

        [TestMethod]
        public async Task WhenGettingAllSignalsResultsButFailedToDeserializeResultsThenThrowException()
        {
            this.applicationInsightClientMock.Setup(ai => ai.GetCustomEventsAsync(It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(this.GetApplicationInsightsEvents());
            this.signalResultContainerMock.Setup(src => src.DownloadBlobContentAsync(It.IsAny<string>())).ReturnsAsync("someCorruptedData");

            try
            {
                await this.signalResultApi.GetAllAlertsAsync(this.startTime, this.endTime, CancellationToken.None);
            }
            catch (SmartDetectorsManagementApiException)
            {
                return;
            }

            Assert.Fail("A management exception should have been thrown in case failing to de-serialize signals results");
        }

        [TestMethod]
        public async Task WhenFailedToGetResultItemsFromStorageThenThrowException()
        {
            this.applicationInsightClientMock.Setup(ai => ai.GetCustomEventsAsync(It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(this.GetApplicationInsightsEvents());
            this.signalResultContainerMock.Setup(src => src.DownloadBlobContentAsync(It.IsAny<string>())).ThrowsAsync(new StorageException());

            try
            {
                await this.signalResultApi.GetAllAlertsAsync(this.startTime, this.endTime, CancellationToken.None);
            }
            catch (SmartDetectorsManagementApiException)
            {
                return;
            }

            Assert.Fail("A management exception should have been thrown in case failing to de-serialize signals results");
        }

        private List<ApplicationInsightsEvent> GetApplicationInsightsEvents()
        {
            return new List<ApplicationInsightsEvent>()
            {
                new ApplicationInsightsEvent()
                {
                    Id = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    CustomDimensions = new Dictionary<string, string>()
                    {
                        {
                            "AlertBlobUri", "https://storagename.blob.core.windows.net/signalresult/signalName/2018-01-24/037635a40da413daafe528b7399eb5574581091d704097dd67bc7c63ddde8882"
                        }
                    }
                }
            };
        }

        private string GetSmartSignalResultItemPresentation()
        {
            var signalResult = new ContractsAlert
            {
                Id = "someId",
                Title = "someTitle",
                ResourceId = "/subscriptions/b4b7d4c1-8c25-4da3-bf1c-e50f647a8130/resourceGroups/asafst/providers/Microsoft.Insights/components/deepinsightsdailyreports",
                CorrelationHash = "93e9a62b1e1a0dca5d9d63cc7e9aae71edb9988aa6f1dfc3b85e71b0f57d2819",
                SmartDetectorId = "SampleSignal",
                SmartDetectorName = "SampleSignal",
                AnalysisTimestamp = DateTime.UtcNow,
                AnalysisWindowSizeInMinutes = 5,
                Properties = new List<AlertProperty>(),
                RawProperties = new Dictionary<string, string>()
            };

            return JsonConvert.SerializeObject(signalResult);
        }
    }
}
