using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CSharpGrammarProductionsVisualizer {
    class Program {
        static void Main(string[] args) {
            string specDocPath = GetCSharpSpecificationPath(args);
            IList<string> gpNodes = ParseGPFromHtml(specDocPath);
            string resultDocPath = BuildGPHtmlDocument(gpNodes);
            Process.Start(resultDocPath);
        }

        static string GetCSharpSpecificationPath(string[] args) {
            if(args == null || args.Length == 0 || string.IsNullOrWhiteSpace(args.First())) {
                Console.Write("Give me a path to the C# spec (in the .html format of course):");
                return Console.ReadLine().Trim().Trim('"', '\'');
            } else {
                return args.First().Trim().Trim('"', '\'');
            }
        }
        static IList<string> ParseGPFromHtml(string htmlFilePath) {
            Contract.Requires(!string.IsNullOrWhiteSpace(htmlFilePath));
            var doc = new HtmlDocument();
            doc.Load(htmlFilePath);
            return doc.DocumentNode
                .SelectNodes("//p[@style='margin-left: 1.91cm; text-indent: -0.64cm; margin-bottom: 0.21cm; line-height: 0.44cm; page-break-inside: avoid']")
                .Select(node => node.OuterHtml)
                .ToList();
        }
        static string BuildGPHtmlDocument(IList<string> gpNodes) {
            Contract.Requires(gpNodes != null);
            var builder = new StringBuilder();
            foreach(var node in gpNodes) {
                builder.Append(node).AppendLine();
            }
            string template = GetTextFromEmbeddedResource("CSharpGrammarProductionsVisualizer.Template.html");
            template = template.Replace("<!--placeholder-->", builder.ToString());
            File.WriteAllText("result.html", template);
            return Path.GetFullPath("result.html");
        }
        static string GetTextFromEmbeddedResource(string name) {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            using(var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name)) {
                using(var reader = new StreamReader(resourceStream)) {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
