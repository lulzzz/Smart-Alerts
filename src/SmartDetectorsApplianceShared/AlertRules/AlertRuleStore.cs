﻿//-----------------------------------------------------------------------
// <copyright file="AlertRuleStore.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.AlertRules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.AzureStorage;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Exceptions;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using Newtonsoft.Json;

    /// <summary>
    /// Implementation of the <see cref="IAlertRuleStore"/> using Azure table.
    /// </summary>
    public class AlertRuleStore : IAlertRuleStore
    {
        private const string TableName = "alertrules";
        private const string PartitionKey = "rules";

        private readonly ICloudTableWrapper alertRulesTable;
        private readonly ITracer tracer;

        /// <summary>
        /// Initializes a new instance of the<see cref="AlertRuleStore"/> class.
        /// </summary>
        /// <param name="storageProviderFactory">The Azure Storage provider factory</param>
        /// <param name="tracer">Log wrapper</param>
        public AlertRuleStore(ICloudStorageProviderFactory storageProviderFactory, ITracer tracer)
        {
            this.tracer = tracer;

            // create the cloud table instance
            ICloudTableClientWrapper tableClient = storageProviderFactory.GetSmartDetectorStorageTableClient();
            this.alertRulesTable = tableClient.GetTableReference(TableName);
            this.alertRulesTable.CreateIfNotExists();
        }

        /// <summary>
        /// Gets all the alert rules from the store.
        /// </summary>
        /// <returns>A <see cref="IList{AlertRule}"/> containing all the alert rules in the store.</returns>
        public async Task<IList<AlertRule>> GetAllAlertRulesAsync()
        {
            try
            {
                this.tracer.TraceInformation("Getting all Smart Detector alert rules");
                var alertRulesEntities = await this.alertRulesTable.ReadPartitionAsync<AlertRuleEntity>(PartitionKey);
                this.tracer.TraceInformation($"Found {alertRulesEntities.Count} alert rules");

                this.tracer.TraceVerbose($"Found alert rules: {string.Join(", ", alertRulesEntities.Select(e => e.RowKey))}");

                return alertRulesEntities.Select(entity => new AlertRule
                {
                    Id = entity.RowKey,
                    Name = entity.Name,
                    Description = entity.Description,
                    SmartDetectorId = entity.SmartDetectorId,
                    ResourceId = entity.ResourceId,
                    Cadence = TimeSpan.FromMinutes(entity.CadenceInMinutes),
                    EmailRecipients = entity.EmailRecipients == null ? null : JsonConvert.DeserializeObject<List<string>>(entity.EmailRecipients)
                }).ToList();
            }
            catch (StorageException e)
            {
                throw new AlertRuleStoreException("Failed to get alert rules", e);
            }
        }

        /// <summary>
        /// Adds or updates an alert rule in the store.
        /// </summary>
        /// <param name="alertRule">The alert rule to add to the store.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> object that represents the asynchronous operation.</returns>
        public async Task AddOrReplaceAlertRuleAsync(AlertRule alertRule, CancellationToken cancellationToken)
        {
            try
            {
                // Execute the update operation
                this.tracer.TraceInformation($"updating alert rule: {alertRule.Id} for Smart Detector: {alertRule.SmartDetectorId}");
                var operation = TableOperation.InsertOrReplace(new AlertRuleEntity
                {
                    PartitionKey = PartitionKey,
                    RowKey = alertRule.Id,
                    Name = alertRule.Name,
                    Description = alertRule.Description,
                    SmartDetectorId = alertRule.SmartDetectorId,
                    ResourceId = alertRule.ResourceId,
                    CadenceInMinutes = (int)alertRule.Cadence.TotalMinutes,
                    EmailRecipients = alertRule.EmailRecipients == null ? null : JsonConvert.SerializeObject(alertRule.EmailRecipients)
                });

                await this.alertRulesTable.ExecuteAsync(operation, cancellationToken);

                this.tracer.TraceInformation($"updated alert rule: {alertRule.Id}");
            }
            catch (StorageException e)
            {
                throw new AlertRuleStoreException("Failed to add/replace alert rule", e);
            }
        }
    }
}
