using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Orleans.TestingHost;
using SoftBank.sdk.Models;
using SoftBank.sdk.test.Application;
using SpinCluster.sdk.Application;
using Toolbox.Types;

namespace SoftBank.sdk.test;

public class StandardTransactions : IClassFixture<ClusterFixture>
{
    private readonly TestCluster _cluster;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

    public StandardTransactions(ClusterFixture fixture)
    {
        _cluster = fixture.Cluster;
    }

    [Fact]
    public async Task CreateBankAccount()
    {
        string ownerId = "signKey@test.com";
        string objectId = $"{SpinConstants.Schema.SoftBank}/test/SignAndVerify/{ownerId}";

        var request = new AccountDetail
        {
        };
    }
}
