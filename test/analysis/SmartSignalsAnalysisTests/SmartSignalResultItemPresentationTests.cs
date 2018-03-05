//-----------------------------------------------------------------------
// <copyright file="SmartSignalResultItemPresentationTests.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartSignalsAnalysisTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Microsoft.Azure.Monitoring.SmartSignals.SignalResultPresentation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class SmartSignalResultItemPresentationTests
    {
        private const string SignalName = "signalName";

        [TestInitialize]
        public void TestInitialize()
        {
        }

        [TestMethod]
        public void WhenProcessingSmartSignalResultItemThenThePresentationIsCreatedCorrectly()
        {
            var presentation = this.CreatePresentation(new TestResultItem());
            Assert.IsTrue(presentation.AnalysisTimestamp <= DateTime.UtcNow, "Unexpected analysis timestamp in the future");
            Assert.IsTrue(presentation.AnalysisTimestamp >= DateTime.UtcNow.AddMinutes(-1), "Unexpected analysis timestamp - too back in the past");
            Assert.AreEqual(24 * 60, presentation.AnalysisWindowSizeInMinutes, "Unexpected analysis window size");
            Assert.AreEqual(SignalName, presentation.SignalName, "Unexpected signal name");
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
        public void WhenProcessingSmartSignalResultItemWithoutSummaryChartThenNoExceptionIsThrown()
        {
            this.CreatePresentation(new TestResultItemNoSummaryChart());
        }

        [TestMethod]
        public void WhenSmartSignalResultItemsHaveDifferentPredicatesThenTheCorrelationHashIsDifferent()
        {
            var resultItem1 = new TestResultItem();
            var resultItem2 = new TestResultItem();
            resultItem2.NoPresentation += "X";

            var presentation1 = this.CreatePresentation(resultItem1);

            // A non predicate property is different - correlation hash should be the same
            var presentation2 = this.CreatePresentation(resultItem2);
            Assert.AreNotEqual(presentation1.Id, presentation2.Id);
            Assert.AreEqual(presentation1.CorrelationHash, presentation2.CorrelationHash);

            // A predicate property is different - correlation hash should be the different
            resultItem2.OnlyPredicate += "X";
            presentation2 = this.CreatePresentation(resultItem2);
            Assert.AreNotEqual(presentation1.Id, presentation2.Id);
            Assert.AreNotEqual(presentation1.CorrelationHash, presentation2.CorrelationHash);
        }

        [TestMethod]
        public void WhenProcessingSmartSignalResultItemWithoutQueriesAndNullRunInfoThenThePresentationIsCreatedSuccessfully()
        {
            this.CreatePresentation(new TestResultItemNoQueries(), nullQueryRunInfo: true);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidSmartSignalResultItemPresentationException))]
        public void WhenProcessingSmartSignalResultItemWithQueriesAndNullRunInfoThenAnExceptionIsThrown()
        {
            this.CreatePresentation(new TestResultItem(), nullQueryRunInfo: true);
        }

        private SmartSignalResultItemPresentation CreatePresentation(Alert resultItem, bool nullQueryRunInfo = false)
        {
            SmartSignalResultItemQueryRunInfo queryRunInfo = null;
            if (!nullQueryRunInfo)
            {
                queryRunInfo = new SmartSignalResultItemQueryRunInfo(
                    resultItem.ResourceIdentifier.ResourceType == ResourceType.ApplicationInsights ? TelemetryDbType.ApplicationInsights : TelemetryDbType.LogAnalytics,
                    new List<string>() { "resourceId1", "resourceId2" });
            }

            DateTime lastExecutionTime = DateTime.Now.Date.AddDays(-1);
            string resourceId = "resourceId";
            var request = new SmartSignalRequest(new List<string>() { resourceId }, "signalId", lastExecutionTime, TimeSpan.FromDays(1), new SmartSignalSettings());
            return SmartSignalResultItemPresentation.CreateFromResultItem(request, SignalName, resultItem, queryRunInfo);
        }

        private void VerifyProperty(List<SmartSignalResultItemPresentationProperty> properties, string name, AlertPresentationSection displayCategory, string value, string infoBalloon)
        {
            var property = properties.SingleOrDefault(p => p.Name == name);
            Assert.IsNotNull(property, $"Property {name} not found");
            Assert.AreEqual(displayCategory, property.DisplayCategory);
            Assert.AreEqual(value, property.Value);
            Assert.AreEqual(infoBalloon, property.InfoBalloon);
        }

        private class TestResultItemNoSummary : Alert
        {
            public TestResultItemNoSummary()
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

        private class TestResultItemNoQueries : TestResultItemNoSummary
        {
            [AlertPredicateProperty]
            [AlertPresentationProperty(AlertPresentationSection.Property, "CPU increased", InfoBalloon = "CPU increase on machine {MachineName}")]
            public new double Value => 22.4;
        }

        private class TestResultItemNoSummaryProperty : TestResultItemNoSummary
        {
            [AlertPresentationProperty(AlertPresentationSection.Chart, "CPU over the last 7 days", InfoBalloon = "CPU chart for machine {MachineName}, showing increase of {Value}")]
            public string CpuChartQuery => "<the query>";
        }

        private class TestResultItem : TestResultItemNoSummaryProperty
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

        private class TestResultItemNoSummaryChart : TestResultItemNoSummary
        {
            [AlertPredicateProperty]
            [AlertPresentationProperty(AlertPresentationSection.Property, "CPU increased", InfoBalloon = "CPU increase on machine {MachineName}")]
            public new double Value => 22.4;
        }
    }
}