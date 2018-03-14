//-----------------------------------------------------------------------
// <copyright file="SmartDetectorConfigurationControlViewModel.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliancEmulator.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Clients;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliancEmulator.Controls;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliancEmulator.Models;
    using Microsoft.Azure.Monitoring.SmartDetectors.Package;
    using Unity.Attributes;

    /// <summary>
    /// The view model class for the <see cref="SmartDetectorConfigurationControl"/> control.
    /// </summary>
    public class SmartDetectorConfigurationControlViewModel : ObservableObject
    {
        private readonly IAzureResourceManagerClient azureResourceManagerClient;

        private readonly SmartDetectorManifest smartDetectorManifes;

        private readonly ITracer tracer;

        private SmartDetectorRunner smartDetectorRunner;

        private string smartDetectorName;

        private ObservableCollection<SmartDetectorCadence> cadences;

        private SmartDetectorCadence selectedCadence;

        private ObservableTask<ObservableCollection<AzureSubscription>> readSubscriptionsTask;

        private AzureSubscription selectedSubscription;

        private ObservableTask<ObservableCollection<string>> readResourceGroupsTask;

        private string selectedResourceGroup;

        private ObservableTask<ObservableCollection<string>> readResourceTypesTask;

        private string selectedResourceType;

        private ObservableTask<ObservableCollection<ResourceIdentifier>> readResourcesTask;

        private ResourceIdentifier selectedResource;

        private bool shouldShowStatusControl;

        #region Ctros

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartDetectorConfigurationControlViewModel"/> class for design time only.
        /// </summary>
        public SmartDetectorConfigurationControlViewModel()
        {
            this.ShouldShowStatusControl = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartDetectorConfigurationControlViewModel"/> class.
        /// </summary>
        /// <param name="azureResourceManagerClient">The Azure resources manager client.</param>
        /// <param name="tracer">The tracer.</param>
        /// <param name="smartDetectorManifest">The Smart Detector manifest.</param>
        /// <param name="smartDetectorRunner">The Smart Detector runner.</param>
        [InjectionConstructor]
        public SmartDetectorConfigurationControlViewModel(
            IAzureResourceManagerClient azureResourceManagerClient,
            ITracer tracer,
            SmartDetectorManifest smartDetectorManifest,
            SmartDetectorRunner smartDetectorRunner)
        {
            this.azureResourceManagerClient = azureResourceManagerClient;
            this.smartDetectorManifes = smartDetectorManifest;
            this.tracer = tracer;

            this.SmartDetectorRunner = smartDetectorRunner;
            this.SmartDetectorName = this.smartDetectorManifes.Name;
            this.ShouldShowStatusControl = false;

            // Initialize cadences combo box
            IEnumerable<SmartDetectorCadence> cadences = this.smartDetectorManifes.SupportedCadencesInMinutes
                    .Select(cadence => new SmartDetectorCadence(TimeSpan.FromMinutes(cadence)));

            this.Cadences = new ObservableCollection<SmartDetectorCadence>(cadences);

            // Initialize combo boxes read tasks
            this.ReadSubscriptionsTask = new ObservableTask<ObservableCollection<AzureSubscription>>(
                this.GetSubscriptionsAsync());

            this.ReadResourceGroupsTask = new ObservableTask<ObservableCollection<string>>(
                Task.FromResult(new ObservableCollection<string>()));

            this.ReadResourceTypesTask = new ObservableTask<ObservableCollection<string>>(
                Task.FromResult(new ObservableCollection<string>()));

            this.ReadResourcesTask = new ObservableTask<ObservableCollection<ResourceIdentifier>>(
                Task.FromResult(new ObservableCollection<ResourceIdentifier>()));
        }

        #endregion

        #region Binded Properties

        /// <summary>
        /// Gets the Smart Detector name.
        /// </summary>
        public string SmartDetectorName
        {
            get
            {
                return this.smartDetectorName;
            }

            private set
            {
                this.smartDetectorName = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the Smart Detector is running or not.
        /// </summary>
        public bool ShouldShowStatusControl
        {
            get
            {
                return this.shouldShowStatusControl;
            }

            private set
            {
                this.shouldShowStatusControl = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the Smart Detector runner.
        /// </summary>
        public SmartDetectorRunner SmartDetectorRunner
        {
            get
            {
                return this.smartDetectorRunner;
            }

            private set
            {
                this.smartDetectorRunner = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets a task that returns the user's subscriptions.
        /// </summary>
        public ObservableTask<ObservableCollection<AzureSubscription>> ReadSubscriptionsTask
        {
            get
            {
                return this.readSubscriptionsTask;
            }

            private set
            {
                this.readSubscriptionsTask = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the subscription selected by the user.
        /// </summary>
        public AzureSubscription SelectedSubscription
        {
            get
            {
                return this.selectedSubscription;
            }

            set
            {
                this.selectedSubscription = value;
                this.OnPropertyChanged();

                this.ReadResourceGroupsTask = new ObservableTask<ObservableCollection<string>>(
                    this.GetResourceGroupsAsync());
            }
        }

        /// <summary>
        /// Gets a task that returns the user's resource groups.
        /// </summary>
        public ObservableTask<ObservableCollection<string>> ReadResourceGroupsTask
        {
            get
            {
                return this.readResourceGroupsTask;
            }

            private set
            {
                this.readResourceGroupsTask = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the resource group selected by the user.
        /// </summary>
        public string SelectedResourceGroup
        {
            get
            {
                return this.selectedResourceGroup;
            }

            set
            {
                this.selectedResourceGroup = value;
                this.OnPropertyChanged();

                this.ReadResourceTypesTask = new ObservableTask<ObservableCollection<string>>(
                    this.GetResourceTypesAsync());

                this.ReadResourcesTask = new ObservableTask<ObservableCollection<ResourceIdentifier>>(
                    Task.FromResult(new ObservableCollection<ResourceIdentifier>()));
            }
        }

        /// <summary>
        /// Gets a task that returns the user's resource types.
        /// </summary>
        public ObservableTask<ObservableCollection<string>> ReadResourceTypesTask
        {
            get
            {
                return this.readResourceTypesTask;
            }

            private set
            {
                this.readResourceTypesTask = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the resource type selected by the user.
        /// </summary>
        public string SelectedResourceType
        {
            get
            {
                return this.selectedResourceType;
            }

            set
            {
                this.selectedResourceType = value;
                this.OnPropertyChanged();

                this.ReadResourcesTask = new ObservableTask<ObservableCollection<ResourceIdentifier>>(
                    this.GetResourcesAsync());
            }
        }

        /// <summary>
        /// Gets a task that returns the user's resource types.
        /// </summary>
        public ObservableTask<ObservableCollection<ResourceIdentifier>> ReadResourcesTask
        {
            get
            {
                return this.readResourcesTask;
            }

            private set
            {
                this.readResourcesTask = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the resource selected by the user.
        /// </summary>
        public ResourceIdentifier SelectedResource
        {
            get
            {
                return this.selectedResource;
            }

            set
            {
                this.selectedResource = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets a task that returns the user's subscriptions.
        /// </summary>
        public ObservableCollection<SmartDetectorCadence> Cadences
        {
            get
            {
                return this.cadences;
            }

            private set
            {
                this.cadences = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the subscription selected by the user.
        /// </summary>
        public SmartDetectorCadence SelectedCadence
        {
            get
            {
                return this.selectedCadence;
            }

            set
            {
                this.selectedCadence = value;
                this.OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Gets the command that runs the Smart Detector.
        /// </summary>
        public CommandHandler RunSmartDetectorCommand => new CommandHandler(this.RunSmartDetectorAsync);

        #endregion

        /// <summary>
        /// Gets Azure subscriptions.
        /// </summary>
        /// <returns>A task that returns the subscriptions</returns>
        private async Task<ObservableCollection<AzureSubscription>> GetSubscriptionsAsync()
        {
            var subscriptionsList = (await this.azureResourceManagerClient.GetAllSubscriptionsAsync())
                .OrderBy(subscription => subscription.DisplayName)
                .ToList();

            return new ObservableCollection<AzureSubscription>(subscriptionsList);
        }

        /// <summary>
        /// Gets Azure resource groups.
        /// </summary>
        /// <returns>A task that returns the resource groups</returns>
        private async Task<ObservableCollection<string>> GetResourceGroupsAsync()
        {
            var resourceGroups = (await this.azureResourceManagerClient.GetAllResourceGroupsInSubscriptionAsync(this.SelectedSubscription.Id, CancellationToken.None)).ToList()
                .Select(ri => ri.ResourceGroupName)
                .OrderBy(resourceGroup => resourceGroup)
                .ToList();

            return new ObservableCollection<string>(resourceGroups);
        }

        /// <summary>
        /// Gets Azure resource types.
        /// </summary>
        /// <returns>A task that returns the resource types</returns>
        private async Task<ObservableCollection<string>> GetResourceTypesAsync()
        {
            var supportedResourceTypes = new List<ResourceType>() { ResourceType.ApplicationInsights, ResourceType.LogAnalytics, ResourceType.VirtualMachine, ResourceType.VirtualMachineScaleSet, ResourceType.AzureStorage };
            var groups = (await this.azureResourceManagerClient.GetAllResourcesInResourceGroupAsync(this.SelectedSubscription.Id, this.SelectedResourceGroup, supportedResourceTypes, CancellationToken.None)).ToList()
                .GroupBy(resourceIndentifier => resourceIndentifier.ResourceType)
                .Select(group => group.Key.ToString())
                .OrderBy(resourceType => resourceType)
                .ToList();

            return new ObservableCollection<string>(groups);
        }

        /// <summary>
        /// Gets Azure resources.
        /// </summary>
        /// <returns>A task that returns the resources</returns>
        private async Task<ObservableCollection<ResourceIdentifier>> GetResourcesAsync()
        {
            ResourceType selectedResourceType = (ResourceType)Enum.Parse(typeof(ResourceType), this.SelectedResourceType);
            var resources = (await this.azureResourceManagerClient.GetAllResourcesInResourceGroupAsync(
                    this.SelectedSubscription.Id,
                    this.SelectedResourceGroup,
                    new List<ResourceType>() { selectedResourceType },
                    CancellationToken.None)).ToList()
                .Where(resourceIndentifier => resourceIndentifier.ResourceType == selectedResourceType)
                .OrderBy(resource => resource.ResourceName)
                .ToList();

            return new ObservableCollection<ResourceIdentifier>(resources);
        }

        /// <summary>
        /// Runs the Smart Detector.
        /// </summary>
        private async void RunSmartDetectorAsync()
        {
            this.ShouldShowStatusControl = true;

            List<ResourceIdentifier> resources = new List<ResourceIdentifier>() { this.selectedResource };
            try
            {
                await this.smartDetectorRunner.RunAsync(resources, this.selectedCadence.TimeSpan);
            }
            catch (Exception e)
            {
                this.tracer.TraceError($"Failed running Smart Detector: {e.Message}");
            }
        }
    }
}
