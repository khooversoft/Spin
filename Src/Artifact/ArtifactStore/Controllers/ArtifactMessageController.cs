using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.sdk.Services;
using MessageNet.sdk.Host;
using MessageNet.sdk.Protocol;
using Toolbox.Tools;

namespace ArtifactStore.Controllers
{
    [MessageController("artifact")]
    public class ArtifactMessageController
    {
        private readonly IArtifactStoreFactory _acticleStoreFactory;

        public ArtifactMessageController(IArtifactStoreFactory acticleStoreFactory)
        {
            acticleStoreFactory.VerifyNotNull(nameof(acticleStoreFactory));

            _acticleStoreFactory = acticleStoreFactory;
        }

        [MessageGet()]
        public void Get(Message message)
        {
            message.VerifyNotNull(nameof(message));
        }


        [MessagePost()]
        public void Post(Message message)
        {

        }

        [MessageDelete()]
        public void Delete(Message message)
        {

        }
    }
}
