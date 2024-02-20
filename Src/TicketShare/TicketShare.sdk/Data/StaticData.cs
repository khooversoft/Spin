//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;
//using TicketShare.sdk.Tools;
//using Toolbox.Tools;
//using Toolbox.Extensions;

//namespace TicketShare.sdk;

//public static class StaticData
//{
//    public static string GetIntro() => AssemblyResource.GetResourceString("TicketShare.sdk.Data.Intro.md", typeof(StaticData));
//    public static MarkdownDoc GetIntroDoc() => GetIntro().ToMarkdownDoc();

//    public static string GetIntroHtml()
//    {
//        string html = AssemblyResource.GetResourceString("TicketShare.sdk.Data.Intro.html", typeof(StaticData));

//        if (html.IsEmpty()) return html;

//        var replacements = ((string searchFor, string replaceWith)[])[
//            ("src=\"SharringTickets.webp\"", "src=\"data:image/webp;base64," + GetImage("SharringTickets").GetBase64() + "\""),
//            ("src=\"Fans.webp\"", "src=\"data:image/webp;base64," + GetImage("Fans").GetBase64() + "\""),
//            ("src=\"FansTrading.webp\"", "src=\"data:image/webp;base64," + GetImage("FansTrading").GetBase64() + "\""),
//            ("src=\"FanHappy.webp\"", "src=\"data:image/webp;base64," + GetImage("FanHappy").GetBase64() + "\""),
//        ];

//        foreach(var replacement in replacements)
//        {
//            html = html.Replace(replacement.searchFor, replacement.replaceWith);
//        }

//        return html;
//    }

//    private static StaticImage GetImage(string resourceId) => new StaticImage
//    {
//        Name = resourceId,
//        Data = AssemblyResource.GetResourceBytes($"TicketShare.sdk.Data.{resourceId}.webp", typeof(StaticData))!
//    };
//}
