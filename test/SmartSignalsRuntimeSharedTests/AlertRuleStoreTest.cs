//-----------------------------------------------------------------------
// <copyright file="AlertRuleStoreTest.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartSignalsRuntimeSharedTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.AlertRules;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.AzureStorage;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Storage.Table;
    using Moq;

    [TestClass]
    public class AlertRuleStoreTest
    {
        private AlertRuleStore alertRuleStore;
        private Mock<ICloudTableWrapper> tableMock;

        [TestInitialize]
        public void Setup()
        {
            this.tableMock = new Mock<ICloudTableWrapper>();
            var tableClientMock = new Mock<ICloudTableClientWrapper>();
            tableClientMock.Setup(m => m.GetTableReference(It.IsAny<string>())).Returns(this.tableMock.Object);
            var storageProviderFactoryMock = new Mock<ICloudStorageProviderFactory>();
            storageProviderFactoryMock.Setup(m => m.GetSmartDetectorStorageTableClient()).Returns(tableClientMock.Object);

            var tracerMock = new Mock<ITracer>();
            this.alertRuleStore = new AlertRuleStore(storageProviderFactoryMock.Object, tracerMock.Object);
        }

        [TestMethod]
        public async Task WhenUpdatingAlertRuleThenUpdateIsCalledCorrectly()
        {
            var ruleToUpdate = new AlertRule
            {
                Id = "ruleId",
                SmartDetectorId = "smartDetectorId",
                Cadence = TimeSpan.FromMinutes(1440),
                ResourceId = "resourceId"
            };

            await this.alertRuleStore.AddOrReplaceAlertRuleAsync(ruleToUpdate, CancellationToken.None);

            this.tableMock.Verify(m => m.ExecuteAsync(
                It.Is<TableOperation>(operation =>
                    operation.OperationType == TableOperationType.InsertOrReplace &&
                    operation.Entity.RowKey.Equals(ruleToUpdate.Id) &&
                    ((AlertRuleEntity)operation.Entity).SmartDetectorId.Equals(ruleToUpdate.SmartDetectorId) &&
                    ((AlertRuleEntity)operation.Entity).CadenceInMinutes.Equals(1440) &&
                    ((AlertRuleEntity)operation.Entity).ResourceId.Equals(ruleToUpdate.ResourceId)),
                It.IsAny<CancellationToken>()));
        }

        [TestMethod]
        public async Task WhenGettingAllAlertRulesThenTableIsCalledCorrectly()
        {
            // Create rules entites in the table
            var ruleEntities = new List<AlertRuleEntity>
            {
                new AlertRuleEntity
                {
                    RowKey = "rule1",
                    SmartDetectorId = "smartDetector1",
                    CadenceInMinutes = 1440,
                    ResourceId = "resourceId1"
                },
                new AlertRuleEntity
                {
                    RowKey = "rule2",
                    SmartDetectorId = "smartDetector2",
                    CadenceInMinutes = 60,
                    ResourceId = "resourceId2"
                }
            };

            this.tableMock.Setup(m => m.ReadPartitionAsync<AlertRuleEntity>("rules")).ReturnsAsync(ruleEntities);

            var returnedRules = await this.alertRuleStore.GetAllAlertRulesAsync();
            Assert.AreEqual(2, returnedRules.Count);

            var firstRule = returnedRules.First();
            Assert.AreEqual("rule1", firstRule.Id);
            Assert.AreEqual("smartDetector1", firstRule.SmartDetectorId);
            Assert.AreEqual(1440, firstRule.Cadence.TotalMinutes);
            Assert.AreEqual("resourceId1", firstRule.ResourceId);

            var lastRule = returnedRules.Last();
            Assert.AreEqual("rule2", lastRule.Id);
            Assert.AreEqual("smartDetector2", lastRule.SmartDetectorId);
            Assert.AreEqual(60, lastRule.Cadence.TotalMinutes);
            Assert.AreEqual("resourceId2", lastRule.ResourceId);
        }
    }
}