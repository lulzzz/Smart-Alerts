//-----------------------------------------------------------------------
// <copyright file="SignalResultDetailsControlViewModel.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Emulator.ViewModels
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
    using Microsoft.Azure.Monitoring.SmartDetectors.Presentation;
    using Microsoft.Azure.Monitoring.SmartSignals.Emulator.Models;
    using Unity.Attributes;

    /// <summary>
    /// The view model class for the <see cref="SignalResultDetailsControl"/> control.
    /// </summary>
    public class SignalResultDetailsControlViewModel : ObservableObject
    {
        private SignalResultItem signalResult;

        private ObservableCollection<AzureResourceProperty> essentialsSectionProperties;

        private ObservableCollection<AlertPresentationProperty> propertiesSectionProperties;

        private ObservableCollection<AlertPresentationProperty> analysisSectionProperties;

        private ObservableCollection<AnalyticsQuery> analyticsQueries;

        #region Ctros

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalResultDetailsControlViewModel"/> class for design time only.
        /// </summary>
        public SignalResultDetailsControlViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalResultDetailsControlViewModel" /> class.
        /// </summary>
        /// <param name="signalResult">The smart signal runner.</param>
        /// <param name="resultDetailsControlClosed">The smart signal runner gunner.</param>
        [InjectionConstructor]
        public SignalResultDetailsControlViewModel(
            SignalResultItem signalResult, 
            ResultDetailsControlClosedEventHandler resultDetailsControlClosed)
        {
            this.SignalResult = signalResult;

            this.EssentialsSectionProperties = new ObservableCollection<AzureResourceProperty>(new List<AzureResourceProperty>()
                {
                    new AzureResourceProperty("Subscription id", this.SignalResult.ResourceIdentifier.SubscriptionId),
                    new AzureResourceProperty("Resource group", this.SignalResult.ResourceIdentifier.ResourceGroupName),
                    new AzureResourceProperty("Resource type", this.SignalResult.ResourceIdentifier.ResourceType.ToString()),
                    new AzureResourceProperty("Resource name", this.SignalResult.ResourceIdentifier.ResourceName)
                });

            this.PropertiesSectionProperties = new ObservableCollection<AlertPresentationProperty>(
                this.SignalResult.AlertPresentation.Properties
                    .Where(prop => prop.DisplayCategory == AlertPresentationSection.Property).ToList());

            this.AnalysisSectionProperties = new ObservableCollection<AlertPresentationProperty>(
                this.SignalResult.AlertPresentation.Properties
                    .Where(prop => prop.DisplayCategory == AlertPresentationSection.Analysis).ToList());

            List<AnalyticsQuery> queries = this.SignalResult.AlertPresentation.Properties
                    .Where(prop => prop.DisplayCategory == AlertPresentationSection.Chart)
                    .Select(chartItem => new AnalyticsQuery(chartItem.Name, chartItem.Value)).ToList();

            this.AnalyticsQuerys = new ObservableCollection<AnalyticsQuery>(queries);

            this.CloseControlCommand = new CommandHandler(() =>
            {
                resultDetailsControlClosed.Invoke();
            });
        }

        #endregion

        #region Binded Properties

        /// <summary>
        /// Gets the signal result.
        /// </summary>
        public SignalResultItem SignalResult
        {
            get
            {
                return this.signalResult;
            }

            private set
            {
                this.signalResult = value;
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
        public ObservableCollection<AlertPresentationProperty> PropertiesSectionProperties
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
        public ObservableCollection<AlertPresentationProperty> AnalysisSectionProperties
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
        /// Gets the command that runs the signal.
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
            if (this.SignalResult.ResourceIdentifier.ResourceType == ResourceType.ApplicationInsights)
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
            string resultResourceId = this.SignalResult.AlertPresentation.QueryRunInfo.ResourceIds.First();
            ResourceIdentifier resultResourceIdentifier = ResourceIdentifier.CreateFromResourceId(resultResourceId);

            Uri queryDeepLink =
                new Uri($"https://{endpoint}/subscriptions/{resultResourceIdentifier.SubscriptionId}/resourcegroups/{resultResourceIdentifier.ResourceGroupName}/{resourceUrlParameterName}/{resultResourceIdentifier.ResourceName}?q={compressedQuery}");

            Process.Start(new ProcessStartInfo(queryDeepLink.AbsoluteUri));
        });

        #endregion
    }
}
