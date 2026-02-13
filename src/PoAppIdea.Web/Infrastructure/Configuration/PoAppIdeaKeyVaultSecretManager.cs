using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;

namespace PoAppIdea.Web.Infrastructure.Configuration;

/// <summary>
/// Maps Key Vault secret names to app configuration keys.
/// Supports both generic keys (e.g. AzureOpenAI--Endpoint)
/// and app-prefixed keys (e.g. PoAppIdea--AzureAI--Endpoint).
/// </summary>
public sealed class PoAppIdeaKeyVaultSecretManager : KeyVaultSecretManager
{
    private const string Prefix = "PoAppIdea--";

    public override bool Load(SecretProperties secret)
    {
        return true;
    }

    public override string GetKey(KeyVaultSecret secret)
    {
        var key = secret.Name;

        if (key.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
        {
            key = key[Prefix.Length..];
        }

        key = key.Replace("--", ConfigurationPath.KeyDelimiter, StringComparison.Ordinal);

        return key switch
        {
            "AzureAI:Endpoint" => "AzureOpenAI:Endpoint",
            "AzureAI:ApiKey" => "AzureOpenAI:ApiKey",
            "AzureAI:DeploymentName" => "AzureOpenAI:ChatDeployment",
            "GoogleOAuth:ClientId" => "PoAppIdea:Authentication:Google:ClientId",
            "GoogleOAuth:ClientSecret" => "PoAppIdea:Authentication:Google:ClientSecret",
            "MicrosoftOAuth:ClientId" => "PoAppIdea:Authentication:Microsoft:ClientId",
            "MicrosoftOAuth:ClientSecret" => "PoAppIdea:Authentication:Microsoft:ClientSecret",
            _ => key
        };
    }
}
