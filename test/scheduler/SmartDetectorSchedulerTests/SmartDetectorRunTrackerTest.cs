//-----------------------------------------------------------------------
// <copyright file="SmartDetectorRunTrackerTest.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartDetectorSchedulerTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.AlertRules;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.AzureStorage;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Scheduler;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Scheduler.SmartDetectorRunTracker;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Storage.Table;
    using Moq;

    [TestClass]
    public class SmartDetectorRunTrackerTest
    {
        private SmartDetectorRunsTracker smartDetectorRunsTracker;
        private Mock<ICloudTableWrapper> tableMock;

        [TestInitialize]
        public void Setup()
        {
            this.tableMock = new Mock<ICloudTableWrapper>();
            var tableClientMock = new Mock<ICloudTableClientWrapper>();
            tableClientMock.Setup(m => m.GetTableReference(It.IsAny<string>())).Returns(this.tableMock.Object);
            var storageProviderFactoryMock = new Mock<ICloudStorageProviderFactory>();
            storageProviderFactoryMock.Setup(m => m.GetSmartDetectorStorageTableClient()).Returns(tableClientMock.Object);

            var tracerMock = new Mock<ITracer>();
            this.smartDetectorRunsTracker = new SmartDetectorRunsTracker(storageProviderFactoryMock.Object, tracerMock.Object);
        }

        [TestMethod]
        public async Task WhenUpdatingSmartDetectorRunThenUpdateIsCalledCorrectly()
        {
            var smartDetectorxecution = new SmartDetectorExecutionInfo
            {
                AlertRule = new AlertRule
                {
                    Id = "some_rule",
                    SmartDetectorId = "some_smart_detector",
                },
                LastExecutionTime = DateTime.UtcNow.AddHours(-1),
                CurrentExecutionTime = DateTime.UtcNow.AddMinutes(-1)
            };
            await this.smartDetectorRunsTracker.UpdateSmartDetectorRunAsync(smartDetectorxecution);
            this.tableMock.Verify(m => m.ExecuteAsync(
                It.Is<TableOperation>(operation =>
                    operation.OperationType == TableOperationType.InsertOrReplace &&
                    operation.Entity.RowKey.Equals(smartDetectorxecution.AlertRule.Id) &&
                    ((TrackSmartDetectorRunEntity)operation.Entity).SmartDetectorId.Equals(smartDetectorxecution.AlertRule.SmartDetectorId) &&
                    ((TrackSmartDetectorRunEntity)operation.Entity).LastSuccessfulExecutionTime.Equals(smartDetectorxecution.CurrentExecutionTime)),
                It.IsAny<CancellationToken>()));
        }

        [TestMethod]
        public async Task WhenGettingSmartDetectorsToRunWithRulesThenOnlyValidSmartDetectorsAreReturned()
        {
            var rules = new List<AlertRule>
            {
                new AlertRule
                {
                    Id = "should_not_run_rule",
                    SmartDetectorId = "should_not_run_smart_detector",
                    Cadence = TimeSpan.FromMinutes(1440) // once a day
                },
                new AlertRule
                {
                    Id = "should_run_rule",
                    SmartDetectorId = "should_run_smart_detector",
                    Cadence = TimeSpan.FromMinutes(60) // once every hour
                },
                new AlertRule
                {
                    Id = "should_run_rule2",
                    SmartDetectorId = "should_run_smart_detector2",
                    Cadence = TimeSpan.FromMinutes(1440) // once a day
                }
            };

            // create a table tracking result where 1 Smart Detector never ran, 1 Smart Detector that ran today and 1 Smart Detector that ran 2 hours ago
            var now = DateTime.UtcNow;
            var tableResult = new List<TrackSmartDetectorRunEntity>
            {
                new TrackSmartDetectorRunEntity
                {
                    RowKey = "should_not_run_rule",
                    SmartDetectorId = "should_not_run_smart_detector",
                    LastSuccessfulExecutionTime = new DateTime(now.Year, now.Month, now.Day, 0, 5, 0)
                },
                new TrackSmartDetectorRunEntity
                {
                    RowKey = "should_run_rule",
                    SmartDetectorId = "should_run_smart_detector",
                    LastSuccessfulExecutionTime = now.AddHours(-2)
                }
            };
            
            this.tableMock.Setup(m => m.ReadPartitionAsync<TrackSmartDetectorRunEntity>("tracking")).ReturnsAsync(tableResult);

            var smartDetectorsToRun = await this.smartDetectorRunsTracker.GetSmartDetectorsToRunAsync(rules);
            Assert.AreEqual(2, smartDetectorsToRun.Count);
            Assert.AreEqual("should_run_smart_detector", smartDetectorsToRun.First().AlertRule.SmartDetectorId);
            Assert.AreEqual("should_run_smart_detector2", smartDetectorsToRun.Last().AlertRule.SmartDetectorId);
        }
    }
}
