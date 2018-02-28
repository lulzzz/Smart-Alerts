//-----------------------------------------------------------------------
// <copyright file="MdmClientTests.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartSignalsSharedTests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Management.Monitor.Fluent.Models;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Microsoft.Azure.Monitoring.SmartSignals.Clients;
    using Microsoft.Azure.Monitoring.SmartSignals.Emulator.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

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
                subscriptionId: "1c4ea252-89fc-447a-ba67-9c4d5b2270a1", 
                resourceGroupName: "AI-MON-RG", 
                resourceName: "draftprodexportgeneva");

            var resourceId = $"/subscriptions/{resouceIdentifier.SubscriptionId}/resourceGroups/{resouceIdentifier.ResourceGroupName}/providers/Microsoft.Storage/storageAccounts/{resouceIdentifier.ResourceName}/queueServices/default";
            MdmClient client = new MdmClient(this.tracerMock.Object, credentialsFactory, resouceIdentifier);
            var definitions1 = client.GetMetricDefinitions(resourceId).Result;
            var definitions2 = client.GetMetricDefinitions(AzureResourceService.AzureStorageQueue).Result;

            var parameters = new MdmQueryParameters()
            {
                StartTime = DateTime.UtcNow.Date.AddDays(-1),
                EndTime = DateTime.UtcNow.Date,
                Aggregation = AggregationType.Total,
                MetricName = "QueueMessageCount",
                ResultType = ResultType.Data,
                Interval = TimeSpan.FromMinutes(60)
            };

            var metrics1 = client.GetResourceMetrics(resourceId, parameters).Result;
            var metrics2 = client.GetResourceMetrics(AzureResourceService.AzureStorageQueue, parameters).Result;
        }
    }
}
