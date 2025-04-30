using Toolbox.Tools;
using Toolbox.Tools.Should;
using Toolbox.Types;

namespace Toolbox.Test.Validation.Should;

public class ShouldOptionTests
{
    [Fact]
    public void BeOk()
    {
        Option v1 = StatusCode.OK;
        v1.Should().BeOk();

        Verify.Throw<ArgumentException>(() =>
        {
            Option v2 = StatusCode.BadRequest;
            v2.Should().BeOk();
        });
    }

    [Fact]
    public void BeOkOfType()
    {
        var r = new object();

        Option<object> v1 = StatusCode.OK;
        v1.Should().BeOk();

        Verify.Throw<ArgumentException>(() =>
        {
            Option<object> v2 = StatusCode.BadRequest;
            v2.Should().BeOk();
        });
    }

    [Fact]
    public void BeError()
    {
        Option v1 = StatusCode.Conflict;
        v1.Should().BeError();

        Verify.Throw<ArgumentException>(() =>
        {
            Option v2 = StatusCode.OK;
            v2.Should().BeError();
        });
    }

    [Fact]
    public void BeErrorOfType()
    {
        var r = new object();

        Option<object> v1 = StatusCode.Conflict;
        v1.Should().BeError();

        Verify.Throw<ArgumentException>(() =>
        {
            Option<object> v2 = StatusCode.OK;
            v2.Should().BeError();
        });
    }

    [Fact]
    public void BeNotFound()
    {
        Option v1 = StatusCode.NotFound;
        v1.Should().BeNotFound();

        Verify.Throw<ArgumentException>(() =>
        {
            Option v2 = StatusCode.OK;
            v2.Should().BeNotFound();
        });
    }

    [Fact]
    public void BeNotFoundOfType()
    {
        var r = new object();

        Option<object> v1 = StatusCode.NotFound;
        v1.Should().BeNotFound();

        Verify.Throw<ArgumentException>(() =>
        {
            Option<object> v2 = StatusCode.OK;
            v2.Should().BeNotFound();
        });
    }

    [Fact]
    public void BeConflict()
    {
        Option v1 = StatusCode.Conflict;
        v1.Should().BeConflict();

        Verify.Throw<ArgumentException>(() =>
        {
            Option v2 = StatusCode.OK;
            v2.Should().BeConflict();
        });
    }

    [Fact]
    public void BeConflictOfType()
    {
        var r = new object();

        Option<object> v1 = StatusCode.Conflict;
        v1.Should().BeConflict();

        Verify.Throw<ArgumentException>(() =>
        {
            Option<object> v2 = StatusCode.OK;
            v2.Should().BeConflict();
        });
    }

    [Fact]
    public void BeBadRequest()
    {
        Option v1 = StatusCode.BadRequest;
        v1.Should().BeBadRequest();

        Verify.Throw<ArgumentException>(() =>
        {
            Option v2 = StatusCode.OK;
            v2.Should().BeBadRequest();
        });
    }

    [Fact]
    public void BeBadRequestOfType()
    {
        var r = new object();

        Option<object> v1 = StatusCode.BadRequest;
        v1.Should().BeBadRequest();

        Verify.Throw<ArgumentException>(() =>
        {
            Option<object> v2 = StatusCode.OK;
            v2.Should().BeBadRequest();
        });
    }
}
