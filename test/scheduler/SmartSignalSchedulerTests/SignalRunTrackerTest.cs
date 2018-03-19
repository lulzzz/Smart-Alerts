//-----------------------------------------------------------------------
// <copyright file="SignalRunTrackerTest.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartSignalSchedulerTests
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
    public class SignalRunTrackerTest
    {
        private SmartDetectorRunsTracker signalRunsTracker;
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
            this.signalRunsTracker = new SmartDetectorRunsTracker(storageProviderFactoryMock.Object, tracerMock.Object);
        }

        [TestMethod]
        public async Task WhenUpdatingSignalRunThenUpdateIsCalledCorrectly()
        {
            var signalExecution = new SmartDetectorExecutionInfo
            {
                AlertRule = new AlertRule
                {
                    Id = "some_rule",
                    SmartDetectorId = "some_smart_detector",
                },
                LastExecutionTime = DateTime.UtcNow.AddHours(-1),
                CurrentExecutionTime = DateTime.UtcNow.AddMinutes(-1)
            };
            await this.signalRunsTracker.UpdateSmartDetectorRunAsync(signalExecution);
            this.tableMock.Verify(m => m.ExecuteAsync(
                It.Is<TableOperation>(operation =>
                    operation.OperationType == TableOperationType.InsertOrReplace &&
                    operation.Entity.RowKey.Equals(signalExecution.AlertRule.Id) &&
                    ((TrackSmartDetectorRunEntity)operation.Entity).SmartDetectorId.Equals(signalExecution.AlertRule.SmartDetectorId) &&
                    ((TrackSmartDetectorRunEntity)operation.Entity).LastSuccessfulExecutionTime.Equals(signalExecution.CurrentExecutionTime)),
                It.IsAny<CancellationToken>()));
        }

        [TestMethod]
        public async Task WhenGettingSignalsToRunWithRulesThenOnlyValidSignalsAreReturned()
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

            // create a table tracking result where 1 signal never ran, 1 signal that ran today and 1 signal that ran 2 hours ago
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

            var signalsToRun = await this.signalRunsTracker.GetSmartDetectorsToRunAsync(rules);
            Assert.AreEqual(2, signalsToRun.Count);
            Assert.AreEqual("should_run_smart_detector", signalsToRun.First().AlertRule.SmartDetectorId);
            Assert.AreEqual("should_run_smart_detector2", signalsToRun.Last().AlertRule.SmartDetectorId);
        }
    }
}
