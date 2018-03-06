//-----------------------------------------------------------------------
// <copyright file="IAzureResourceManagerClient.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Clients
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// An interface for azure resource manager client
    /// </summary>
    public interface IAzureResourceManagerClient
    {
        /// <summary>
        /// Enumerates all the resource groups in the specified subscription.
        /// </summary>
        /// <param name="subscriptionId">The subscription ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the resource groups.</returns>
        Task<IList<ResourceIdentifier>> GetAllResourceGroupsInSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken);

        /// <summary>
        /// Enumerates all the resources of the specified types in the specified subscription.
        /// </summary>
        /// <param name="subscriptionId">The subscription ID.</param>
        /// <param name="resourceTypes">The types of resource to enumerate.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the resource identifiers.</returns>
        Task<IList<ResourceIdentifier>> GetAllResourcesInSubscriptionAsync(string subscriptionId, IEnumerable<ResourceType> resourceTypes, CancellationToken cancellationToken);

        /// <summary>
        /// Enumerates all the resources in the specified resource group.
        /// </summary>
        /// <param name="subscriptionId">The subscription ID.</param>
        /// <param name="resourceGroupName">The resource group name.</param>
        /// <param name="resourceTypes">The types of resource to enumerate.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the resource identifiers.</returns>
        Task<IList<ResourceIdentifier>> GetAllResourcesInResourceGroupAsync(string subscriptionId, string resourceGroupName, IEnumerable<ResourceType> resourceTypes, CancellationToken cancellationToken);

        /// <summary>
        /// Enumerates all the accessible subscriptions.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the subscription IDs</returns>
        Task<IList<string>> GetAllSubscriptionIdsAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Returns the resource properties, as a <see cref="JObject"/> instance.
        /// </summary>
        /// <param name="resourceIdentifier">The resource identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the resource properties.</returns>
        Task<JObject> GetResourcePropertiesAsync(ResourceIdentifier resourceIdentifier, CancellationToken cancellationToken);

        /// <summary>
        /// Returns the Application Insights app ID.
        /// </summary>
        /// <param name="resourceIdentifier">
        /// The Application Insights resource identifier.
        /// The value of the <see cref="ResourceIdentifier.ResourceType"/> property must be equal to <see cref="ResourceType.ApplicationInsights"/>
        /// </param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the app ID.</returns>
        Task<string> GetApplicationInsightsAppIdAsync(ResourceIdentifier resourceIdentifier, CancellationToken cancellationToken);

        /// <summary>
        /// Enumerates all the accessible subscriptions.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the subscription IDs</returns>
        Task<IList<AzureSubscription>> GetAllSubscriptionsAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Returns the log analytics workspace ID.
        /// </summary>
        /// <param name="resourceIdentifier">
        /// The log analytics resource identifier.
        /// The value of the <see cref="ResourceIdentifier.ResourceType"/> property must be equal to <see cref="ResourceType.LogAnalytics"/>
        /// </param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the workspace ID.</returns>
        Task<string> GetLogAnalyticsWorkspaceIdAsync(ResourceIdentifier resourceIdentifier, CancellationToken cancellationToken);
    }
}