//-----------------------------------------------------------------------
// <copyright file="AlertRuleApi.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.EndpointsLogic
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors.Tools;
    using Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.Models;
    using Microsoft.Azure.Monitoring.SmartSignals.RuntimeShared.AlertRules;
    using Microsoft.Azure.Monitoring.SmartSignals.RuntimeShared.Exceptions;

    /// <summary>
    /// This class is the logic for the /alertRule endpoint.
    /// </summary>
    public class AlertRuleApi : IAlertRuleApi
    {
        private readonly IAlertRuleStore alertRuleStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="AlertRuleApi"/> class.
        /// </summary>
        /// <param name="alertRuleStore">The alert rules store.</param>
        public AlertRuleApi(IAlertRuleStore alertRuleStore)
        {
            Diagnostics.EnsureArgumentNotNull(() => alertRuleStore);

            this.alertRuleStore = alertRuleStore;
        }

        /// <summary>
        /// Add the given alert rule to the alert rules store.
        /// </summary>
        /// <returns>A task represents this operation.</returns>
        /// <param name="addAlertRule">The model that contains all the require parameters for adding alert rule.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="SmartSignalsManagementApiException">This exception is thrown when we failed to add the alert rule.</exception>
        public async Task AddAlertRuleAsync(AlertRuleApiEntity addAlertRule, CancellationToken cancellationToken)
        {
            Diagnostics.EnsureArgumentNotNull(() => addAlertRule);

            // Verify given input model is valid
            if (!IsAddAlertRuleModelValid(addAlertRule, out var validationError))
            {
                throw new SmartSignalsManagementApiException(validationError, HttpStatusCode.BadRequest);
            }

            try
            {
                await this.alertRuleStore.AddOrReplaceAlertRuleAsync(
                    new AlertRule
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = addAlertRule.Name,
                        Description = addAlertRule.Description,
                        SignalId = addAlertRule.SignalId,
                        ResourceId = addAlertRule.ResourceId,
                        Cadence = TimeSpan.FromMinutes(addAlertRule.CadenceInMinutes),
                        EmailRecipients = addAlertRule.EmailRecipients
                    },
                    cancellationToken);
            }
            catch (AlertRuleStoreException e)
            {
                throw new SmartSignalsManagementApiException("Failed to add the given alert rule", e, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get the alert rules from the alert rules store.
        /// </summary>
        /// <returns>The alert rules list.</returns>
        /// <exception cref="SmartSignalsManagementApiException">This exception is thrown when we failed to get the alert rules.</exception>
        public async Task<IList<AlertRule>> GetAlertRulesAsync()
        {
            try
            {
                return await this.alertRuleStore.GetAllAlertRulesAsync();
            }
            catch (AlertRuleStoreException e)
            {
                throw new SmartSignalsManagementApiException("Failed to get alert rules", e, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Validates if the given model for adding alert rule is valid.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="errorInformation">The error information which will be filled in case validation will fail.</param>
        /// <returns>True in case model is valid, else false.</returns>
        private static bool IsAddAlertRuleModelValid(AlertRuleApiEntity model, out string errorInformation)
        {
            if (string.IsNullOrWhiteSpace(model.SignalId))
            {
                errorInformation = "Signal ID can't be empty";
                return false;
            }

            if (model.CadenceInMinutes <= 0)
            {
                errorInformation = "CadenceInMinutes parameter must be a positive integer";
                return false;
            }

            errorInformation = string.Empty;
            return true;
        }
    }
}
