// Copyright (c) Isaiah Williams. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.WebJobs.Extensions.PartnerCenter
{
    using System;
    using Description;

    /// <summary>
    /// Attribute used by the Azure Functions framework to obtain audit records from the partner service.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    [Binding]
    public sealed class AuditRecordAttribute : AuthenticationAttribute
    {
        /// <summary>
        /// Gets or sets the end date of the audit record logs.
        /// </summary>
        [AutoResolve]
        public string EndDate { get; set; }

        /// <summary>
        /// Gets or sets the start date of the audit record logs.
        /// </summary>
        [AutoResolve]
        public string StartDate { get; set; }
    }
}