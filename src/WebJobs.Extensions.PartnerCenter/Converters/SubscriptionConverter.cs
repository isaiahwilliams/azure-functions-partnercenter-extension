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
    using Store.PartnerCenter.Models.Subscriptions;

    /// <summary>
    /// Converter used to obtain a collection of subscriptions based on the input.
    /// </summary>
    internal class SubscriptionConverter : IAsyncConverter<SubscriptionAttribute, IEnumerable<Subscription>>
    {
        /// <summary>
        /// The configuration provider for the Partner Center extension.
        /// </summary>
        private readonly PartnerCenterExtensionConfigProvider provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionConverter"/>
        /// </summary>
        /// <param name="provider">The configuration provider for the Partner Center extension.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="provider"/> is null.
        /// </exception>
        public SubscriptionConverter(PartnerCenterExtensionConfigProvider provider)
        {
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <summary>
        /// Converts the input to a collection of subscriptions.
        /// </summary>
        /// <param name="input">The attribute that provides the input.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>
        /// A collection containing instances of the <see cref="Subscription" /> class that match the specified input.
        /// </returns>
        public async Task<IEnumerable<Subscription>> ConvertAsync(SubscriptionAttribute input, CancellationToken cancellationToken)
        {
            IPartner operations;
            ResourceCollection<Subscription> subscriptionCollection;
            Subscription subscription;

            operations = PartnerService.Instance.CreatePartnerOperations(
                await provider.GetCredentialsAsync(input).ConfigureAwait(false),
                provider.GetHttpClient(input.ApplicationId));

            if (string.IsNullOrEmpty(input.SubscriptionId))
            {
                subscriptionCollection = await operations.Customers[input.CustomerId]
                    .Subscriptions.GetAsync(cancellationToken).ConfigureAwait(false);

                return subscriptionCollection.Items;
            }

            subscription = await operations.Customers[input.CustomerId]
                .Subscriptions[input.SubscriptionId].GetAsync(cancellationToken).ConfigureAwait(false);

            return new List<Subscription> { subscription };
        }
    }
}