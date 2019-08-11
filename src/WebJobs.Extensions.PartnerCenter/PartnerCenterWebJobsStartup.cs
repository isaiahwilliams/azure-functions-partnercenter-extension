// Copyright (c) Isaiah Williams. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

[assembly: Microsoft.Azure.WebJobs.Hosting.WebJobsStartup(typeof(Microsoft.Azure.WebJobs.Extensions.PartnerCenter.PartnerCenterWebJobsStartup))]

namespace Microsoft.Azure.WebJobs.Extensions.PartnerCenter
{
    using Azure.WebJobs.Hosting;

    public class PartnerCenterWebJobsStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddPartnerCenter();
        }
    }
}