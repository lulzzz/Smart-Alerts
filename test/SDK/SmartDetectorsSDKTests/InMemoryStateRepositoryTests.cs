//-----------------------------------------------------------------------
// <copyright file="InMemoryStateRepositoryTests.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartDetectorsSDKTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors.Emulator.State;
    using Microsoft.Azure.Monitoring.SmartDetectors.State;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class InMemoryStateRepositoryTests
    {
        [TestMethod]
        public void WhenRunningMultipleStateActionsInParallelThenNoExceptionIsThrown()
        {
            var stateRepo = new InMemoryStateRepository();

            Random random = new Random();
            List<string> keys = Enumerable.Range(1, 10000)
                .Select(num => random.Next(10).ToString())
                .ToList();

            Parallel.ForEach(keys, key => TestSingleRun(key).Wait());
        }

        [TestMethod]
        public async Task WhenExecutingBasicStateActionsThenFlowCompletesSuccesfully()
        {
            IStateRepository stateRepository = new InMemoryStateRepository();

            var originalState = new TestState
            {
                Field1 = "testdata",
                Field2 = new List<DateTime> { new DateTime(2018, 02, 15) },
                Field3 = true
            };

            await stateRepository.StoreStateAsync("key", originalState, CancellationToken.None);

            var retrievedState = await stateRepository.GetStateAsync<TestState>("key", CancellationToken.None);

            // validate
            Assert.AreEqual(originalState.Field1, retrievedState.Field1);
            CollectionAssert.AreEquivalent(originalState.Field2, retrievedState.Field2);
            Assert.AreEqual(originalState.Field3, retrievedState.Field3);
            Assert.AreEqual(originalState.Field4, retrievedState.Field4);

            // update existing state
            var updatedState = new TestState
            {
                Field1 = null,
                Field2 = new List<DateTime> { new DateTime(2018, 02, 15) },
                Field3 = true
            };

            await stateRepository.StoreStateAsync("key", updatedState, CancellationToken.None);

            retrievedState = await stateRepository.GetStateAsync<TestState>("key", CancellationToken.None);

            // validate
            Assert.AreEqual(updatedState.Field1, retrievedState.Field1);
            CollectionAssert.AreEquivalent(updatedState.Field2, retrievedState.Field2);
            Assert.AreEqual(updatedState.Field3, retrievedState.Field3);
            Assert.AreEqual(updatedState.Field4, retrievedState.Field4);

            await stateRepository.StoreStateAsync("key2", originalState, CancellationToken.None);
            await stateRepository.DeleteStateAsync("key", CancellationToken.None);

            retrievedState = await stateRepository.GetStateAsync<TestState>("key", CancellationToken.None);

            Assert.IsNull(retrievedState);

            retrievedState = await stateRepository.GetStateAsync<TestState>("key2", CancellationToken.None);

            Assert.IsNotNull(retrievedState);
        }

        private static async Task TestSingleRun(string key)
        {
            var stateRepo = new InMemoryStateRepository();

            var state = new List<int> { 5, 10 };

            await stateRepo.StoreStateAsync(key, state, CancellationToken.None);
            await stateRepo.GetStateAsync<List<int>>(key, CancellationToken.None);
            await stateRepo.StoreStateAsync(key, state, CancellationToken.None);
            await stateRepo.GetStateAsync<List<int>>(key, CancellationToken.None);
            await stateRepo.DeleteStateAsync(key, CancellationToken.None);
            await stateRepo.GetStateAsync<List<int>>(key, CancellationToken.None);
        }

        private class TestState
        {
            public string Field1 { get; set; }

            public List<DateTime> Field2 { get; set; }

            public bool Field3 { get; set; }

            public string Field4 { get; set; }
        }
    }
}
