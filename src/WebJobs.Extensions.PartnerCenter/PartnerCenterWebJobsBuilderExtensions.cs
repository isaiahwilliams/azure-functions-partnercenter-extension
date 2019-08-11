// Copyright (c) Isaiah Williams. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.WebJobs.Extensions.PartnerCenter
{
    /// <summary>
    /// Provides useful extension methods for enable the Partner Center extension.
    /// </summary>
    public static class PartnerCenterWebJobsBuilderExtensions
    {
        /// <summary>
        /// Adds the Partner Center extension to the provided <see cref="IWebJobsBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="IWebJobsBuilder" /> to configure.</param>
        public static IWebJobsBuilder AddPartnerCenter(this IWebJobsBuilder builder)
        {
            builder.AssertNotNull(nameof(builder));

            builder.AddExtension<PartnerCenterExtensionConfigProvider>();

            return builder;
        }
    }
}