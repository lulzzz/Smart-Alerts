//-----------------------------------------------------------------------
// <copyright file="AlertPresentation.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Presentation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Clients;
    using Microsoft.Azure.Monitoring.SmartDetectors.Extensions;
    using Newtonsoft.Json;
    using SmartFormat;

    /// <summary>
    /// This class holds the presentation information of the alert -
    /// the way a alert should be presented in the UI
    /// </summary>
    public class AlertPresentation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlertPresentation"/> class
        /// </summary>
        /// <param name="id">The alert ID</param>
        /// <param name="title">The alert title</param>
        /// <param name="resourceId">The alert resource ID</param>
        /// <param name="correlationHash">The alert correlation hash</param>
        /// <param name="smartDetectorId">The Smart Detector ID</param>
        /// <param name="smartDetectorName">The Smart Detector name</param>
        /// <param name="analysisTimestamp">The end time of the analysis window</param>
        /// <param name="analysisWindowSizeInMinutes">The analysis window size (in minutes)</param>
        /// <param name="properties">The alert properties</param>
        /// <param name="rawProperties">The raw alert properties</param>
        /// <param name="queryRunInfo">The query run information</param>
        public AlertPresentation(
            string id,
            string title,
            string resourceId,
            string correlationHash,
            string smartDetectorId,
            string smartDetectorName,
            DateTime analysisTimestamp,
            int analysisWindowSizeInMinutes,
            List<AlertPresentationProperty> properties,
            IReadOnlyDictionary<string, string> rawProperties,
            QueryRunInfo queryRunInfo)
        {
            this.Id = id;
            this.Title = title;
            this.ResourceId = resourceId;
            this.CorrelationHash = correlationHash;
            this.SmartDetectorId = smartDetectorId;
            this.SmartDetectorName = smartDetectorName;
            this.AnalysisTimestamp = analysisTimestamp;
            this.AnalysisWindowSizeInMinutes = analysisWindowSizeInMinutes;
            this.Properties = properties;
            this.RawProperties = rawProperties;
            this.QueryRunInfo = queryRunInfo;
        }

        /// <summary>
        /// Gets the alert ID
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; }

        /// <summary>
        /// Gets the alert title
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; }

        /// <summary>
        /// Gets the alert resource ID
        /// </summary>
        [JsonProperty("resourceId")]
        public string ResourceId { get; }

        /// <summary>
        /// Gets the alert correlation hash
        /// </summary>
        [JsonProperty("correlationHash")]
        public string CorrelationHash { get; }

        /// <summary>
        /// Gets the Smart Detector ID
        /// </summary>
        [JsonProperty("smartDetectorId")]
        public string SmartDetectorId { get; }

        /// <summary>
        /// Gets the Smart Detector name
        /// </summary>
        [JsonProperty("smartDetectorName")]
        public string SmartDetectorName { get; }

        /// <summary>
        /// Gets the end time of the analysis window
        /// </summary>
        [JsonProperty("analysisTimestamp")]
        public DateTime AnalysisTimestamp { get; }

        /// <summary>
        /// Gets the analysis window size (in minutes)
        /// </summary>
        [JsonProperty("analysisWindowSizeInMinutes")]
        public int AnalysisWindowSizeInMinutes { get; }

        /// <summary>
        /// Gets the alert properties
        /// </summary>
        [JsonProperty("properties")]
        public List<AlertPresentationProperty> Properties { get; }

        /// <summary>
        /// Gets the raw alert properties
        /// </summary>
        [JsonProperty("rawProperties")]
        public IReadOnlyDictionary<string, string> RawProperties { get; }

        /// <summary>
        /// Gets the query run information
        /// </summary>
        [JsonProperty("queryRunInfo")]
        public QueryRunInfo QueryRunInfo { get; }

        /// <summary>
        /// Creates a presentation from a alert
        /// </summary>
        /// <param name="request">The Smart Detector request</param>
        /// <param name="smartDetectorName">The Smart Detector name</param>
        /// <param name="alert">The alert</param>
        /// <param name="queryRunInfo">The query run information</param>
        /// <returns>The presentation</returns>
        public static AlertPresentation CreateFromAlert(SmartDetectorRequest request, string smartDetectorName, Alert alert, QueryRunInfo queryRunInfo)
        {
            // A null alert has null presentation
            if (alert == null)
            {
                return null;
            }

            // Create presentation elements for each alert property
            Dictionary<string, string> predicates = new Dictionary<string, string>();
            List<AlertPresentationProperty> presentationProperties = new List<AlertPresentationProperty>();
            Dictionary<string, string> rawProperties = new Dictionary<string, string>();
            foreach (PropertyInfo property in alert.GetType().GetProperties())
            {
                // Get the property value
                string propertyValue = PropertyValueToString(property.GetValue(alert));
                rawProperties[property.Name] = propertyValue;

                // Check if this property is a predicate
                if (property.GetCustomAttribute<AlertPredicatePropertyAttribute>() != null)
                {
                    predicates[property.Name] = propertyValue;
                }

                // Get the presentation attribute
                AlertPresentationPropertyAttribute attribute = property.GetCustomAttribute<AlertPresentationPropertyAttribute>();
                if (attribute != null)
                {
                    // Verify that if the entity is a chart or query, then query run information was provided
                    if (queryRunInfo == null && (attribute.Section == AlertPresentationSection.Chart || attribute.Section == AlertPresentationSection.AdditionalQuery))
                    {
                        throw new InvalidAlertPresentationException($"The presentation contains an item for the {attribute.Section} section, but no telemetry data client was provided");
                    }

                    // Get the attribute title and information balloon - support interpolated strings
                    string attributeTitle = Smart.Format(attribute.Title, alert);
                    string attributeInfoBalloon = Smart.Format(attribute.InfoBalloon, alert);

                    // Add the presentation property
                    presentationProperties.Add(new AlertPresentationProperty(attributeTitle, propertyValue, attribute.Section, attributeInfoBalloon));
                }
            }

            string id = string.Join("##", alert.GetType().FullName, JsonConvert.SerializeObject(request), JsonConvert.SerializeObject(alert)).Hash();
            string resourceId = alert.ResourceIdentifier.ToResourceId();
            string correlationHash = string.Join("##", predicates.OrderBy(x => x.Key).Select(x => x.Key + "|" + x.Value)).Hash();

            // Return the presentation object
            return new AlertPresentation(
                id,
                alert.Title,
                resourceId,
                correlationHash,
                request.SmartDetectorId,
                smartDetectorName,
                DateTime.UtcNow,
                (int)request.Cadence.TotalMinutes,
                presentationProperties,
                rawProperties,
                queryRunInfo);
        }

        /// <summary>
        /// Converts a presentation property's value to a string
        /// </summary>
        /// <param name="propertyValue">The property value</param>
        /// <returns>The string</returns>
        private static string PropertyValueToString(object propertyValue)
        {
            if (propertyValue == null)
            {
                // null is an empty string
                return string.Empty;
            }
            else if (propertyValue is DateTime)
            {
                // Convert to universal sortable time
                return ((DateTime)propertyValue).ToString("u");
            }
            else
            {
                return propertyValue.ToString();
            }
        }
    }
}