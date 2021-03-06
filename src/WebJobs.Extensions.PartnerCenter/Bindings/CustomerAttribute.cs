﻿// Copyright (c) Isaiah Williams. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.WebJobs.Extensions.PartnerCenter
{
    using System;
    using Description;

    /// <summary>
    /// Attribute used by the Azure Functions framework to obtain customer information from the partner service.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    [Binding]
    public sealed class CustomerAttribute : TokenBaseAttribute
    {
        /// <summary>
        /// Gets or sets the identifier of the customer.
        /// </summary>
        [AutoResolve]
        public string CustomerId { get; set; }
    }
}