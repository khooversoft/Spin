using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools.Validation;

namespace SpinClusterApi.test.Application;

public record TestOption
{
    public string ClusterApiUri { get; init; } = null!;
}


public static class TestOptionExtensions
{
    public static IValidator<TestOption> Validator { get; } = new Validator<TestOption>()
        .RuleFor(x => x.ClusterApiUri).NotEmpty()
        .Build();

    public static TestOption Verify(this TestOption subject)
    {
        Validator.Validate(subject).ThrowOnError();
        return subject;
    }
}
