//using ArtifactStore.sdk.Model;
//using Identity.sdk.Types;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Toolbox.Tools;

//namespace Identity.sdk.Models
//{
//    public record Subscription
//    {
//        public IdentityId TenantId { get; set; } = null!;

//        public IdentityId SubscriptionId { get; set; } = null!;

//        public string Name { get; set; } = null!;

//        public static ArtifactId ToArtifactId(IdentityId tenantId, IdentityId subscriptionId)
//        {
//            tenantId.VerifyNotNull(nameof(tenantId));
//            subscriptionId.VerifyNotNull(nameof(subscriptionId));

//            return new ArtifactId($"subscription/{tenantId}/{subscriptionId}");
//        }
//    }
//}
