//-----------------------------------------------------------------------
// <copyright file="AlertPresentationProperty.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Presentation
{
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Newtonsoft.Json;

    /// <summary>
    /// This class holds presentation information of a single alert property
    /// </summary>
    public class AlertPresentationProperty
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlertPresentationProperty"/> class
        /// </summary>
        /// <param name="name">The property name</param>
        /// <param name="value">The property value</param>
        /// <param name="displayCategory">The property display category</param>
        /// <param name="infoBalloon">The property information balloon</param>
        public AlertPresentationProperty(string name, string value, AlertPresentationSection displayCategory, string infoBalloon)
        {
            this.Name = name;
            this.Value = value;
            this.DisplayCategory = displayCategory;
            this.InfoBalloon = infoBalloon;
        }

        /// <summary>
        /// Gets the property name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; }

        /// <summary>
        /// Gets the property value
        /// </summary>
        [JsonProperty("value")]
        public string Value { get; }

        /// <summary>
        /// Gets the property display category
        /// </summary>
        [JsonProperty("displayCategory")]
        public AlertPresentationSection DisplayCategory { get; }

        /// <summary>
        /// Gets the property information balloon
        /// </summary>
        [JsonProperty("infoBalloon")]
        public string InfoBalloon { get; }
    }
}