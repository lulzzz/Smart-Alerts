﻿//-----------------------------------------------------------------------
// <copyright file="Schedule.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.FunctionApp
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.AlertRules;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Scheduler;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Scheduler.Publisher;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Scheduler.SmartDetectorRunTracker;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;
    using Unity;

    /// <summary>
    /// A class responsible for scheduling the Smart Detectors execution
    /// </summary>
    public static class Schedule
    {
        private static readonly IUnityContainer Container;

        /// <summary>
        /// Initializes static members of the <see cref="Schedule"/> class.
        /// </summary>
        static Schedule()
        {
            // To increase Azure calls performance we increase default connection limit (default is 2) and ThreadPool minimum threads to allow more open connections
            System.Net.ServicePointManager.DefaultConnectionLimit = 100;
            ThreadPool.SetMinThreads(100, 100);

            Container = DependenciesInjector.GetContainer()
                .RegisterType<IAlertRuleStore, AlertRuleStore>()
                .RegisterType<ISmartDetectorRunsTracker, SmartDetectorRunsTracker>()
                .RegisterType<IAnalysisExecuter, AnalysisExecuter>()
                .RegisterType<IAlertsPublisher, AlertsPublisher>();
        }

        /// <summary>
        /// The starting point for the Smart Detector scheduler
        /// </summary>
        /// <param name="myTimer">The timer information of the timer trigger used by Azure to trigger the function</param>
        /// <param name="log">The function's logger</param>
        /// <returns>A <see cref="Task"/> running the asynchronous operation.</returns>
        [FunctionName("Schedule")]
        public static async Task RunAsync([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            // Since we add the web job log tracer for each function invocation then we need to register the instance here in a child container
            using (IUnityContainer childContainer = Container.CreateChildContainer().WithTracer(log, true))
            {
                ITracer tracer = childContainer.Resolve<ITracer>();
                var scheduleFlow = childContainer.Resolve<ScheduleFlow>();
                
                try
                {
                    tracer.TraceInformation($"Executing schedule flow");
                    await scheduleFlow.RunAsync();
                    tracer.TraceInformation($"Executed schedule flow successfully");
                }
                catch (Exception exception)
                {
                    tracer.TraceError($"Failed running scheduling with exception {exception}");
                    TopLevelExceptionHandler.TraceUnhandledException(exception, tracer, log);
                    throw;
                }
            }
        }
    }
}
