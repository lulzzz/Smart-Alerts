//-----------------------------------------------------------------------
// <copyright file="SmartSignalResultItemPresentation.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.SignalResultPresentation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartSignals.Clients;
    using Microsoft.Azure.Monitoring.SmartSignals.Extensions;
    using Newtonsoft.Json;
    using SmartFormat;

    /// <summary>
    /// This class holds the presentation information of the result item -
    /// the way a result item should be presented in the UI
    /// </summary>
    public class SmartSignalResultItemPresentation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmartSignalResultItemPresentation"/> class
        /// </summary>
        /// <param name="id">The result item ID</param>
        /// <param name="title">The result item title</param>
        /// <param name="resourceId">The result item resource ID</param>
        /// <param name="correlationHash">The result item correlation hash</param>
        /// <param name="signalId">The signal ID</param>
        /// <param name="signalName">The signal name</param>
        /// <param name="analysisTimestamp">The end time of the analysis window</param>
        /// <param name="analysisWindowSizeInMinutes">The analysis window size (in minutes)</param>
        /// <param name="properties">The result item properties</param>
        /// <param name="rawProperties">The raw result item properties</param>
        /// <param name="queryRunInfo">The query run information</param>
        public SmartSignalResultItemPresentation(
            string id, 
            string title, 
            string resourceId, 
            string correlationHash, 
            string signalId, 
            string signalName, 
            DateTime analysisTimestamp, 
            int analysisWindowSizeInMinutes, 
            List<SmartSignalResultItemPresentationProperty> properties, 
            IReadOnlyDictionary<string, string> rawProperties, 
            SmartSignalResultItemQueryRunInfo queryRunInfo)
        {
            this.Id = id;
            this.Title = title;
            this.ResourceId = resourceId;
            this.CorrelationHash = correlationHash;
            this.SignalId = signalId;
            this.SignalName = signalName;
            this.AnalysisTimestamp = analysisTimestamp;
            this.AnalysisWindowSizeInMinutes = analysisWindowSizeInMinutes;
            this.Properties = properties;
            this.RawProperties = rawProperties;
            this.QueryRunInfo = queryRunInfo;
        }

        /// <summary>
        /// Gets the result item ID
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; }

        /// <summary>
        /// Gets the result item title
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; }
        
        /// <summary>
        /// Gets the result item resource ID
        /// </summary>
        [JsonProperty("resourceId")]
        public string ResourceId { get; }

        /// <summary>
        /// Gets the result item correlation hash
        /// </summary>
        [JsonProperty("correlationHash")]
        public string CorrelationHash { get; }

        /// <summary>
        /// Gets the signal ID
        /// </summary>
        [JsonProperty("signalId")]
        public string SignalId { get; }

        /// <summary>
        /// Gets the signal name
        /// </summary>
        [JsonProperty("signalName")]
        public string SignalName { get; }

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
        /// Gets the result item properties
        /// </summary>
        [JsonProperty("properties")]
        public List<SmartSignalResultItemPresentationProperty> Properties { get; }

        /// <summary>
        /// Gets the raw result item properties
        /// </summary>
        [JsonProperty("rawProperties")]
        public IReadOnlyDictionary<string, string> RawProperties { get; }

        /// <summary>
        /// Gets the query run information
        /// </summary>
        [JsonProperty("queryRunInfo")]
        public SmartSignalResultItemQueryRunInfo QueryRunInfo { get; }

        /// <summary>
        /// Creates a presentation from a result item
        /// </summary>
        /// <param name="request">The smart signal request</param>
        /// <param name="signalName">The signal name</param>
        /// <param name="smartSignalResultItem">The result item</param>
        /// <param name="queryRunInfo">The query run information</param>
        /// <returns>The presentation</returns>
        public static SmartSignalResultItemPresentation CreateFromResultItem(SmartSignalRequest request, string signalName, Alert smartSignalResultItem, SmartSignalResultItemQueryRunInfo queryRunInfo)
        {
            // A null result item has null presentation
            if (smartSignalResultItem == null)
            {
                return null;
            }

            // Create presentation elements for each result item property
            Dictionary<string, string> predicates = new Dictionary<string, string>();
            List<SmartSignalResultItemPresentationProperty> presentationProperties = new List<SmartSignalResultItemPresentationProperty>();
            Dictionary<string, string> rawProperties = new Dictionary<string, string>();
            foreach (PropertyInfo property in smartSignalResultItem.GetType().GetProperties())
            {
                // Get the property value
                string propertyValue = PropertyValueToString(property.GetValue(smartSignalResultItem));
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
                        throw new InvalidSmartSignalResultItemPresentationException($"The presentation contains an item for the {attribute.Section} section, but no telemetry data client was provided");
                    }

                    // Get the attribute title and information balloon - support interpolated strings
                    string attributeTitle = Smart.Format(attribute.Title, smartSignalResultItem);
                    string attributeInfoBalloon = Smart.Format(attribute.InfoBalloon, smartSignalResultItem);

                    // Add the presentation property
                    presentationProperties.Add(new SmartSignalResultItemPresentationProperty(attributeTitle, propertyValue, attribute.Section, attributeInfoBalloon));
                }
            }

            string id = string.Join("##", smartSignalResultItem.GetType().FullName, JsonConvert.SerializeObject(request), JsonConvert.SerializeObject(smartSignalResultItem)).Hash();
            string resourceId = smartSignalResultItem.ResourceIdentifier.ToResourceId();
            string correlationHash = string.Join("##", predicates.OrderBy(x => x.Key).Select(x => x.Key + "|" + x.Value)).Hash();

            // Return the presentation object
            return new SmartSignalResultItemPresentation(
                id,
                smartSignalResultItem.Title,
                resourceId,
                correlationHash,
                request.SignalId,
                signalName,
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