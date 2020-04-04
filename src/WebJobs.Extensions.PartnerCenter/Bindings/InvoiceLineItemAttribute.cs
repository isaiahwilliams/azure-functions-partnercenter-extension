// Copyright (c) Isaiah Williams. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.WebJobs.Extensions.PartnerCenter
{
    using System;
    using Description;
    using Store.PartnerCenter.Models.Invoices;

    /// <summary>
    /// Attribute used by the Azure Functions framework to obtain invoice line items information from the partner service.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    [Binding]
    public sealed class InvoiceLineItemAttribute : TokenBaseAttribute
    {
        /// <summary>
        /// Gets or sets the billing provider for the invoice line items.
        /// </summary>
        [AutoResolve]
        public BillingProvider BillingProvider { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the invoice.
        /// </summary>
        [AutoResolve]
        public string InvoiceId { get; set; }

        /// <summary>
        /// Gets or sets the type of invoice line items.
        /// </summary>
        [AutoResolve]
        public InvoiceLineItemType LineItemType { get; set; }

        /// <summary>
        /// Gets or sets the period for the invoice line items.
        /// </summary>
        [AutoResolve]
        public BillingPeriod Period { get; set; }
    }
}