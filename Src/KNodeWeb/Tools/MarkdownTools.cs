using Markdig;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KNodeWeb.Tools
{
    public class MarkdownTools
    {
        private readonly IWebHostEnvironment _environment;

        public MarkdownTools(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public string ReadDataFile(string filePath)
        {
            string fullFilePath = Path.Combine(_environment.ContentRootPath, filePath);
            return File.ReadAllText(fullFilePath);
        }

        public string Transform(string mdSource)
        {
            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();

            var result = Markdig.Markdown.ToHtml(mdSource, pipeline);

            return result;
        }
    }
}
