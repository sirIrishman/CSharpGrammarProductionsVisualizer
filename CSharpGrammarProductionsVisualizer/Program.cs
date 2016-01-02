using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
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
            Process.Start(@"c:\Program Files (x86)\Google\Chrome\Application\chrome.exe", resultDocPath);
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
            var grammarSectionMark = doc.DocumentNode
                .SelectSingleNode("//a[@name='_Toc251613491']");

            return doc.DocumentNode
                .SelectNodes("//p[@style='margin-left: 1.91cm; text-indent: -0.64cm; margin-bottom: 0.21cm; line-height: 0.44cm; page-break-inside: avoid']")
                .Where(node => node.NodeType == HtmlNodeType.Element && node.Line >= grammarSectionMark.Line)
                .Select(node => CleanupGPHtmlNode(node))
                .GroupBy(node => node.ChildNodes.FindFirst("i").FirstChild.InnerText.Trim())
                .Select(duplicatedNodeGroup => MergeNodeGroup(duplicatedNodeGroup))
                //.OrderBy(node => node.InnerText.Trim())
                .Select(node => node.OuterHtml)
                .ToList();
        }

        static HtmlNode CleanupGPHtmlNode(HtmlNode gpNode) {
            Contract.Requires(gpNode != null);
            Contract.Requires(Contract.Result<HtmlNode>() != null);

            gpNode.Attributes.Remove("style");
            if(gpNode.Attributes.Contains("class")) {
                gpNode.Attributes["class"].Value += " symbol";
            } else {
                gpNode.Attributes.Add("class", "symbol");
            }
            return CleanupHtmlNode(gpNode);
        }
        static HtmlNode CleanupHtmlNode(HtmlNode node) {
            Contract.Requires(node != null);
            Contract.Requires(Contract.Result<HtmlNode>() != null);

            foreach(var langSpan in node.ChildNodes.Where(n => n.Name == "span" && n.Attributes.Contains("lang")).ToList()) {
                langSpan.ReplaceWithChildNodes();
            }
            foreach(var fontChildNode in node.ChildNodes.Where(n => n.Name == "font").ToList()) {
                var replacingNode = node.OwnerDocument.CreateElement("span");
                replacingNode.Attributes.Add("class", "terminal-symbol");
                replacingNode.InnerHtml = fontChildNode.FirstChild.InnerHtml; // the font node is doubled
                node.ChildNodes.Insert(node.ChildNodes.GetNodeIndex(fontChildNode), replacingNode);
                node.RemoveChild(fontChildNode);
            }
            var ellipsises = new List<string> { "&hellip;", "..." };
            foreach(var ellipsisLineNode in node.ChildNodes.Where(n => ellipsises.Contains(n.InnerText.Trim()) || n.Name == "br" && ellipsises.Contains(n.PreviousSibling?.InnerText?.Trim())).ToList()) {
                ellipsisLineNode.Remove();
            }
            foreach(var childNode in node.ChildNodes) {
                CleanupHtmlNode(childNode);
            }
            return node;
        }

        static HtmlNode MergeNodeGroup(IEnumerable<HtmlNode> nodeGroup) {
            Contract.Requires(nodeGroup != null);
            Contract.Requires(Contract.Result<HtmlNode>() != null);

            if(nodeGroup.Count() == 1) {
                return nodeGroup.Single();
            }
            var acceptor = nodeGroup.First();
            var donorGroupItems = nodeGroup
                .Skip(1)
                .Select(node => node.ChildNodes
                    .FindFirst("i")
                    .ChildNodes
                    .Skip(2)
                    .ToList()
                );
            var donorChildrenContainer = acceptor.ChildNodes.FindFirst("i");
            foreach(var groupItems in donorGroupItems) {
                donorChildrenContainer.AppendChild(donorChildrenContainer.OwnerDocument.CreateElement("br"));
                foreach(var item in groupItems) {
                    donorChildrenContainer.AppendChild(item);
                }
            }
            return acceptor;
        }

        static string BuildGPHtmlDocument(IList<string> gpNodes) {
            Contract.Requires(gpNodes != null);
            var builder = new StringBuilder();
            foreach(var node in gpNodes) {
                builder.Append(node).AppendLine();
            }
            string template = AssemblyResourceHelper.GetAsText("CSharpGrammarProductionsVisualizer.Template.html");
            template = template.Replace("<!--placeholder-->", builder.ToString());
            File.WriteAllText("result.html", template);
            return Path.GetFullPath("result.html");
        }
    }
}
