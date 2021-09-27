using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.sdk.Services;
using Toolbox.Tools;

namespace ArtifactStore.Controllers
{
    [MessageController]
    public class ArtifactMessageController
    {
        private readonly IArtifactStoreFactory _acticleStoreFactory;

        public ArtifactMessageController(IArtifactStoreFactory acticleStoreFactory)
        {
            acticleStoreFactory.VerifyNotNull(nameof(acticleStoreFactory));

            _acticleStoreFactory = acticleStoreFactory;
        }
    }
}
