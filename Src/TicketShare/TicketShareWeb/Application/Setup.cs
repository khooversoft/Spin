//using Azure.Identity;
//using Microsoft.Extensions.Configuration.AzureAppConfiguration;
//using Toolbox.Azure.Identity;
//using Toolbox.Tools;

//namespace TicketShareWeb.Application;

//public static class Setup
//{
//    public static void AddApplicationConfiguration(this WebApplicationBuilder builder)
//    {
//        string connectionString = builder.Configuration.GetConnectionString("AppConfig").NotNull();
//        ClientSecretCredential credential = ClientCredential.ToClientSecretCredential(connectionString);

//        var appConfigEndpoint = "https://biz-bricks-prod-configuration.azconfig.io";

//        // Build configuration
//        builder.Configuration.AddAzureAppConfiguration(options =>
//        {
//            options.Connect(new Uri(appConfigEndpoint), credential)
//                .ConfigureKeyVault(kv =>
//                {
//                    kv.SetCredential(credential);
//                })
//                .Select(TicketShareConstants.ConfigurationFilter, LabelFilter.Null)
//                .Select(TicketShareConstants.ConfigurationFilter, builder.Environment.EnvironmentName);
//        });

//    }
//}
