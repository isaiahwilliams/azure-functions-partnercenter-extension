// Copyright (c) Isaiah Williams. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.WebJobs.Extensions.PartnerCenter
{
    using System;
    using Description;

    /// <summary>
    /// Attribute used by the Azure Functions framework to obtain Azure utilization records from the partner service.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    [Binding]
    public sealed class AzureUtilizationRecordAttribute : TokenBaseAttribute
    {
        /// <summary>
        /// Gets or sets the identifier of the customer.
        /// </summary>
        [AutoResolve]
        public string CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the starting time of when the utilization was metered in the billing system.
        /// </summary>
        [AutoResolve]
        public string EndTime { get; set; }

        /// <summary>
        /// Gets or sets the resource usage time granularity. Can either be daily or hourly. Default is daily.
        /// </summary>
        [AutoResolve]
        public string Granularity { get; set; }

        /// <summary>
        /// Gets or set the identifier of the subscription.
        /// </summary>
        [AutoResolve]
        public string SubscriptionId { get; set; }

        /// <summary>
        /// Gets or sets the starting time of when the utilization was metered in the billing system.
        /// </summary>
        [AutoResolve]
        public string StartTime { get; set; }
    }
}