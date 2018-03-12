//-----------------------------------------------------------------------
// <copyright file="HighRequestCountAlert.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.SampleSmartDetector
{
    using System;
    using Microsoft.Azure.Monitoring.SmartDetectors;

    /// <summary>
    /// A class representing the alert
    /// </summary>
    public class HighRequestCountAlert : Alert
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HighRequestCountAlert" /> class with the values 
        /// that were fetched from the Application Insights and the log analytics
        /// </summary>
        /// <param name="title">The title of the alert</param>
        /// <param name="appName">The name of the application with the highest number of requests (from Application Insights)</param>
        /// <param name="requestCount">The highest number of requests (from Application Insights)</param>
        /// <param name="highestProcessorTimePercent">The highest processor time percent (from Log Analytics)</param>
        /// <param name="timeOfHighestProcessorTimePercent">The time of the highest processor time percent (from Log Analytics)</param>
        /// <param name="resourceIdentifier">The resource identifier</param>
        public HighRequestCountAlert(string title, string appName, long requestCount, double highestProcessorTimePercent, DateTime timeOfHighestProcessorTimePercent, ResourceIdentifier resourceIdentifier) : base(title, resourceIdentifier)
        {
            this.AppName = appName;
            this.RequestCount = requestCount;
            this.HighestProcessorTimePercent = highestProcessorTimePercent;
            this.TimeOfHighestProcessorTimePercent = timeOfHighestProcessorTimePercent;
        }

        /// <summary>
        /// Gets or sets the max request count - fetched from Application Insights
        /// </summary>
        [AlertPredicateProperty]
        [AlertPresentationProperty(AlertPresentationSection.Property, "Maximum Request Count for the application", InfoBalloon = "Maximum requests for application '{AppName}'")]
        public long RequestCount { get; set; }

        /// <summary>
        /// Gets or sets the name of the application with the maximum requests in the last day - fetched from Application Insights
        /// </summary>
        [AlertPredicateProperty]
        [AlertPresentationProperty(AlertPresentationSection.Property, "App Name", InfoBalloon = "App Name")]
        public string AppName { get; set; }

        /// <summary>
        /// Gets or sets the highest Processor Time percentage in the last hour - fetched from log analytics
        /// </summary>
        [AlertPredicateProperty]
        [AlertPresentationProperty(AlertPresentationSection.Property, "Highest Processor Time Percentage", InfoBalloon = "The highest Processor Time percentage")]
        public double HighestProcessorTimePercent { get; set; }

        /// <summary>
        /// Gets or sets the time of the highest Processor Time percentage in the last hour - fetched from log analytics
        /// </summary>
        [AlertPredicateProperty]
        [AlertPresentationProperty(AlertPresentationSection.Property, "Highest CPU Percentage Time", InfoBalloon = "When was the highest CPU percentage?")]
        public DateTime TimeOfHighestProcessorTimePercent { get; set; }

        /// <summary>
        /// Describe the problem
        /// </summary>
        [AlertPresentationProperty(AlertPresentationSection.Analysis, "What is the problem?", InfoBalloon = "The problem analysis")]
        public string Problem => "There was an unusually high request count for the application";

        /// <summary>
        /// Time chart to appear in the details component - fetched from Log Analytics by default
        /// </summary>
        [AlertPresentationProperty(AlertPresentationSection.Chart, "Time Chart", InfoBalloon = "Time Chart showing requests per app in the last day")]
        public string DetailQuery => "Perf | where TimeGenerated  >= ago(1h) | where CounterName == '% Processor Time'| project CounterValue,TimeGenerated   | render timechart ";

        /// <summary>
        /// Bar chart to appear in the summary component - fetched from Log Analytics by default
        /// </summary>
        [AlertPresentationProperty(AlertPresentationSection.Chart, "Bar Chart", InfoBalloon = "Bar Chart showing requests per app  in the last day")]
        public string SummaryQuery => "Perf | where TimeGenerated  >= ago(1h) | where CounterName == '% Processor Time'| project CounterValue,TimeGenerated   | render barchart";

        /// <summary>
        /// All the data that is displayed in the charts - fetched from Log Analytics by default
        /// </summary>
        [AlertPresentationProperty(AlertPresentationSection.AdditionalQuery, "More Information", InfoBalloon = "More Information...")]
        public string AdditionalQuery => "Perf | where TimeGenerated  >= ago(1h) | where CounterName == '% Processor Time'| project CounterValue,TimeGenerated";
    }
}
