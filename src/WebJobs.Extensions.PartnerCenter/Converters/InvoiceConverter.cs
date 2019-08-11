// Copyright (c) Isaiah Williams. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.WebJobs.Extensions.PartnerCenter
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Store.PartnerCenter;
    using Store.PartnerCenter.Models;
    using Store.PartnerCenter.Models.Invoices;

    /// <summary>
    /// Converter used to obtain a collection of invoices based on the input.
    /// </summary>
    internal class InvoiceConverter : IAsyncConverter<InvoiceAttribute, IEnumerable<Invoice>>
    {
        /// <summary>
        /// The configuration provider for the Partner Center extension.
        /// </summary>
        private readonly PartnerCenterExtensionConfigProvider provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvoiceConverter"/> class.
        /// </summary>
        /// <param name="provider">The configuration provider for the Partner Center extension.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="provider"/> is null.
        /// </exception>
        public InvoiceConverter(PartnerCenterExtensionConfigProvider provider)
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
        public async Task<IEnumerable<Invoice>> ConvertAsync(InvoiceAttribute input, CancellationToken cancellationToken)
        {
            IPartner operations = PartnerService.Instance.CreatePartnerOperations(
                await provider.GetCredentialsAsync(input).ConfigureAwait(false),
                provider.GetHttpClient(input.ApplicationId));
            ResourceCollection<Invoice> invoices;


            if (string.IsNullOrEmpty(input.InvoiceId))
            {
                invoices = await operations.Invoices.GetAsync(cancellationToken).ConfigureAwait(false);

                return invoices.Items;
            }

            return new List<Invoice> { await operations.Invoices.ById(input.InvoiceId).GetAsync(cancellationToken).ConfigureAwait(false) };
        }
    }
}
