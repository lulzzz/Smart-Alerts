//-----------------------------------------------------------------------
// <copyright file="TestSignal.cs" company="Microsoft Corporation">
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

    public class TestSignal : ISmartDetector
    {
        public Task<List<Alert>> AnalyzeResourcesAsync(AnalysisRequest analysisRequest, ITracer tracer, CancellationToken cancellationToken)
        {
            List<Alert> alerts = new List<Alert>();
            alerts.Add(new TestSignalResultItem("test title", analysisRequest.TargetResources.First()));
            return Task.FromResult(alerts);
        }
    }
}
