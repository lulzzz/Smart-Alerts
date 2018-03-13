//-----------------------------------------------------------------------
// <copyright file="AlertExtensions.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Presentation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Azure.Monitoring.SmartDetectors.Extensions;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Contracts;
    using Newtonsoft.Json;
    using SmartFormat;
    using Alert = Microsoft.Azure.Monitoring.SmartDetectors.Alert;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Contracts.Alert;

    /// <summary>
    /// A class for Alert extension methods
    /// </summary>
    public static class AlertExtensions
    {
        /// <summary>
        /// Creates a presentation from a alert
        /// </summary>
        /// <param name="alert">The alert</param>
        /// <param name="request">The Smart Detector request</param>
        /// <param name="smartDetectorName">The Smart Detector name</param>
        /// <param name="queryRunInfo">The query run information</param>
        /// <returns>The presentation</returns>
        public static ContractsAlert CreateContractsAlert(this Alert alert, SmartDetectorExecutionRequest request, string smartDetectorName, QueryRunInfo queryRunInfo)
        {
            // A null alert has null presentation
            if (alert == null)
            {
                return null;
            }

            // Create presentation elements for each alert property
            Dictionary<string, string> predicates = new Dictionary<string, string>();
            List<AlertProperty> alertProperties = new List<AlertProperty>();
            Dictionary<string, string> rawProperties = new Dictionary<string, string>();
            foreach (PropertyInfo property in alert.GetType().GetProperties())
            {
                // Get the property value
                string propertyValue = PropertyValueToString(property.GetValue(alert));
                if (string.IsNullOrWhiteSpace(propertyValue))
                {
                    // not accepting empty properties
                    continue;
                }

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
                    alertProperties.Add(new AlertProperty
                    {
                        Name = attributeTitle,
                        Value = propertyValue,
                        DisplayCategory = GetDisplayCategoryFromPresentationSection(attribute.Section),
                        InfoBalloon = attributeInfoBalloon
                    });
                }
            }

            string id = string.Join("##", alert.GetType().FullName, JsonConvert.SerializeObject(request), JsonConvert.SerializeObject(alert)).Hash();
            string resourceId = alert.ResourceIdentifier.ToResourceId();
            string correlationHash = string.Join("##", predicates.OrderBy(x => x.Key).Select(x => x.Key + "|" + x.Value)).Hash();

            // Return the presentation object
            return new ContractsAlert
            {
                Id = id,
                Title = alert.Title,
                ResourceId = resourceId,
                CorrelationHash = correlationHash,
                SmartDetectorId = request.SmartDetectorId,
                SmartDetectorName = smartDetectorName,
                AnalysisTimestamp = DateTime.UtcNow,
                AnalysisWindowSizeInMinutes = (int)request.Cadence.TotalMinutes,
                Properties = alertProperties,
                RawProperties = rawProperties,
                QueryRunInfo = queryRunInfo
            };
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

        /// <summary>
        /// Gets the display category enum value from the presentation section enum value
        /// </summary>
        /// <param name="presentationSection">The property presentation section</param>
        /// <returns>The display category that coralline with the presentation section</returns>
        private static AlertPropertyDisplayCategory GetDisplayCategoryFromPresentationSection(AlertPresentationSection presentationSection)
        {
            switch (presentationSection)
            {
                case AlertPresentationSection.AdditionalQuery:
                    return AlertPropertyDisplayCategory.AdditionalQuery;

                case AlertPresentationSection.Analysis:
                    return AlertPropertyDisplayCategory.Analysis;

                case AlertPresentationSection.Chart:
                    return AlertPropertyDisplayCategory.Chart;

                case AlertPresentationSection.Property:
                default:
                    return AlertPropertyDisplayCategory.Property;
            }
        }
    }
}
