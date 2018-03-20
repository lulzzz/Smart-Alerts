//-----------------------------------------------------------------------
// <copyright file="InMemoryStateRepositoryFactory.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Emulator.State
{
    using Microsoft.Azure.Monitoring.SmartDetectors.State;
    using Microsoft.Azure.Monitoring.SmartDetectors.Tools;

    /// <summary>
    /// Represents a factory for creating in-memory state repository for a Smart Detector.
    /// </summary>
    public class InMemoryStateRepositoryFactory : IStateRepositoryFactory
    {
        /// <summary>
        /// Creates a state repository for a Smart Detector with ID <paramref name="smartDetectorId"/>.
        /// </summary>
        /// <param name="smartDetectorId">The ID of the Smart Detector to create the state repository for.</param>
        /// <returns>A state repository associated with the requested Smart Detector.</returns>
        public IStateRepository Create(string smartDetectorId)
        {
            Diagnostics.EnsureStringNotNullOrWhiteSpace(() => smartDetectorId);

            return new InMemoryStateRepository();
        }
    }
}
