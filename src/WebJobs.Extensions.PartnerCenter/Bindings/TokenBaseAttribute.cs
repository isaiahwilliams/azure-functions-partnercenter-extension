// Copyright (c) Isaiah Williams. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.WebJobs.Extensions.PartnerCenter
{
    using System;
    using Description;

    /// <summary>
    /// Represents an attribute used to obtain access tokens.
    /// </summary>
    public abstract class TokenBaseAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the identifier of the client requesting the token.
        /// </summary>
        [AutoResolve]
        public string ApplicationId { get; set; }

        /// <summary>
        /// Gets or sets the secret of the client requesting the token.
        /// </summary>
        [AutoResolve]
        public string ApplicationSecret { get; set; }

        /// <summary>
        /// Gets or sets the Key Vault secret name for the secret of the client requesting the token.
        /// </summary>
        [AutoResolve]
        public string ApplicationSecretName { get; set; }

        /// <summary>
        /// Gets or sets the address of the authority to issue the token.
        /// </summary>
        [AutoResolve]
        public string Authority { get; set; } = "https://login.microsoftonline.com";

        /// <summary>
        /// Gets or sets the Key Vault endpoint address.
        /// </summary>
        [AutoResolve]
        public string KeyVaultEndpoint { get; set; }

        /// <summary>
        /// Gets or set the refresh token to be used to obtain a new access token.
        /// </summary>
        [AutoResolve]
        public string RefreshToken { get; set; }

        /// <summary>
        /// Gets or sets the Key Vault secret name for the refresh token to be used to obtain a new access token.
        /// </summary>
        [AutoResolve]
        public string RefreshTokenName { get; set; }

        /// <summary>
        /// Gets or sets the scopes of the token being requested.
        /// </summary>
        [AutoResolve]
        public string Scopes { get; set; } = "https://api.partnercenter.microsoft.com/user_impersonation";

        /// <summary>
        /// Gets or sets the tenant identifier that owns the application.
        /// </summary>
        [AutoResolve]
        public string TenantId { get; set; }
    }
}
