// Copyright (c) Isaiah Williams. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.WebJobs.Extensions.PartnerCenter
{
    using System;
    using Store.PartnerCenter;

    /// <summary>
    /// Provides the credentials need to access the partner service.
    /// </summary>
    internal class FunctionExtensionCredentials : IPartnerCredentials
    {
        /// <summary>
        /// The result from a successfully token request.
        /// </summary>
        private readonly AuthenticationToken authToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionExtensionCredentials" /> class.
        /// </summary>
        /// <param name="authToken">The authentication token for Partner Center.</param>
        public FunctionExtensionCredentials(AuthenticationToken authToken)
        {
            this.authToken = authToken;
        }

        /// <summary>
        /// Gets the expiry time in UTC for the token.
        /// </summary>
        public DateTimeOffset ExpiresAt => authToken.ExpiryTime;

        /// <summary>
        /// Gets the token needed to authenticate with the partner service.
        /// </summary>
        public string PartnerServiceToken => authToken.Token;

        /// <summary>
        /// Indicates whether the partner credentials have expired or not.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the partner credentials have expired; otherwise <c>false</c>.
        /// </returns>
        public bool IsExpired()
        {
            return DateTimeOffset.UtcNow > authToken.ExpiryTime;
        }
    }
}