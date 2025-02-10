using Microsoft.Extensions.DependencyInjection;
using Toolbox.Azure.test.Application;
using Toolbox.Tools.Should;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Email.test;

public class SendEmailTest
{
    private readonly TestApplicationContext _appService;
    public SendEmailTest(ITestOutputHelper outputHelper) => _appService = TestApplication.Create<SendEmailTest>(outputHelper);

    [Fact]
    public async Task TestSendEmail()
    {
        IEmailSender emailSender = _appService.ServiceProvider.GetRequiredService<IEmailSender>();
        ScopeContext context = _appService.CreateContext<SendEmailTest>();

        var toEmail = ("khoover", "kelvin.hoover@hotmail.com");
        var result = await emailSender.SendHtml(toEmail, "Please join", "goto <a href=\"https://ticket-share.com\">Ticket Share</a>", context);
        result.IsOk().Should().BeTrue();
    }
}
