using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Identity;
using Toolbox.Tools;
using Toolbox.Extensions;

namespace Toolbox.Azure.Identity;

public static class ClientCredential
{
    public static ClientSecretCredential ToClientSecretCredential(this ClientSecretOption clientSecretOption)
    {
        clientSecretOption.NotNull();

        return new ClientSecretCredential(
            clientSecretOption.TenantId,
            clientSecretOption.ClientId,
            clientSecretOption.ClientSecret);
    }

    public static ClientSecretCredential ToClientSecretCredential(string connectionString)
    {
        ClientSecretOption option = connectionString
            .ToDictionaryFromString()
            .ToObject<ClientSecretOption>();

        return ToClientSecretCredential(option);
    }
}
