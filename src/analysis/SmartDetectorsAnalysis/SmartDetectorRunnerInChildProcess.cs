﻿//-----------------------------------------------------------------------
// <copyright file="SmartDetectorRunnerInChildProcess.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Analysis
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ChildProcess;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Contracts;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Contracts.Alert;

    /// <summary>
    /// An implementation of <see cref="ISmartDetectorRunner"/>, that runs the analysis in a separate process
    /// </summary>
    public class SmartDetectorRunnerInChildProcess : ISmartDetectorRunner
    {
        private const string ChildProcessName = "SmartDetectorRunnerChildProcess.exe";

        private readonly IChildProcessManager childProcessManager;
        private readonly ITracer tracer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartDetectorRunnerInChildProcess"/> class
        /// </summary>
        /// <param name="childProcessManager">The child process manager</param>
        /// <param name="tracer">The tracer</param>
        public SmartDetectorRunnerInChildProcess(IChildProcessManager childProcessManager, ITracer tracer)
        {
            this.childProcessManager = childProcessManager;
            this.tracer = tracer;
        }

        /// <summary>
        /// Runs the Smart Detector analysis, in a separate process
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the generated alerts presentations</returns>
        public async Task<List<ContractsAlert>> RunAsync(SmartDetectorExecutionRequest request, CancellationToken cancellationToken)
        {
            // Find the executable location
            string currentDllPath = new Uri(typeof(SmartDetectorRunnerInChildProcess).Assembly.CodeBase).AbsolutePath;
            string exePath = Path.Combine(Path.GetDirectoryName(currentDllPath) ?? string.Empty, ChildProcessName);
            if (!File.Exists(exePath))
            {
                this.tracer.TraceError($"Verification of executable path {exePath} failed");
                throw new FileNotFoundException("Could not find child process executable", ChildProcessName);
            }

            // Run the child process
            return await this.childProcessManager.RunChildProcessAsync<List<ContractsAlert>>(exePath, request, cancellationToken);
        }
    }
}