//-----------------------------------------------------------------------
// <copyright file="AlertPresentationTests.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartDetectorsAnalysisTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Contracts;
    using Microsoft.Azure.Monitoring.SmartDetectors.Presentation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Alert = Microsoft.Azure.Monitoring.SmartDetectors.Alert;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Contracts.Alert;
    using ResourceType = Microsoft.Azure.Monitoring.SmartDetectors.ResourceType;

    [TestClass]
    public class AlertPresentationTests
    {
        private const string SmartDetectorName = "smartDetectorName";

        [TestInitialize]
        public void TestInitialize()
        {
        }

        [TestMethod]
        public void WhenProcessingAlertThenThePresentationIsCreatedCorrectly()
        {
            var presentation = this.CreatePresentation(new TestAlert());
            Assert.IsTrue(presentation.AnalysisTimestamp <= DateTime.UtcNow, "Unexpected analysis timestamp in the future");
            Assert.IsTrue(presentation.AnalysisTimestamp >= DateTime.UtcNow.AddMinutes(-1), "Unexpected analysis timestamp - too back in the past");
            Assert.AreEqual(24 * 60, presentation.AnalysisWindowSizeInMinutes, "Unexpected analysis window size");
            Assert.AreEqual(SmartDetectorName, presentation.SmartDetectorName, "Unexpected Smart Detector name");
            Assert.AreEqual("Test title", presentation.Title, "Unexpected title");
            Assert.AreEqual(8, presentation.Properties.Count, "Unexpected number of properties");
            this.VerifyProperty(presentation.Properties, "Machine name", AlertPresentationSection.Property, "strongOne", "The machine on which the CPU had increased");
            this.VerifyProperty(presentation.Properties, "CPU over the last 7 days", AlertPresentationSection.Chart, "<the query>", "CPU chart for machine strongOne, showing increase of 22.4");
            this.VerifyProperty(presentation.Properties, "CPU increased", AlertPresentationSection.Property, "22.4", "CPU increase on machine strongOne");
            this.VerifyProperty(presentation.Properties, "Another query 1", AlertPresentationSection.AdditionalQuery, "<query1>", "Info balloon for another query 1");
            this.VerifyProperty(presentation.Properties, "Another query 2", AlertPresentationSection.AdditionalQuery, "<query2>", "Info balloon for another query 2");
            this.VerifyProperty(presentation.Properties, "Analysis 1", AlertPresentationSection.Analysis, "analysis1", "Info balloon for analysis 1");
            this.VerifyProperty(presentation.Properties, "Analysis 2", AlertPresentationSection.Analysis, "analysis2", "Info balloon for analysis 2");
            this.VerifyProperty(presentation.Properties, "Analysis 3", AlertPresentationSection.Analysis, (new DateTime(2012, 11, 12, 17, 22, 37)).ToString("u"), "Info balloon for analysis 3");
            Assert.AreEqual("no show", presentation.RawProperties["NoPresentation"]);
            Assert.AreEqual(TelemetryDbType.LogAnalytics, presentation.QueryRunInfo.Type, "Unexpected telemetry DB type");
            CollectionAssert.AreEqual(new[] { "resourceId1", "resourceId2" }, presentation.QueryRunInfo.ResourceIds.ToArray(), "Unexpected resource IDs");
        }

        [TestMethod]
        public void WhenProcessingAlertWithoutSummaryChartThenNoExceptionIsThrown()
        {
            this.CreatePresentation(new TestAlertNoSummaryChart());
        }

        [TestMethod]
        public void WhenAlertsHaveDifferentPredicatesThenTheCorrelationHashIsDifferent()
        {
            var alert1 = new TestAlert();
            var alert2 = new TestAlert();
            alert2.NoPresentation += "X";

            var presentation1 = this.CreatePresentation(alert1);

            // A non predicate property is different - correlation hash should be the same
            var presentation2 = this.CreatePresentation(alert2);
            Assert.AreNotEqual(presentation1.Id, presentation2.Id);
            Assert.AreEqual(presentation1.CorrelationHash, presentation2.CorrelationHash);

            // A predicate property is different - correlation hash should be the different
            alert2.OnlyPredicate += "X";
            presentation2 = this.CreatePresentation(alert2);
            Assert.AreNotEqual(presentation1.Id, presentation2.Id);
            Assert.AreNotEqual(presentation1.CorrelationHash, presentation2.CorrelationHash);
        }

        [TestMethod]
        public void WhenProcessingAlertWithoutQueriesAndNullRunInfoThenThePresentationIsCreatedSuccessfully()
        {
            this.CreatePresentation(new TestAlertNoQueries(), nullQueryRunInfo: true);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidAlertPresentationException))]
        public void WhenProcessingAlertWithQueriesAndNullRunInfoThenAnExceptionIsThrown()
        {
            this.CreatePresentation(new TestAlert(), nullQueryRunInfo: true);
        }

        private ContractsAlert CreatePresentation(Alert alert, bool nullQueryRunInfo = false)
        {
            QueryRunInfo queryRunInfo = null;
            if (!nullQueryRunInfo)
            {
                queryRunInfo = new QueryRunInfo
                {
                    Type = alert.ResourceIdentifier.ResourceType == ResourceType.ApplicationInsights ? TelemetryDbType.ApplicationInsights : TelemetryDbType.LogAnalytics,
                    ResourceIds = new List<string>() { "resourceId1", "resourceId2" }
                };
            }

            string resourceId = "resourceId";
            var request = new SmartDetectorExecutionRequest
            {
                ResourceIds = new List<string>() { resourceId },
                SmartDetectorId = "smartDetectorId",
                DataEndTime = DateTime.UtcNow.AddMinutes(-20),
                Cadence = TimeSpan.FromDays(1),
            };
                
            return alert.CreateContractsAlert(request, SmartDetectorName, queryRunInfo);
        }

        private void VerifyProperty(List<AlertProperty> properties, string name, AlertPresentationSection displayCategory, string value, string infoBalloon)
        {
            var property = properties.SingleOrDefault(p => p.Name == name);
            Assert.IsNotNull(property, $"Property {name} not found");
            Assert.AreEqual(displayCategory.ToString(), property.DisplayCategory.ToString());
            Assert.AreEqual(value, property.Value);
            Assert.AreEqual(infoBalloon, property.InfoBalloon);
        }

        private class TestAlertNoSummary : Alert
        {
            public TestAlertNoSummary()
                : base("Test title", default(ResourceIdentifier))
            {
            }

            [AlertPredicateProperty]
            [AlertPresentationProperty(AlertPresentationSection.Property, "CPU increased", InfoBalloon = "CPU increase on machine {MachineName}")]
            public double Value => 22.4;

            [AlertPredicateProperty]
            [AlertPresentationProperty(AlertPresentationSection.Property, "Machine name", InfoBalloon = "The machine on which the CPU had increased")]
            public string MachineName => "strongOne";
        }

        private class TestAlertNoQueries : TestAlertNoSummary
        {
            [AlertPredicateProperty]
            [AlertPresentationProperty(AlertPresentationSection.Property, "CPU increased", InfoBalloon = "CPU increase on machine {MachineName}")]
            public new double Value => 22.4;
        }

        private class TestAlertNoSummaryProperty : TestAlertNoSummary
        {
            [AlertPresentationProperty(AlertPresentationSection.Chart, "CPU over the last 7 days", InfoBalloon = "CPU chart for machine {MachineName}, showing increase of {Value}")]
            public string CpuChartQuery => "<the query>";
        }

        private class TestAlert : TestAlertNoSummaryProperty
        {
            [AlertPredicateProperty]
            [AlertPresentationProperty(AlertPresentationSection.Property, "CPU increased", InfoBalloon = "CPU increase on machine {MachineName}")]
            public new double Value => 22.4;

            [AlertPresentationProperty(AlertPresentationSection.AdditionalQuery, "Another query 1", InfoBalloon = "Info balloon for another query 1")]
            public string Query1 => "<query1>";

            [AlertPresentationProperty(AlertPresentationSection.AdditionalQuery, "Another query 2", InfoBalloon = "Info balloon for another query 2")]
            public string Query2 => "<query2>";

            [AlertPresentationProperty(AlertPresentationSection.Analysis, "Analysis 1", InfoBalloon = "Info balloon for analysis 1")]
            public string Analysis1 => "analysis1";

            [AlertPresentationProperty(AlertPresentationSection.Analysis, "Analysis 2", InfoBalloon = "Info balloon for analysis 2")]
            public string Analysis2 => "analysis2";

            [AlertPresentationProperty(AlertPresentationSection.Analysis, "Analysis 3", InfoBalloon = "Info balloon for analysis 3")]
            public DateTime Analysis3 => new DateTime(2012, 11, 12, 17, 22, 37);

            public string NoPresentation { get; set; } = "no show";

            [AlertPredicateProperty]
            public string OnlyPredicate { get; set; } = "only predicate";
        }

        private class TestAlertNoSummaryChart : TestAlertNoSummary
        {
            [AlertPredicateProperty]
            [AlertPresentationProperty(AlertPresentationSection.Property, "CPU increased", InfoBalloon = "CPU increase on machine {MachineName}")]
            public new double Value => 22.4;
        }
    }
}