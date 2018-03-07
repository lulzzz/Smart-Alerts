//-----------------------------------------------------------------------
// <copyright file="StringTracer.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Emulator.Models
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using Microsoft.Azure.Monitoring.SmartDetectors;

    /// <summary>
    /// Implementation of <see cref="ITracer"/> that saves all traces in a string.
    /// </summary>
    public class StringTracer : ObservableObject, ITracer
    {
        private StringBuilder traceStringBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringTracer"/> class.
        /// </summary>
        /// <param name="sessionId">Session id used for tracing</param>
        public StringTracer(string sessionId)
        {
            this.SessionId = sessionId;
            this.traceStringBuilder = new StringBuilder();
        }

        /// <summary>
        /// Gets all traces in a single string.
        /// </summary>
        public StringBuilder Traces
        {
            get
            {
                return this.traceStringBuilder;
            }

            private set
            {
                this.traceStringBuilder = value;
                this.OnPropertyChanged();
            }
        }

        #region Implementation of ITracer

        /// <summary>
        /// Gets the tracer's session ID
        /// </summary>
        public string SessionId { get; }

        /// <summary>
        /// Trace <paramref name="message"/> as Information message.
        /// </summary>
        /// <param name="message">The message to trace</param>
        /// <param name="properties">Named string values you can use to classify and filter traces</param>
        public void TraceInformation(string message, IDictionary<string, string> properties = null)
        {
            this.Trace($"Information: {message}");
        }

        /// <summary>
        /// Trace <paramref name="message"/> as Error message.
        /// </summary>
        /// <param name="message">The message to trace</param>
        /// <param name="properties">Named string values you can use to classify and filter traces</param>
        public void TraceError(string message, IDictionary<string, string> properties = null)
        {
            this.Trace($"Error: {message}");
        }

        /// <summary>
        /// Trace <paramref name="message"/> as Verbose message.
        /// </summary>
        /// <param name="message">The message to trace</param>
        /// <param name="properties">Named string values you can use to classify and filter traces</param>
        public void TraceVerbose(string message, IDictionary<string, string> properties = null)
        {
            this.Trace($"Verbose: {message}");
        }

        /// <summary>
        /// Trace <paramref name="message"/> as Warning message.
        /// </summary>
        /// <param name="message">The message to trace</param>
        /// <param name="properties">Named string values you can use to classify and filter traces</param>
        public void TraceWarning(string message, IDictionary<string, string> properties = null)
        {
            this.Trace($"Warning: {message}");
        }

        /// <summary>
        /// Reports a metric.
        /// </summary>
        /// <param name="name">The metric name</param>
        /// <param name="value">The metric value</param>
        /// <param name="properties">Named string values you can use to classify and filter metrics</param>
        /// <param name="count">The aggregated metric count</param>
        /// <param name="max">The aggregated metric max value</param>
        /// <param name="min">The aggregated metric min name</param>
        /// <param name="timestamp">The timestamp of the aggregated metric</param>
        public void ReportMetric(string name, double value, IDictionary<string, string> properties = null, int? count = null, double? max = null, double? min = null, DateTime? timestamp = null)
        {
            this.Trace($"METRIC: {name}={value}");
        }

        /// <summary>
        /// Reports a runtime exception.
        /// It uses exception and trace entities with same operation id.
        /// </summary>
        /// <param name="exception">The exception to report</param>
        public void ReportException(Exception exception)
        {
            this.Trace($"EXCEPTION: {exception}");
        }

        /// <summary>
        /// Send information about a dependency call.
        /// </summary>
        /// <param name="dependencyName">The dependency name.</param>
        /// <param name="commandName">The command name</param>
        /// <param name="startTime">The dependency call start time</param>
        /// <param name="duration">The time taken to handle the dependency.</param>
        /// <param name="success">Was the dependency call successful</param>
        /// <param name="metrics">Named double values that define additional dependency metrics</param>
        /// <param name="properties">Named string values you can use to classify and filter dependencies</param>
        public void TrackDependency(string dependencyName, string commandName, DateTimeOffset startTime, TimeSpan duration, bool success, IDictionary<string, double> metrics = null, IDictionary<string, string> properties = null)
        {
            this.Trace($"--> name={dependencyName}, command={commandName}, duration={duration}, success={success}");
        }

        /// <summary>
        /// Send information about an event.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="properties">Named string values you can use to classify and filter dependencies</param>
        /// <param name="metrics">Named double values that define additional event metrics</param>
        public void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            this.Trace($"EVENT: name={eventName}");
        }

        /// <summary>
        /// Flushes the telemetry channel
        /// </summary>
        public void Flush()
        {
        }

        /// <summary>
        /// Adds a custom property, to be included in all traces.
        /// </summary>
        /// <param name="name">The property name</param>
        /// <param name="value">The property value</param>
        public void AddCustomProperty(string name, string value)
        {
        }

        #endregion

        /// <summary>
        /// Appends a trace line to the string.
        /// </summary>
        /// <param name="message">The message to trace</param>
        private void Trace(string message)
        {
            string timestamp = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            this.Traces = this.Traces.AppendLine($"[{timestamp}] {message}");
        }
    }
}
