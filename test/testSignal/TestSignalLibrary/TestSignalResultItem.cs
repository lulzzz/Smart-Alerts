//-----------------------------------------------------------------------
// <copyright file="TestSignalResultItem.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace TestSignalLibrary
{
    using Microsoft.Azure.Monitoring.SmartDetectors;

    public class TestSignalResultItem : Alert
    {
        public TestSignalResultItem(string title, ResourceIdentifier resourceIdentifier) : base(title, resourceIdentifier)
        {
        }
    }
}