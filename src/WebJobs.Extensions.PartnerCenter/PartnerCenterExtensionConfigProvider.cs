// Copyright (c) Isaiah Williams. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.WebJobs.Extensions.PartnerCenter
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Description;
    using Extensions;
    using Host.Config;
    using Identity.Client;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Azure.KeyVault.Models;
    using Microsoft.Azure.Services.AppAuthentication;
    using Microsoft.Extensions.Logging;
    using Rest;
    using Store.PartnerCenter;
    using Store.PartnerCenter.Extensions;
    using Store.PartnerCenter.Models.Auditing;
    using Store.PartnerCenter.Models.Customers;
    using Store.PartnerCenter.Models.Invoices;
    using Store.PartnerCenter.Models.Subscriptions;
    using Store.PartnerCenter.Models.Utilizations;

    [Extension("PartnerCenter")]
    internal class PartnerCenterExtensionConfigProvider : IExtensionConfigProvider
    {
        /// <summary>
        /// Used to configure the logging system and create instances of <see cref="ILogger" />.
        /// </summary>
        private readonly ILoggerFactory loggerFactory;

        /// <summary>
        /// Used to perform logging.
        /// </summary>
        private ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PartnerCenterExtensionConfigProvider" /> class.
        /// </summary>
        /// <param name="loggerFactory">Factory used to write information to the log.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="loggerFactory"/> is null.
        /// </exception>
        public PartnerCenterExtensionConfigProvider(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        /// <summary>
        /// Gets an instance of <see cref="ILogger" /> used to perform logging.
        /// </summary>
        public ILogger Logger
        {
            get
            {
                if (logger == null)
                {
                    logger = loggerFactory.CreateLogger("PartnerCenter");
                }

                return logger;
            }
        }

        /// <summary>
        /// The collection of <see cref="HttpClient" /> objects used to access the partner service.
        /// </summary>
        private ConcurrentDictionary<string, HttpClient> ClientCache { get; } = new ConcurrentDictionary<string, HttpClient>();

        /// <summary>
        /// Gets the credentials used to communicate with the partner service.
        /// </summary>
        /// <param name="input">The attribute used to generate the credentials.</param>
        /// <returns>
        /// An instance of <see cref="FunctionExtensionCredentials" /> that can be used to communicate with the partner service.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="input"/> is null.
        /// </exception>
        public async Task<IPartnerCredentials> GetCredentialsAsync(TokenBaseAttribute input)
        {
            AuthenticationResult authResult;
            IConfidentialClientApplication app;
            string applicationSecret = string.Empty;
            string refreshToken = string.Empty;

            input.AssertNotNull(nameof(input));

            if (!string.IsNullOrEmpty(input.ApplicationSecretName) || !string.IsNullOrEmpty(input.RefreshTokenName))
            {
                if (string.IsNullOrEmpty(input.KeyVaultEndpoint))
                {
                    throw new ArgumentException("When using the name properties the Key Vault endpoint must be specified.");
                }

                using (IKeyVaultClient keyVaultClient = new KeyVaultClient(
                    new KeyVaultClient.AuthenticationCallback(new AzureServiceTokenProvider().KeyVaultTokenCallback)))
                {
                    if (!string.IsNullOrEmpty(input.ApplicationSecretName))
                    {
                        applicationSecret = await GetSecretAsync(
                            keyVaultClient,
                            input.KeyVaultEndpoint,
                            input.ApplicationSecretName).ConfigureAwait(false);
                    }

                    if (!string.IsNullOrEmpty(input.RefreshTokenName))
                    {
                        refreshToken = await GetSecretAsync(
                            keyVaultClient,
                            input.KeyVaultEndpoint,
                            input.RefreshTokenName).ConfigureAwait(false);
                    }
                }
            }

            if (string.IsNullOrEmpty(applicationSecret))
            {
                applicationSecret = input.ApplicationSecret;
            }

            if (string.IsNullOrEmpty(refreshToken))
            {
                refreshToken = input.RefreshToken;
            }

            if (string.IsNullOrEmpty(refreshToken))
            {
                return await PartnerCredentials.GenerateByApplicationCredentialsAsync(
                    input.ApplicationId,
                    applicationSecret,
                    input.TenantId).ConfigureAwait(false);
            }
            else
            {
                app = ConfidentialClientApplicationBuilder
                    .Create(input.ApplicationId)
                    .WithAuthority(new Uri($"{input.Authority}/{input.TenantId}"))
                    .WithClientSecret(applicationSecret)
                    .Build();

                authResult = await app
                    .AsRefreshTokenClient()
                    .AcquireTokenByRefreshToken(input.Scopes.Split(','), refreshToken)
                    .ExecuteAsync().ConfigureAwait(false);

                return new FunctionExtensionCredentials(new AuthenticationToken(authResult.AccessToken, authResult.ExpiresOn));
            }
        }

        /// <summary>
        /// Gets an instance of <see cref="HttpClient" /> from the collection.
        /// </summary>
        /// <param name="applicationId">The identifier of the application being used to communicate with the partner service.</param>
        /// <returns>
        /// An instance of the <see cref="HttpClient" /> class that can be used to communicate with the partner service.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="applicationId"/> is empty or null.
        /// </exception>
        public HttpClient GetHttpClient(string applicationId)
        {
            applicationId.AssertNotEmpty(nameof(applicationId));

            return ClientCache.GetOrAdd(applicationId, c => new HttpClient(new CancelRetryHandler(3, TimeSpan.FromSeconds(10))
            {
                InnerHandler = new RetryDelegatingHandler
                {
                    InnerHandler = new HttpClientHandler()
                }
            }));
        }

        /// <summary>
        /// Performs the operations to initialize the extension.
        /// </summary>
        /// <param name="context">The context for the extension configuration.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="context"/> is null.
        /// </exception>
        public void Initialize(ExtensionConfigContext context)
        {
            context.AssertNotNull(nameof(context));

            context
                .AddBindingRule<AuditRecordAttribute>()
                .BindToInput<List<AuditRecord>>(typeof(AuditRecordConverter), this);

            context
                .AddBindingRule<AzureUtilizationRecordAttribute>()
                .BindToInput<List<AzureUtilizationRecord>>(typeof(AzureUtilizationRecordConverter), this);

            context
                .AddBindingRule<CustomerAttribute>()
                .WhenIsNull(nameof(CustomerAttribute.CustomerId))
                .BindToInput<List<Customer>>(typeof(CustomerConverter), this);

            context
                .AddBindingRule<CustomerAttribute>()
                .WhenIsNotNull(nameof(CustomerAttribute.CustomerId))
                .BindToInput<Customer>(typeof(CustomerConverter), this);

            context
                .AddBindingRule<InvoiceAttribute>()
                .WhenIsNull(nameof(InvoiceAttribute.InvoiceId))
                .BindToInput<List<Invoice>>(typeof(InvoiceConverter), this);

            context
                .AddBindingRule<InvoiceAttribute>()
                .WhenIsNotNull(nameof(InvoiceAttribute.InvoiceId))
                .BindToInput<Invoice>(typeof(InvoiceConverter), this);

            context
                .AddBindingRule<InvoiceLineItemAttribute>()
                .BindToInput<List<InvoiceLineItem>>(typeof(InvoiceLineItemConverter), this);

            context
                .AddBindingRule<SubscriptionAttribute>()
                .WhenIsNull(nameof(SubscriptionAttribute.SubscriptionId))
                .BindToInput<List<Subscription>>(typeof(SubscriptionConverter), this);

            context
                .AddBindingRule<SubscriptionAttribute>()
                .WhenIsNotNull(nameof(SubscriptionAttribute.SubscriptionId))
                .BindToInput<Subscription>(typeof(SubscriptionConverter), this);
        }

        private async Task<string> GetSecretAsync(IKeyVaultClient keyVaultClient, string vaultBaseUrl, string secretName)
        {
            keyVaultClient.AssertNotNull(nameof(keyVaultClient));
            vaultBaseUrl.AssertNotNull(nameof(vaultBaseUrl));
            secretName.AssertNotNull(nameof(secretName));

            SecretBundle bundle = await keyVaultClient.GetSecretAsync(
                vaultBaseUrl,
                secretName).ConfigureAwait(false);

            return bundle.Value;
        }
    }
}