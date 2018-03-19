//-----------------------------------------------------------------------
// <copyright file="ScheduleFlowTest.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartSignalSchedulerTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.AlertRules;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Scheduler;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Scheduler.Publisher;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Scheduler.SmartDetectorRunTracker;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Contracts.Alert;

    [TestClass]
    public class ScheduleFlowTest
    {
        private Mock<IAlertRuleStore> alertRuleStoreMock;
        private Mock<ISmartDetectorRunsTracker> smartDetectorRunTrackerMock;
        private Mock<IAnalysisExecuter> analysisExecuterMock;
        private Mock<IAlertsPublisher> publisherMock;

        private ScheduleFlow scheduleFlow;

        [TestInitialize]
        public void Setup()
        {
            var tracerMock = new Mock<ITracer>();
            this.alertRuleStoreMock = new Mock<IAlertRuleStore>();
            this.smartDetectorRunTrackerMock = new Mock<ISmartDetectorRunsTracker>();
            this.analysisExecuterMock = new Mock<IAnalysisExecuter>();
            this.publisherMock = new Mock<IAlertsPublisher>();

            this.scheduleFlow = new ScheduleFlow(
                tracerMock.Object,
                this.alertRuleStoreMock.Object,
                this.smartDetectorRunTrackerMock.Object,
                this.analysisExecuterMock.Object,
                this.publisherMock.Object);
        }

        [TestMethod]
        public async Task WhenSignalExecutionThrowsExceptionThenNextSignalIsProcessed()
        {
            // Create Smart Detector execution information to be returned from the job tracker
            var smartDetectorExecution1 = new SmartDetectorExecutionInfo
            {
                AlertRule = new AlertRule
                {
                    SmartDetectorId = "s1",
                    Id = "r1",
                    ResourceId = "1",
                },
                LastExecutionTime = DateTime.UtcNow.AddHours(-1)
            };
            var smartDetectorExecution2 = new SmartDetectorExecutionInfo
            {
                AlertRule = new AlertRule
                {
                    SmartDetectorId = "s2",
                    Id = "r2",
                    ResourceId = "2",
                },
                LastExecutionTime = DateTime.UtcNow.AddHours(-1)
            };
            var smartDetectorExecutions = new List<SmartDetectorExecutionInfo> { smartDetectorExecution1, smartDetectorExecution2 };

            this.smartDetectorRunTrackerMock.Setup(m => m.GetSmartDetectorsToRunAsync(It.IsAny<IList<AlertRule>>())).ReturnsAsync(smartDetectorExecutions);

            // first Smart Detector execution throws exception and the second one returns a result
            const string ResultItemTitle = "someTitle";
            this.analysisExecuterMock.SetupSequence(m => m.ExecuteSmartDetectorAsync(It.IsAny<SmartDetectorExecutionInfo>(), It.Is<IList<string>>(lst => lst.First() == "1" || lst.First() == "2")))
                .Throws(new Exception())
                .ReturnsAsync(new List<ContractsAlert> { new TestResultItem(ResultItemTitle) });

            await this.scheduleFlow.RunAsync();

            this.alertRuleStoreMock.Verify(m => m.GetAllAlertRulesAsync(), Times.Once);

            // Verify that these were called only once since the first Smart Detector execution throwed exception
            this.publisherMock.Verify(m => m.PublishAlertsAsync("s2", It.Is<IList<ContractsAlert>>(items => items.Count == 1 && items.First().Title == ResultItemTitle)), Times.Once);
            this.smartDetectorRunTrackerMock.Verify(m => m.UpdateSmartDetectorRunAsync(It.IsAny<SmartDetectorExecutionInfo>()), Times.Once());
            this.smartDetectorRunTrackerMock.Verify(m => m.UpdateSmartDetectorRunAsync(smartDetectorExecution2));
        }

        [TestMethod]
        public async Task WhenThereAreSignalsToRunThenAllSiganlResultItemsArePublished()
        {
            // Create Smart Detector execution information to be returned from the job tracker
            var smartDetectorExecution1 = new SmartDetectorExecutionInfo
            {
                AlertRule = new AlertRule
                {
                    Id = "r1",
                    SmartDetectorId = "s1",
                    ResourceId = "1",
                },
                LastExecutionTime = DateTime.UtcNow.AddHours(-1)
            };
            var smartDetectorExecution2 = new SmartDetectorExecutionInfo
            {
                AlertRule = new AlertRule
                {
                    Id = "r2",
                    SmartDetectorId = "s2",
                    ResourceId = "2",
                },
                LastExecutionTime = DateTime.UtcNow.AddHours(-1)
            };
            var smartDetectorExecutions = new List<SmartDetectorExecutionInfo> { smartDetectorExecution1, smartDetectorExecution2 };

            this.smartDetectorRunTrackerMock.Setup(m => m.GetSmartDetectorsToRunAsync(It.IsAny<IList<AlertRule>>())).ReturnsAsync(smartDetectorExecutions);

            // each Smart Detector execution returns a result
            this.analysisExecuterMock.Setup(m => m.ExecuteSmartDetectorAsync(It.IsAny<SmartDetectorExecutionInfo>(), It.Is<IList<string>>(lst => lst.First() == "1" || lst.First() == "2")))
                .ReturnsAsync(new List<ContractsAlert> { new TestResultItem("title") });

            await this.scheduleFlow.RunAsync();

            // Verify Alerts were published and Smart Detector tracker was updated for each Smart Detector execution
            this.alertRuleStoreMock.Verify(m => m.GetAllAlertRulesAsync(), Times.Once);
            this.publisherMock.Verify(m => m.PublishAlertsAsync(It.IsAny<string>(), It.IsAny<IList<ContractsAlert>>()), Times.Exactly(2));
            this.smartDetectorRunTrackerMock.Verify(m => m.UpdateSmartDetectorRunAsync(It.IsAny<SmartDetectorExecutionInfo>()), Times.Exactly(2));
        }

        private class TestResultItem : ContractsAlert
        {
            public TestResultItem(string title)
            {
                this.Title = title;
                this.AnalysisTimestamp = DateTime.UtcNow;
            }
        }
    }
}
