//-----------------------------------------------------------------------
// <copyright file="AlertDetailsControlViewModel.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliancEmulator.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Contracts;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliancEmulator.Models;
    using Unity.Attributes;

    /// <summary>
    /// The view model class for the <see cref="AlertDetailsControl"/> control.
    /// </summary>
    public class AlertDetailsControlViewModel : ObservableObject
    {
        private Models.Alert alert;

        private ObservableCollection<AzureResourceProperty> essentialsSectionProperties;

        private ObservableCollection<AlertProperty> propertiesSectionProperties;

        private ObservableCollection<AlertProperty> analysisSectionProperties;

        private ObservableCollection<AnalyticsQuery> analyticsQueries;

        #region Ctros

        /// <summary>
        /// Initializes a new instance of the <see cref="AlertDetailsControlViewModel"/> class for design time only.
        /// </summary>
        public AlertDetailsControlViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AlertDetailsControlViewModel" /> class.
        /// </summary>
        /// <param name="alert">The alert runner.</param>
        /// <param name="alertDetailsControlClosed">The Smart Detector runner gunner.</param>
        [InjectionConstructor]
        public AlertDetailsControlViewModel(
            Models.Alert alert, 
            AlertDetailsControlClosedEventHandler alertDetailsControlClosed)
        {
            this.Alert = alert;

            this.EssentialsSectionProperties = new ObservableCollection<AzureResourceProperty>(new List<AzureResourceProperty>()
                {
                    new AzureResourceProperty("Subscription id", this.Alert.ResourceIdentifier.SubscriptionId),
                    new AzureResourceProperty("Resource group", this.Alert.ResourceIdentifier.ResourceGroupName),
                    new AzureResourceProperty("Resource type", this.Alert.ResourceIdentifier.ResourceType.ToString()),
                    new AzureResourceProperty("Resource name", this.Alert.ResourceIdentifier.ResourceName)
                });

            this.PropertiesSectionProperties = new ObservableCollection<AlertProperty>(
                this.Alert.ContractsAlert.Properties
                    .Where(prop => prop.DisplayCategory.ToString() == AlertPresentationSection.Property.ToString()).ToList());

            this.AnalysisSectionProperties = new ObservableCollection<AlertProperty>(
                this.Alert.ContractsAlert.Properties
                    .Where(prop => prop.DisplayCategory.ToString() == AlertPresentationSection.Analysis.ToString()).ToList());

            List<AnalyticsQuery> queries = this.Alert.ContractsAlert.Properties
                    .Where(prop => prop.DisplayCategory.ToString() == AlertPresentationSection.Chart.ToString())
                    .Select(chartItem => new AnalyticsQuery(chartItem.Name, chartItem.Value)).ToList();

            this.AnalyticsQuerys = new ObservableCollection<AnalyticsQuery>(queries);

            this.CloseControlCommand = new CommandHandler(() =>
            {
                alertDetailsControlClosed.Invoke();
            });
        }

        #endregion

        #region Binded Properties

        /// <summary>
        /// Gets the alert.
        /// </summary>
        public Models.Alert Alert
        {
            get
            {
                return this.alert;
            }

            private set
            {
                this.alert = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the essentials section's properties.
        /// </summary>
        public ObservableCollection<AzureResourceProperty> EssentialsSectionProperties
        {
            get
            {
                return this.essentialsSectionProperties;
            }

            private set
            {
                this.essentialsSectionProperties = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the properties section's properties.
        /// </summary>
        public ObservableCollection<AlertProperty> PropertiesSectionProperties
        {
            get
            {
                return this.propertiesSectionProperties;
            }

            private set
            {
                this.propertiesSectionProperties = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the analysis section's properties.
        /// </summary>
        public ObservableCollection<AlertProperty> AnalysisSectionProperties
        {
            get
            {
                return this.analysisSectionProperties;
            }

            private set
            {
                this.analysisSectionProperties = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the App Analytics queries.
        /// </summary>
        public ObservableCollection<AnalyticsQuery> AnalyticsQuerys
        {
            get
            {
                return this.analyticsQueries;
            }

            private set
            {
                this.analyticsQueries = value;
                this.OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Gets the command that runs the Smart Detector.
        /// </summary>
        public CommandHandler CloseControlCommand { get; }

        /// <summary>
        /// Gets a command to open an analytics kusto query in a new browser tab.
        /// </summary>
        public CommandHandler OpenAnalyticsQueryCommand => new CommandHandler(queryParameter =>
        {
            // Get the query from the parameter
            string query = (string)queryParameter;

            // Compress it so we can add it to the query parameters
            string compressedQuery;
            using (var outputStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
                {
                    byte[] queryBtyes = Encoding.UTF8.GetBytes(query);
                    gzipStream.Write(queryBtyes, 0, queryBtyes.Length);
                }

                compressedQuery = Convert.ToBase64String(outputStream.ToArray());
            }

            // Compose the URI
            string endpoint;
            string resourceUrlParameterName;
            if (this.Alert.ResourceIdentifier.ResourceType == ResourceType.ApplicationInsights)
            {
                endpoint = "analytics.applicationinsights.io";
                resourceUrlParameterName = "components"; 
            }
            else
            {
                endpoint = "portal.loganalytics.io";
                resourceUrlParameterName = "workspaces";
            }

            // Use the first resource ID from query run info for the query.
            // It might not work for Log Analytics results - since there might be few resources.
            // Anyway, this is temporary hack until there will be query visualizations in emulator.
            string alertResourceId = this.Alert.ContractsAlert.QueryRunInfo.ResourceIds.First();
            ResourceIdentifier alertResourceIdentifier = ResourceIdentifier.CreateFromResourceId(alertResourceId);

            Uri queryDeepLink =
                new Uri($"https://{endpoint}/subscriptions/{alertResourceIdentifier.SubscriptionId}/resourcegroups/{alertResourceIdentifier.ResourceGroupName}/{resourceUrlParameterName}/{alertResourceIdentifier.ResourceName}?q={compressedQuery}");

            Process.Start(new ProcessStartInfo(queryDeepLink.AbsoluteUri));
        });

        #endregion
    }
}
