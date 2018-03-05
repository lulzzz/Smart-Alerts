//-----------------------------------------------------------------------
// <copyright file="TestSignalWithDependency.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace TestSignalLibrary
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using TestSignalDependentLibrary;

    public class TestSignalWithDependency : ISmartDetector
    {
        public Task<List<Alert>> AnalyzeResourcesAsync(AnalysisRequest analysisRequest, ITracer tracer, CancellationToken cancellationToken)
        {
            int[] obj = { 1, 2, 3 };
            var dependent = new DependentClass();
            List<Alert> alerts = new List<Alert>();
            alerts.Add(new TestSignalResultItem(
                "test title - " + dependent.GetString() + " - " + dependent.ObjectToString(obj),
                analysisRequest.TargetResources.First()));
            return Task.FromResult(alerts);
        }
    }
}