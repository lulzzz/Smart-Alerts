//-----------------------------------------------------------------------
// <copyright file="SignalConfigurationControlViewModel.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Emulator.ViewModels
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
    using Microsoft.Azure.Monitoring.SmartDetectors.Package;
    using Microsoft.Azure.Monitoring.SmartSignals.Emulator.Controls;
    using Microsoft.Azure.Monitoring.SmartSignals.Emulator.Models;
    using Unity.Attributes;

    /// <summary>
    /// The view model class for the <see cref="SignalConfigurationControl"/> control.
    /// </summary>
    public class SignalConfigurationControlViewModel : ObservableObject
    {
        private readonly IAzureResourceManagerClient azureResourceManagerClient;

        private readonly SmartDetectorManifest smartDetectorManifes;

        private readonly ITracer tracer;

        private SmartSignalRunner signalRunner;

        private string signalName;

        private ObservableCollection<SignalCadence> cadences;

        private SignalCadence selectedCadence;

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
        /// Initializes a new instance of the <see cref="SignalConfigurationControlViewModel"/> class for design time only.
        /// </summary>
        public SignalConfigurationControlViewModel()
        {
            this.ShouldShowStatusControl = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalConfigurationControlViewModel"/> class.
        /// </summary>
        /// <param name="azureResourceManagerClient">The Azure resources manager client.</param>
        /// <param name="tracer">The tracer.</param>
        /// <param name="smartDetectorManifest">The smart detector manifest.</param>
        /// <param name="signalRunner">The smart signal runner.</param>
        [InjectionConstructor]
        public SignalConfigurationControlViewModel(
            IAzureResourceManagerClient azureResourceManagerClient,
            ITracer tracer,
            SmartDetectorManifest smartDetectorManifest,
            SmartSignalRunner signalRunner)
        {
            this.azureResourceManagerClient = azureResourceManagerClient;
            this.smartDetectorManifes = smartDetectorManifest;
            this.tracer = tracer;

            this.SignalRunner = signalRunner;
            this.SignalName = this.smartDetectorManifes.Name;
            this.ShouldShowStatusControl = false;

            // Initialize cadences combo box
            IEnumerable<SignalCadence> cadences = this.smartDetectorManifes.SupportedCadencesInMinutes
                    .Select(cadence => new SignalCadence(TimeSpan.FromMinutes(cadence)));

            this.Cadences = new ObservableCollection<SignalCadence>(cadences);

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
        /// Gets the signal name.
        /// </summary>
        public string SignalName
        {
            get
            {
                return this.signalName;
            }

            private set
            {
                this.signalName = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the signal is running or not.
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
        /// Gets the signal runner.
        /// </summary>
        public SmartSignalRunner SignalRunner
        {
            get
            {
                return this.signalRunner;
            }

            private set
            {
                this.signalRunner = value;
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
        public ObservableCollection<SignalCadence> Cadences
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
        public SignalCadence SelectedCadence
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
        /// Gets the command that runs the signal.
        /// </summary>
        public CommandHandler RunSignalCommand => new CommandHandler(this.RunSignalAsync);

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
            var supportedResourceTypes = new List<ResourceType>() { ResourceType.ApplicationInsights, ResourceType.LogAnalytics, ResourceType.VirtualMachine, ResourceType.VirtualMachineScaleSet };
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
        /// Runs the smart signal.
        /// </summary>
        private async void RunSignalAsync()
        {
            this.ShouldShowStatusControl = true;

            List<ResourceIdentifier> resources = new List<ResourceIdentifier>() { this.selectedResource };
            try
            {
                await this.signalRunner.RunAsync(resources, this.selectedCadence.TimeSpan);
            }
            catch (Exception e)
            {
                this.tracer.TraceError($"Failed running signal: {e.Message}");
            }
        }
    }
}
