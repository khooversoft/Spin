using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Telemetry;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;


/// <summary>
/// Manages access control and authorization for graph nodes through principal and grant policy registries.
/// Provides thread-safe operations for determining read, write, and ownership permissions on graph nodes.
/// 
/// Access control follows a permission model where:
/// 
///    Nodes without policies are accessible to all (open access)
///    Nodes with policies require explicit principal inclusion for access
///    Write operations require Contributor or Owner role
///    Ownership operations require Owner role
/// 
/// All operations are thread-safe using <see cref="ReaderWriterLockSlim"/> for concurrent access.
/// </para>
/// </remarks>
public class GrantControl
{
    private readonly ILogger _logger;
    private readonly ReaderWriterLockSlim _slimLock;
    private readonly ITelemetry? _telemetry;
    private GraphCore? _graph = new();

    public GrantControl(ReaderWriterLockSlim slimLock, ILogger logger, ITelemetry? telemetry = null)
    {
        _logger = logger.NotNull();
        _slimLock = slimLock;
        _telemetry = telemetry;

        PrincipalRegistry = new PrincipalRegistry(() => _graph, slimLock, logger, telemetry);
        GrantPolicyRegistry = new GrantPolicyRegistry(() => _graph, slimLock, logger, telemetry);
    }

    public GrantControl(GraphSerialization graphSerialization, ReaderWriterLockSlim slimLock, ILogger logger, ITelemetry? telemetry = null)
        : this(slimLock, logger, telemetry)
    {
    }

    public GraphCore GetGraph() => _graph.NotNull("Graph has not been set");
    public void SetGraph(GraphCore graph) => _graph = graph;

    public PrincipalRegistry PrincipalRegistry { get; }
    public GrantPolicyRegistry GrantPolicyRegistry { get; }

    public bool CanRead(string nodeKey, string principalIdentifier)
    {
        var getOption = GrantPolicyRegistry.Get(nodeKey);
        if (getOption.IsError()) return true;

        var policies = getOption.Return();
        if( policies.CanRead(principalIdentifier)) return true; // User has direct access

        return false;
    }

    public bool CanWrite(string nodeKey, string principalIdentifier)
    {
        var getOption = GrantPolicyRegistry.Get(nodeKey);
        if (getOption.IsError()) return true;

        var policies = getOption.Return();
        if (policies.CanWrite(principalIdentifier)) return true; // User has direct access

        return false;
    }

    public bool IsOwner(string nodeKey, string principalIdentifier)
    {
        var getOption = GrantPolicyRegistry.Get(nodeKey);
        if (getOption.IsError()) return true;

        var policies = getOption.Return();
        if (policies.IsOwner(principalIdentifier)) return true; // User has direct access

        return false;
    }
}
