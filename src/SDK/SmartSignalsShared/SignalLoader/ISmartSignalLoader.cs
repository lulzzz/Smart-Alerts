//-----------------------------------------------------------------------
// <copyright file="ISmartSignalLoader.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.SignalLoader
{
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartSignals.Package;

    /// <summary>
    /// An interface used for loading a Smart Signal from its package
    /// </summary>
    public interface ISmartSignalLoader
    {
        /// <summary>
        /// Load a signal from its package.
        /// </summary>
        /// <param name="signalPackage">The signal package.</param>
        /// <returns>The signal instance.</returns>
        ISmartDetector LoadSignal(SmartSignalPackage signalPackage);
    }
}