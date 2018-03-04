//-----------------------------------------------------------------------
// <copyright file="MdmClientTests.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartSignalsSharedTests
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Microsoft.Azure.Monitoring.SmartSignals.Clients;
    using Microsoft.Azure.Monitoring.SmartSignals.Emulator.Models;
    using Microsoft.Azure.Monitoring.SmartSignals.Mdm;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class MdmClientTests
    {
        private readonly Mock<ITracer> tracerMock = new Mock<ITracer>();
        private readonly Mock<ICredentialsFactory> credentialsFactoryMock = new Mock<ICredentialsFactory>();

        [Ignore]
        [TestMethod]
        public void WhenSendingQueryToMdmThenTheResultsAreAsExpected()
        {
            //// var client = new MdmClient(this.credentialsFactoryMock.Object, this.tracerMock.Object, "1c4ea252-89fc-447a-ba67-9c4d5b2270a1");
            var authenticationServices = new AuthenticationServices();
            authenticationServices.AuthenticateUser();
            ICredentialsFactory credentialsFactory = new ActiveDirectoryCredentialsFactory(authenticationServices);
            var resouceIdentifier = new ResourceIdentifier(
                ResourceType.AzureStorage, 
                subscriptionId: "SUBSCRIPTION_ID", 
                resourceGroupName: "RESOURCE_GROUP_NAME", 
                resourceName: "STORAGE_NAME");

            var resourceId = $"/subscriptions/{resouceIdentifier.SubscriptionId}/resourceGroups/{resouceIdentifier.ResourceGroupName}/providers/Microsoft.Storage/storageAccounts/{resouceIdentifier.ResourceName}/queueServices/default";
            MdmClient client = new MdmClient(this.tracerMock.Object, credentialsFactory, resouceIdentifier);

            var parameters = new QueryParameters()
            {
                StartTime = DateTime.UtcNow.Date.AddDays(-1),
                EndTime = DateTime.UtcNow.Date,
                Aggregations = new List<AggregationType> { AggregationType.Total },
                MetricName = "QueueMessageCount",
                Interval = TimeSpan.FromMinutes(60)
            };

            var metrics1 = client.GetResourceMetrics(resourceId, parameters).Result;
            var metrics2 = client.GetResourceMetrics(ServiceType.AzureStorageQueue, parameters).Result;
        }
    }
}
