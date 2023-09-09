﻿using SpinTestTools.sdk.ObjectBuilder.Builders;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinTestTools.sdk.ObjectBuilder;

public interface IObjectBuilder
{
    Task<Option> Delete(IServiceProvider service, ObjectBuilderOption option, ScopeContext context);
    Task<Option> Create(IServiceProvider service, ObjectBuilderOption option, ScopeContext context);
}

public class TestObjectBuilder
{
    public ObjectBuilderOption Option { get; set; } = null!;
    public IServiceProvider Service { get; set; } = null!;
    public IList<IObjectBuilder> Builders { get; } = new List<IObjectBuilder>();

    public TestObjectBuilder SetService(IServiceProvider service) => this.Action(x => Service = service);
    public TestObjectBuilder SetOption(ObjectBuilderOption option) => this.Action(x => Option = option);
    public TestObjectBuilder Add(params IObjectBuilder[] builders) => this.Action(x => builders.ForEach(x => Builders.Add(x)));

    public TestObjectBuilder AddStandard()
    {
        new IObjectBuilder[]
        {
            new SubscriptionBuilder(),
            new TenantBuilder(),
            new UserBuilder(),
            new AccountBuilder(),
            new LedgerItemBuilder(),
            new AgentBuilder(),
            new SmartcBuilder(),
        }.ForEach(x => Builders.Add(x));

        return this;
    }

    public async Task<Option> Build(ScopeContext context)
    {
        Verify();

        var test = new OptionTest();

        await test.TestAsync(async () => await DeleteAll(context));
        await test.TestAsync(async () => await CreateAll(context));

        return test;
    }

    private void Verify()
    {
        const string msg = "required";

        Option.NotNull(msg);
        Service.NotNull(msg);
        Builders.Count.Assert(x => x > 0, "No builder to execute");
    }

    public async Task<Option> DeleteAll(ScopeContext context)
    {
        Verify();

        var test = new OptionTest();

        foreach (var item in Builders.Reverse())
        {
            await test.TestAsync(async () => await item.Delete(Service, Option, context));
        }

        return test;
    }

    public async Task<Option> CreateAll(ScopeContext context)
    {
        Verify();

        var test = new OptionTest();

        foreach (var item in Builders)
        {
            await test.TestAsync(async () => await item.Create(Service, Option, context));
        }

        return test;
    }
}
