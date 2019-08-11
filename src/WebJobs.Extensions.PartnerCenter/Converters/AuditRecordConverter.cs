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
    using Store.PartnerCenter.Models;
    using Store.PartnerCenter.Models.Auditing;

    /// <summary>
    /// Converter used to obtain a collection of audit records based on the input.
    /// </summary>
    internal class AuditRecordConverter : IAsyncConverter<AuditRecordAttribute, IEnumerable<AuditRecord>>
    {
        /// <summary>
        /// The configuration provider for the Partner Center extension.
        /// </summary>
        private readonly PartnerCenterExtensionConfigProvider provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditRecordConverter"/> class.
        /// </summary>
        /// <param name="provider">The configuration provider for the Partner Center extension.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="provider"/> is null.
        /// </exception>
        public AuditRecordConverter(PartnerCenterExtensionConfigProvider provider)
        {
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <summary>
        /// Converts the input to a collection of audit records.
        /// </summary>
        /// <param name="input">The attribute that provides the input.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>
        /// A collection containing instances of the <see cref="AuditRecord" /> class that match the specified input.
        /// </returns>
        public async Task<IEnumerable<AuditRecord>> ConvertAsync(AuditRecordAttribute input, CancellationToken cancellationToken)
        {
            IPartner operations = PartnerService.Instance.CreatePartnerOperations(
                await provider.GetCredentialsAsync(input).ConfigureAwait(false),
                provider.GetHttpClient(input.ApplicationId));


            SeekBasedResourceCollection<AuditRecord> records = await
                operations.AuditRecords.QueryAsync(
                    DateTime.Parse(input.StartDate, CultureInfo.CurrentCulture),
                    DateTime.Parse(input.EndDate, CultureInfo.CurrentCulture),
                    null, cancellationToken).ConfigureAwait(false);

            return records.Items;
        }
    }
}