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
    using Microsoft.Azure.Monitoring.SmartSignals.RuntimeShared.AlertRules;
    using Microsoft.Azure.Monitoring.SmartSignals.RuntimeShared.AzureStorage;
    using Microsoft.Azure.Monitoring.SmartSignals.Scheduler;
    using Microsoft.Azure.Monitoring.SmartSignals.Scheduler.SignalRunTracker;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Storage.Table;
    using Moq;

    [TestClass]
    public class SignalRunTrackerTest
    {
        private SignalRunsTracker signalRunsTracker;
        private Mock<ICloudTableWrapper> tableMock;

        [TestInitialize]
        public void Setup()
        {
            this.tableMock = new Mock<ICloudTableWrapper>();
            var tableClientMock = new Mock<ICloudTableClientWrapper>();
            tableClientMock.Setup(m => m.GetTableReference(It.IsAny<string>())).Returns(this.tableMock.Object);
            var storageProviderFactoryMock = new Mock<ICloudStorageProviderFactory>();
            storageProviderFactoryMock.Setup(m => m.GetSmartSignalStorageTableClient()).Returns(tableClientMock.Object);

            var tracerMock = new Mock<ITracer>();
            this.signalRunsTracker = new SignalRunsTracker(storageProviderFactoryMock.Object, tracerMock.Object);
        }

        [TestMethod]
        public async Task WhenUpdatingSignalRunThenUpdateIsCalledCorrectly()
        {
            var signalExecution = new SignalExecutionInfo
            {
                AlertRule = new AlertRule
                {
                    Id = "some_rule",
                    SignalId = "some_signal",
                },
                LastExecutionTime = DateTime.UtcNow.AddHours(-1),
                CurrentExecutionTime = DateTime.UtcNow.AddMinutes(-1)
            };
            await this.signalRunsTracker.UpdateSignalRunAsync(signalExecution);
            this.tableMock.Verify(m => m.ExecuteAsync(
                It.Is<TableOperation>(operation =>
                    operation.OperationType == TableOperationType.InsertOrReplace &&
                    operation.Entity.RowKey.Equals(signalExecution.AlertRule.Id) &&
                    ((TrackSignalRunEntity)operation.Entity).SignalId.Equals(signalExecution.AlertRule.SignalId) &&
                    ((TrackSignalRunEntity)operation.Entity).LastSuccessfulExecutionTime.Equals(signalExecution.CurrentExecutionTime)),
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
                    SignalId = "should_not_run_signal",
                    Cadence = TimeSpan.FromMinutes(1440) // once a day
                },
                new AlertRule
                {
                    Id = "should_run_rule",
                    SignalId = "should_run_signal",
                    Cadence = TimeSpan.FromMinutes(60) // once every hour
                },
                new AlertRule
                {
                    Id = "should_run_rule2",
                    SignalId = "should_run_signal2",
                    Cadence = TimeSpan.FromMinutes(1440) // once a day
                }
            };

            // create a table tracking result where 1 signal never ran, 1 signal that ran today and 1 signal that ran 2 hours ago
            var now = DateTime.UtcNow;
            var tableResult = new List<TrackSignalRunEntity>
            {
                new TrackSignalRunEntity
                {
                    RowKey = "should_not_run_rule",
                    SignalId = "should_not_run_signal",
                    LastSuccessfulExecutionTime = new DateTime(now.Year, now.Month, now.Day, 0, 5, 0)
                },
                new TrackSignalRunEntity
                {
                    RowKey = "should_run_rule",
                    SignalId = "should_run_signal",
                    LastSuccessfulExecutionTime = now.AddHours(-2)
                }
            };
            
            this.tableMock.Setup(m => m.ReadPartitionAsync<TrackSignalRunEntity>("tracking")).ReturnsAsync(tableResult);

            var signalsToRun = await this.signalRunsTracker.GetSignalsToRunAsync(rules);
            Assert.AreEqual(2, signalsToRun.Count);
            Assert.AreEqual("should_run_signal", signalsToRun.First().AlertRule.SignalId);
            Assert.AreEqual("should_run_signal2", signalsToRun.Last().AlertRule.SignalId);
        }
    }
}
