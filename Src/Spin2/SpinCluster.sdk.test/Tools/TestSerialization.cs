using SpinCluster.sdk.Actors.User;
using Toolbox.Tools;

namespace SpinCluster.sdk.test.Tools
{
    public class TestSerialization
    {
        [Fact]
        public void Test1()
        {
            string payload = """
                {
                    "UserId": "user/$system/user1@spin.com",
                    "Version": "UserPrincipal-v1",
                    "GlobalPrincipleId": "96f29049-f8a1-4106-8178-f426addd4b07",
                    "DisplayName": "user 1",
                    "FirstName": "user1",
                    "LastName": "user1-last",
                    "Email": "user1@tenant.com",
                    "Phone": [
                        {
                            "Type": "home",
                            "Number": "252-555-1200"
                        },
                        {
                            "Type": "work",
                            "Number": "252-555-1201"
                        }
                    ],
                    "Addresses": [
                        {
                            "Type": "home",
                            "Address1": "line1",
                            "Address2": null,
                            "City": "city1",
                            "State": "state1",
                            "ZipCode": "39393",
                            "Country": "USA"
                        }
                    ],
                    "DataObjects": [],
                    "AccountEnabled": false,
                    "CreatedDate": "2023-06-18T21:42:17.8543813Z",
                    "ActiveDate": null
                }
                """;

            UserModel user = Json.Default.Deserialize<UserModel>(payload).NotNull();
        }
    }
}