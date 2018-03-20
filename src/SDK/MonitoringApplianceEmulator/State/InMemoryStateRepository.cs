//-----------------------------------------------------------------------
// <copyright file="InMemoryStateRepository.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Emulator.State
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors.State;
    using Microsoft.Azure.Monitoring.SmartDetectors.Tools;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a non-persistent, in-memory state repository.
    /// </summary>
    public class InMemoryStateRepository : IStateRepository
    {
        private const int MaxSerializedStateLength = 1024 * 1024 * 1024;

        private readonly ConcurrentDictionary<string, string> stateDictionary = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryStateRepository"/> class
        /// </summary>
        public InMemoryStateRepository()
        {
        }

        /// <summary>
        /// Stores <paramref name="state"/> in the repository with the specified <paramref name="key"/>. 
        /// If there is already a state stored with the same key, it will be replaced by <paramref name="state"/>.
        /// </summary>
        /// <typeparam name="T">The type of the state. The repository will store the state in the repository as a JSON-serialized string.</typeparam>
        /// <param name="key">The state's key (case insensitive).</param>
        /// <param name="state">The state to store.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        /// <exception cref="System.ArgumentNullException">This exception is thrown if the key or the state are null.</exception>
        /// <exception cref="StateSerializationException">This exception is thrown if state serialization fails.</exception>
        /// <exception cref="StateTooBigException">This exception is thrown if serialized state exceeds allowed length.</exception>
        /// <exception cref="FailedToSaveStateException">This exception is thrown if state saving failed due to an internal error.</exception>
        public Task StoreStateAsync<T>(string key, T state, CancellationToken cancellationToken)
        {
            Diagnostics.EnsureArgumentNotNull(() => key);

            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            string serializedState = string.Empty;
            try
            {
                serializedState = JsonConvert.SerializeObject(state);
            }
            catch (Exception ex)
            {
                throw new StateSerializationException(ex);
            }

            if (serializedState.Length > MaxSerializedStateLength)
            {
                throw new StateTooBigException(serializedState.Length, MaxSerializedStateLength);
            }

            this.stateDictionary[key] = serializedState;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Deletes the state specified by <paramref name="key"/> from the repository.
        /// </summary>
        /// <param name="key">The key of the state to delete (case insensitive).</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        /// <exception cref="System.ArgumentNullException">This exception is thrown if the key is null.</exception>
        /// <exception cref="FailedToDeleteStateException">This exception is thrown if state deletion failed due to an internal error.</exception>
        public Task DeleteStateAsync(string key, CancellationToken cancellationToken)
        {
            Diagnostics.EnsureArgumentNotNull(() => key);

            this.stateDictionary.TryRemove(key, out _);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets a Smart Detector's state that was saved with <paramref name="key"/>.
        /// If state does not exist, returns <code>default(<typeparamref name="T"/>)</code>.
        /// </summary>
        /// <typeparam name="T">The type of the state. The repository will try to JSON-deserialize the stored state to this type.</typeparam>
        /// <param name="key">The key that was used to store the state (case insensitive).</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation, returning the requested state.</returns>
        /// <exception cref="System.ArgumentNullException">This exception is thrown if the key is null.</exception>
        /// <exception cref="StateSerializationException">This exception is thrown if state deserialization fails.</exception>
        /// <exception cref="FailedToLoadStateException">This exception is thrown if state retrieval failed due to an internal error.</exception>
        public Task<T> GetStateAsync<T>(string key, CancellationToken cancellationToken)
        {
            Diagnostics.EnsureArgumentNotNull(() => key);

            if (this.stateDictionary.TryGetValue(key, out string value))
            {
                T state = default(T);
                try
                {
                    state = JsonConvert.DeserializeObject<T>(value);
                }
                catch (Exception ex)
                {
                    throw new StateSerializationException(ex);
                }

                return Task.FromResult(state);
            }

            return Task.FromResult(default(T));
        }
    }
}
