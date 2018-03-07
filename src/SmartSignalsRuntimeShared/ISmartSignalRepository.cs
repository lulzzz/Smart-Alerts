//-----------------------------------------------------------------------
// <copyright file="ISmartSignalRepository.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.RuntimeShared
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors.Package;

    /// <summary>
    /// Interface for the Smart Signal repository
    /// </summary>
    public interface ISmartSignalRepository
    {
        /// <summary>
        /// Reads all the smart signals manifests from the repository
        /// For each signal we return the latest version's manifest.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/> returning the smart signals manifests</returns>
        Task<IList<SmartDetectorManifest>> ReadAllSignalsManifestsAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Reads a smart signal's package from the repository
        /// </summary>
        /// <param name="signalId">The signal's ID</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/> returning the signal package</returns>
        Task<SmartDetectorPackage> ReadSignalPackageAsync(string signalId, CancellationToken cancellationToken);
    }
}