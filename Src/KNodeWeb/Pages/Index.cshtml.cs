using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KNodeWeb.Tools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace KNodeWeb.Pages
{
    public class IndexModel : PageModel
    {
        private readonly MarkdownTools _markdownTool;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(MarkdownTools markdownTool, ILogger<IndexModel> logger)
        {
            _markdownTool = markdownTool;
            _logger = logger;
        }

        [BindProperty]
        public string MdHtml { get; set; }

        public void OnGet()
        {
            string mdSource = _markdownTool.ReadDataFile("pages\\data\\main.md");
            MdHtml = _markdownTool.Transform(mdSource);
        }
    }
}
