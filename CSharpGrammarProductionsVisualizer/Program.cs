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

            Console.Write("Parsing...");
            IList<string> gpNodes = ParseGPFromHtml(specDocPath);
            Console.WriteLine(" Done");

            Console.Write("Building...");
            string resultDocPath = BuildGPHtmlDocument(gpNodes);
            Console.WriteLine(" Done");

            Console.Write("Opening...");
            Process.Start(resultDocPath);
            Console.WriteLine(" Done");
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
                .Select(node => CleanupGPNodeHtml(node).OuterHtml)
                .ToList();
        }
        static HtmlNode CleanupGPNodeHtml(HtmlNode gpNode) {
            Contract.Requires(gpNode != null);
            gpNode.Attributes.Remove("style");
            gpNode.Attributes.Add("class", "symbol");
            return CleanupNodeHtml(gpNode);
        }
        static HtmlNode CleanupNodeHtml(HtmlNode node) {
            Contract.Requires(node != null);
            foreach(var fontChildNode in node.ChildNodes.Where(n => n.Name.Equals("font", StringComparison.OrdinalIgnoreCase)).ToList()) {
                var replacingNode = node.OwnerDocument.CreateElement("span");
                replacingNode.Attributes.Add("class", "terminal-symbol");
                replacingNode.InnerHtml = fontChildNode.FirstChild.InnerHtml; // the font node is doubled
                node.ChildNodes.Insert(node.ChildNodes.GetNodeIndex(fontChildNode), replacingNode);
                node.RemoveChild(fontChildNode);
            }
            foreach(var childNode in node.ChildNodes) {
                CleanupNodeHtml(childNode);
            }
            return node;
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
