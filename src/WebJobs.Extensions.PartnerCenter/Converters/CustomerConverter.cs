// Copyright (c) Isaiah Williams. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.WebJobs.Extensions.PartnerCenter
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Store.PartnerCenter;
    using Store.PartnerCenter.Enumerators;
    using Store.PartnerCenter.Models;
    using Store.PartnerCenter.Models.Customers;

    /// <summary>
    /// Converter used to obtain a collection of customers based on the input.
    /// </summary>
    internal class CustomerConverter : IAsyncConverter<CustomerAttribute, IEnumerable<Customer>>
    {
        /// <summary>
        /// The configuration provider for the Partner Center extension.
        /// </summary>
        private readonly PartnerCenterExtensionConfigProvider provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerConverter"/> class.
        /// </summary>
        /// <param name="provider">The configuration provider for the Partner Center extension.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="provider"/> is null.
        /// </exception>
        public CustomerConverter(PartnerCenterExtensionConfigProvider provider)
        {
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <summary>
        /// Converts the input to a collection of customers.
        /// </summary>
        /// <param name="input">The attribute that provides the input.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>
        /// A collection containing instances of the <see cref="Customer" /> class that match the specified input.
        /// </returns>
        public async Task<IEnumerable<Customer>> ConvertAsync(CustomerAttribute input, CancellationToken cancellationToken)
        {
            Customer customer;
            IResourceCollectionEnumerator<SeekBasedResourceCollection<Customer>> enumerator;
            List<Customer> customers;
            SeekBasedResourceCollection<Customer> seekCustomers;
            IPartner partner;

            customers = new List<Customer>();

            partner = PartnerService.Instance.CreatePartnerOperations(
                await provider.GetCredentialsAsync(input).ConfigureAwait(false),
                provider.GetHttpClient(input.ApplicationId));

            if (string.IsNullOrEmpty(input.CustomerId))
            {
                seekCustomers = await partner.Customers.GetAsync(cancellationToken).ConfigureAwait(false);

                enumerator = partner.Enumerators.Customers.Create(seekCustomers);

                while (enumerator.HasValue)
                {
                    customers.AddRange(enumerator.Current.Items);
                    await enumerator.NextAsync(null, cancellationToken).ConfigureAwait(false);
                }

                return customers;
            }

            customer = await partner.Customers[input.CustomerId].GetAsync(cancellationToken).ConfigureAwait(false);

            return new List<Customer> { customer };
        }
    }
}