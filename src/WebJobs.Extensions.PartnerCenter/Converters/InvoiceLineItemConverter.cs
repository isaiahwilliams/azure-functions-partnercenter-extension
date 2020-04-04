// Copyright (c) Isaiah Williams. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.WebJobs.Extensions.PartnerCenter
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Store.PartnerCenter.Enumerators;
    using Store.PartnerCenter;
    using Store.PartnerCenter.Models;
    using Store.PartnerCenter.Models.Invoices;

    /// <summary>
    /// Converter used to obtain a collection of invoice line items based on the input.
    /// </summary>
    internal class InvoiceLineItemConverter : IAsyncConverter<InvoiceLineItemAttribute, IEnumerable<InvoiceLineItem>>
    {
        /// <summary>
        /// The configuration provider for the Partner Center extension.
        /// </summary>
        private readonly PartnerCenterExtensionConfigProvider provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvoiceLineItemConverter"/> class.
        /// </summary>
        /// <param name="provider">The configuration provider for the Partner Center extension.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="provider"/> is null.
        /// </exception>
        public InvoiceLineItemConverter(PartnerCenterExtensionConfigProvider provider)
        {
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <summary>
        /// Converts the input to a collection of invoices.
        /// </summary>
        /// <param name="input">The attribute that provides the input.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>
        /// A collection containing instances of the <see cref="Invoice" /> class that match the specified input.
        /// </returns>
        public async Task<IEnumerable<InvoiceLineItem>> ConvertAsync(InvoiceLineItemAttribute input, CancellationToken cancellationToken)
        {
            IPartner partner = PartnerService.Instance.CreatePartnerOperations(
                await provider.GetCredentialsAsync(input).ConfigureAwait(false),
                provider.GetHttpClient(input.ApplicationId));

            ResourceCollection<InvoiceLineItem> lineItems = await partner.Invoices
                .ById(input.InvoiceId)
                .By(input.BillingProvider, input.LineItemType)
                .GetAsync(cancellationToken)
                .ConfigureAwait(false);

            IResourceCollectionEnumerator<ResourceCollection<InvoiceLineItem>> enumerator = partner.Enumerators.InvoiceLineItems.Create(lineItems);

            List<InvoiceLineItem> items = new List<InvoiceLineItem>();

            while (enumerator.HasValue)
            {
                items.AddRange(enumerator.Current.Items);
                await enumerator.NextAsync(null, cancellationToken).ConfigureAwait(false);
            }

            return items;
        }
    }
}
