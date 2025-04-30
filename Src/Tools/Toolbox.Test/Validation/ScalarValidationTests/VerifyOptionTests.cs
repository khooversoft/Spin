using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Validation.ScalarValidationTests;

public class VerifyOptionTests
{
    [Fact]
    public void Be()
    {
        Option v1 = StatusCode.OK;
        v1.Be(StatusCode.OK);

        Option<string> v2 = StatusCode.OK;
        v2.Be(StatusCode.OK);

        Verify.Throw<ArgumentException>(() =>
        {
            Option v3 = StatusCode.BadRequest;
            v3.Be(StatusCode.OK);
        });

        Verify.Throw<ArgumentException>(() =>
        {
            Option<string> v3 = StatusCode.BadRequest;
            v3.Be(StatusCode.OK);
        });
    }

    [Fact]
    public void NotBe()
    {
        Option v1 = StatusCode.OK;
        v1.NotBe(StatusCode.BadRequest);

        Option<string> v2 = StatusCode.OK;
        v2.NotBe(StatusCode.BadRequest);

        Verify.Throw<ArgumentException>(() =>
        {
            Option v3 = StatusCode.BadRequest;
            v3.NotBe(StatusCode.BadRequest);
        });

        Verify.Throw<ArgumentException>(() =>
        {
            Option<string> v3 = StatusCode.BadRequest;
            v3.NotBe(StatusCode.BadRequest);
        });
    }

    [Fact]
    public void BeOk()
    {
        Option v1 = StatusCode.OK;
        v1.BeOk();

        Option<string> v2 = StatusCode.OK;
        v2.BeOk();

        Verify.Throw<ArgumentException>(() =>
        {
            Option v2 = StatusCode.BadRequest;
            v2.BeOk();
        });

        Verify.Throw<ArgumentException>(() =>
        {
            Option<string> v3 = StatusCode.BadRequest;
            v3.BeOk();
        });
    }

    [Fact]
    public void BeError()
    {
        Option v1 = StatusCode.Conflict;
        v1.BeError();

        Option<string> v2 = StatusCode.NotFound;
        v2.BeError();

        Verify.Throw<ArgumentException>(() =>
        {
            Option v2 = StatusCode.OK;
            v2.BeError();
        });

        Verify.Throw<ArgumentException>(() =>
        {
            Option<int> v2 = StatusCode.OK;
            v2.BeError();
        });
    }
}
