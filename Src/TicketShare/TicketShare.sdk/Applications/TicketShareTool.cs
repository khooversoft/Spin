using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace TicketShare.sdk;

public static class TicketShareTool
{
    public static string ToPartnershipKey(string id) => $"partnership:{id.NotEmpty().ToLower()}";
}
