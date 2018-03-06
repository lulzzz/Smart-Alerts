//-----------------------------------------------------------------------
// <copyright file="EmailSender.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Scheduler.Publisher
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Presentation;
    using Microsoft.Azure.Monitoring.SmartDetectors.Tools;
    using Microsoft.Azure.Monitoring.SmartSignals.RuntimeShared.AlertRules;
    using Microsoft.Azure.Monitoring.SmartSignals.Scheduler.Exceptions;
    using SendGrid;
    using SendGrid.Helpers.Mail;
    using Unity.Attributes;

    /// <summary>
    /// This class is responsible for sending Smart Signal results Email
    /// </summary>
    public class EmailSender : IEmailSender
    {
        internal const string SignalNamePlaceHolder = "[signalName]";
        internal const string ResourceNamePlaceHolder = "[resourceName]";
        internal const string LinkToPortalPlaceHolder = "[linkToPortal]";
        internal const string RuleNamePlaceHolder = "[ruleName]";
        internal const string RuleDescriptionPlaceHolder = "[ruleDescription]";
        internal const string ServiceNamePlaceHolder = "[serviceName]";
        internal const string AlertActivatedTimePlaceHolder = "[alertActivatedTime]";
        internal const string SubscriptionNamePlaceHolder = "[subscriptionName]";
        internal const string LinkToFeedbackPlaceHolder = "[linkToFeedback]";

        private ITracer tracer;
        private ISendGridClient sendGridClient;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailSender"/> class.
        /// </summary>
        /// <param name="tracer">The tracer to use.</param>
        [InjectionConstructor]
        public EmailSender(ITracer tracer)
        {
            this.tracer = Diagnostics.EnsureArgumentNotNull(() => tracer);

            var apiKey = ConfigurationReader.ReadConfig("SendgridApiKey", false);
            this.sendGridClient = string.IsNullOrWhiteSpace(apiKey) ? null : new SendGridClient(apiKey);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailSender"/> class.
        /// </summary>
        /// <param name="tracer">The tracer to use.</param>
        /// <param name="sendGridClient">The send grid client.</param>
        public EmailSender(ITracer tracer, ISendGridClient sendGridClient)
        {
            this.tracer = Diagnostics.EnsureArgumentNotNull(() => tracer);
            this.sendGridClient = sendGridClient;
        }

        /// <summary>
        /// Sends the Smart Signal result Email.
        /// </summary>
        /// <param name="signalExecution">The signals execution information.</param>
        /// <param name="alerts">The Alerts.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task SendSignalResultEmailAsync(SignalExecutionInfo signalExecution, IList<AlertPresentation> alerts)
        {
            AlertRule alertRule = signalExecution.AlertRule;
            if (this.sendGridClient == null)
            {
                this.tracer.TraceWarning("SendGrid API key was not found, not sending email");
                return;
            }

            if (alertRule.EmailRecipients == null || !alertRule.EmailRecipients.Any())
            {
                this.tracer.TraceWarning("Email recipients were not provided, not sending email");
                return;
            }

            if (alerts == null || !alerts.Any())
            {
                this.tracer.TraceInformation($"no result items to publish for signal {alertRule.SignalId}");
                return;
            }

            this.tracer.TraceInformation($"Sending signal result email for signal {alertRule.SignalId}");

            var exceptions = new List<Exception>();

            foreach (AlertPresentation alert in alerts)
            {
                ResourceIdentifier resource = ResourceIdentifier.CreateFromResourceId(alertRule.ResourceId);

                // TODO: Fix links
                string emailBody = Resources.SmartSignalEmailTemplate
                    .Replace(SignalNamePlaceHolder, alert.SmartDetectorName)
                    .Replace(ResourceNamePlaceHolder, alert.ResourceId)
                    .Replace(LinkToPortalPlaceHolder, "LinkToPortal")
                    .Replace(RuleNamePlaceHolder, alertRule.Name) 
                    .Replace(RuleDescriptionPlaceHolder, alertRule.Description)
                    .Replace(ServiceNamePlaceHolder, $@"{resource.ResourceType}: {resource.ResourceName} ({resource.ResourceGroupName})")
                    .Replace(AlertActivatedTimePlaceHolder, signalExecution.LastExecutionTime.ToString())
                    .Replace(SubscriptionNamePlaceHolder, resource.SubscriptionId)
                    .Replace(LinkToFeedbackPlaceHolder, "https://ms.portal.azure.com/");

                var msg = new SendGridMessage
                {
                    From = new EmailAddress("smartsignals@microsoft.com", "Smart Signals"),
                    Subject = $"Azure Smart Alerts (preview) - {alert.SmartDetectorName} detected",
                    PlainTextContent = $@"{alert.SmartDetectorName} was detected for {alert.ResourceId}. 
                                        You can view more details for this alert here: {"LinkToPortal"}",
                    HtmlContent = emailBody
                };

                var emailAddresses = alertRule.EmailRecipients.Select(email => new EmailAddress(email)).ToList();
                msg.AddTos(emailAddresses);

                try
                {
                    var response = await this.sendGridClient.SendEmailAsync(msg);
                    if (!IsSuccessStatusCode(response.StatusCode))
                    {
                        string content = response.Body != null ? await response.Body.ReadAsStringAsync() : string.Empty;
                        var message = $"Failed to send signal results Email for signal {alertRule.SignalId}. Fail StatusCode: {response.StatusCode}. Content: {content}.";
                        this.tracer.TraceError(message);
                        exceptions.Add(new EmailSendingException(message));
                    }
                }
                catch (Exception e)
                {
                    this.tracer.TraceError($"Failed to send email. Exception: {e}");
                    exceptions.Add(new EmailSendingException($"Exception was thrown fo sending signal results Email for signal {alertRule.SignalId}. Exception: {e}"));
                }
            }

            if (exceptions.Count > 0)
            {
                this.tracer.TraceError(
                    $"Failed to send one or more signal result emails." +
                    $"Number of exceptions thrown: {exceptions.Count()}.");
                throw new AggregateException("Failed to send one or more signal result emails", exceptions);
            }

            this.tracer.TraceInformation($"Sent signal result emails successfully for signal {alertRule.SignalId}");
        }

        /// <summary>
        /// Checks if the status code is a success status code
        /// </summary>
        /// <param name="statusCode">The status code</param>
        /// <returns>True if the status code is a success status code, false otherwise</returns>
        private static bool IsSuccessStatusCode(HttpStatusCode statusCode)
        {
            return (int)statusCode >= 200 && (int)statusCode <= 299;
        }
    }
}
