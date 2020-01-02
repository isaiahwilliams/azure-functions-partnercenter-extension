// Copyright (c) Isaiah Williams. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.WebJobs.Extensions.PartnerCenter
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Store.PartnerCenter;
    using Store.PartnerCenter.Enumerators;
    using Store.PartnerCenter.Models;
    using Store.PartnerCenter.Models.Utilizations;

    /// <summary>
    /// Converter used to obtain a collection of Azure utilization records based on the input.
    /// </summary>
    internal class AzureUtilizationRecordConverter : IAsyncConverter<AzureUtilizationRecordAttribute, IEnumerable<AzureUtilizationRecord>>
    {
        /// <summary>
        /// The configuration provider for the Partner Center extension.
        /// </summary>
        private readonly PartnerCenterExtensionConfigProvider provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureUtilizationRecordConverter"/>
        /// </summary>
        /// <param name="provider">The configuration provider for the Partner Center extension.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="provider"/> is null.
        /// </exception>
        public AzureUtilizationRecordConverter(PartnerCenterExtensionConfigProvider provider)
        {
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <summary>
        /// Converts the input to a collection of Azure utilization records.
        /// </summary>
        /// <param name="input">The attribute that provides the input.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>
        /// A collection containing instances of the <see cref="AzureUtilizationRecord" /> class that match the specified input.
        /// </returns>
        public async Task<IEnumerable<AzureUtilizationRecord>> ConvertAsync(AzureUtilizationRecordAttribute input, CancellationToken cancellationToken)
        {
            IPartner partner;
            IResourceCollectionEnumerator<ResourceCollection<AzureUtilizationRecord>> enumerator;
            List<AzureUtilizationRecord> utilizationRecords;
            ResourceCollection<AzureUtilizationRecord> records;

            partner = PartnerService.Instance.CreatePartnerOperations(
                await provider.GetCredentialsAsync(input).ConfigureAwait(false),
                provider.GetHttpClient(input.ApplicationId));

            records = await partner.Customers[input.CustomerId]
                .Subscriptions[input.SubscriptionId].Utilization.Azure.QueryAsync(
                    DateTimeOffset.Parse(input.StartTime, CultureInfo.CurrentCulture),
                    DateTimeOffset.Parse(input.EndTime, CultureInfo.CurrentCulture),
                    (AzureUtilizationGranularity)Enum.Parse(typeof(AzureUtilizationGranularity), input.Granularity),
                    true,
                    1000,
                    cancellationToken).ConfigureAwait(false);

            enumerator = partner.Enumerators.Utilization.Azure.Create(records);

            utilizationRecords = new List<AzureUtilizationRecord>();

            while (enumerator.HasValue)
            {
                utilizationRecords.AddRange(enumerator.Current.Items);
                await enumerator.NextAsync(null, cancellationToken).ConfigureAwait(false);
            }

            return utilizationRecords;
        }
    }
}