using System.Text;
using System.Threading.Tasks;
using Toolbox.Application;
using Toolbox.Extensions;

namespace ArtifactCmd.Application
{
    internal record Option
    {
        public RunEnvironment Environment { get; init; }

        public string ApiKey { get; init; } = null!;

        public string ArtifactUrl { get; init; } = null!;

        public string Namespace { get; set; } = null!;

        public bool List { get; set; }

        public bool Get { get; set; }

        public bool Delete { get; set; }

        public bool Set { get; set; }

        public string File { get; set; } = null!;

        public string Id { get; set; } = null!;
    }
}
