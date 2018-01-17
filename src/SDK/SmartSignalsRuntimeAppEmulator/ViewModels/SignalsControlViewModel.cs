﻿//-----------------------------------------------------------------------
// <copyright file="SignalsControlViewModel.cs" company="Microsoft Corporation">
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
    using Microsoft.Azure.Monitoring.SmartSignals.Emulator.Controls;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.AzureResourceManagerClient;
    using Unity.Attributes;

    /// <summary>
    /// An <see cref="ObservableObject"/> which encapsulates an asynchronous task.
    /// </summary>
    public class SignalsControlViewModel : ObservableObject
    {
        private readonly AzureResourceManagerClient azureResourceManagerClient;

        private ObservableTask<ObservableCollection<AzureSubscription>> readSubscriptionsTask;

        private AzureSubscription selectedSubscription;

        private ObservableTask<ObservableCollection<string>> readResourceGroupsTask;

        private string selectedResourceGroup;

        private ObservableTask<ObservableCollection<string>> readResourceTypesTask;

        private string selectedResourceType;

        private ObservableTask<ObservableCollection<string>> readResourcesTask;

        private string selectedResource;

        #region Ctros

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalsControlViewModel"/> class for design time only.
        /// </summary>
        public SignalsControlViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalsControlViewModel"/> class.
        /// </summary>
        /// <param name="azureResourceManagerClient">The Azure resources manager client</param>
        [InjectionConstructor]
        public SignalsControlViewModel(AzureResourceManagerClient azureResourceManagerClient)
        {
            this.azureResourceManagerClient = azureResourceManagerClient;

            this.ReadSubscriptionsTask = new ObservableTask<ObservableCollection<AzureSubscription>>(
                this.GetSubscriptionsAsync());

            this.ReadResourceGroupsTask = new ObservableTask<ObservableCollection<string>>(
                Task.FromResult(new ObservableCollection<string>()));

            this.ReadResourceTypesTask = new ObservableTask<ObservableCollection<string>>(
                Task.FromResult(new ObservableCollection<string>()));

            this.ReadResourcesTask = new ObservableTask<ObservableCollection<string>>(
                Task.FromResult(new ObservableCollection<string>()));
        }

        #endregion

        #region Binded Properties

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

                this.ReadResourcesTask = new ObservableTask<ObservableCollection<string>>(
                    Task.FromResult(new ObservableCollection<string>()));
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

                this.ReadResourcesTask = new ObservableTask<ObservableCollection<string>>(
                    this.GetResourcesAsync());
            }
        }

        /// <summary>
        /// Gets a task that returns the user's resource types.
        /// </summary>
        public ObservableTask<ObservableCollection<string>> ReadResourcesTask
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
        public string SelectedResource
        {
            get
            {
                return this.selectedResourceType;
            }

            set
            {
                this.selectedResource = value;
                this.OnPropertyChanged();
            }
        }

        #endregion

        /// <summary>
        /// Gets Azure subscriptions.
        /// </summary>
        /// <returns>A task that returns the subscriptions</returns>
        private async Task<ObservableCollection<AzureSubscription>> GetSubscriptionsAsync()
        {
            var subscriptionsList = (await this.azureResourceManagerClient.GetAllSubscriptionsAsync()).ToList();

            return new ObservableCollection<AzureSubscription>(subscriptionsList);
        }

        /// <summary>
        /// Gets Azure resource groups.
        /// </summary>
        /// <returns>A task that returns the resource groups</returns>
        private async Task<ObservableCollection<string>> GetResourceGroupsAsync()
        {
            var resourceGroups = (await this.azureResourceManagerClient.GetAllResourceGroupsInSubscriptionAsync(this.SelectedSubscription.Id, CancellationToken.None)).ToList()
                .Select(ri => ri.ResourceGroupName).ToList();

            return new ObservableCollection<string>(resourceGroups);
        }

        /// <summary>
        /// Gets Azure resource types.
        /// </summary>
        /// <returns>A task that returns the resource types</returns>
        private async Task<ObservableCollection<string>> GetResourceTypesAsync()
        {
            var supportedResourceTypes = new List<ResourceType>() { ResourceType.ApplicationInsights, ResourceType.LogAnalytics, ResourceType.VirtualMachine };
            var groups = (await this.azureResourceManagerClient.GetAllResourcesInResourceGroupAsync(this.SelectedSubscription.Id, this.SelectedResourceGroup, supportedResourceTypes, CancellationToken.None)).ToList()
                .GroupBy(resourceIndentifier => resourceIndentifier.ResourceType)
                .Select(group => group.Key.ToString()).ToList();

            return new ObservableCollection<string>(groups);
        }

        /// <summary>
        /// Gets Azure resources.
        /// </summary>
        /// <returns>A task that returns the resources</returns>
        private async Task<ObservableCollection<string>> GetResourcesAsync()
        {
            ResourceType selectedResourceType = (ResourceType)Enum.Parse(typeof(ResourceType), this.SelectedResourceType);
            var resources = (await this.azureResourceManagerClient.GetAllResourcesInResourceGroupAsync(
                    this.SelectedSubscription.Id,
                    this.SelectedResourceGroup,
                    new List<ResourceType>() { selectedResourceType },
                    CancellationToken.None)).ToList()
                .Where(resourceIndentifier => resourceIndentifier.ResourceType == selectedResourceType)
                .Select(resourceIndentifier => resourceIndentifier.ResourceName).ToList();

            return new ObservableCollection<string>(resources);
        }
    }
}
