using HtmlAgilityPack;
using System.Diagnostics.Contracts;

namespace CSharpGrammarProductionsVisualizer {
    static class HtmlNodeExtensions {
        public static void ReplaceWithChildNodes(this HtmlNode nodeToReplace) {
            Contract.Requires(nodeToReplace != null);

            var parent = nodeToReplace.ParentNode;
            var insertAfter = nodeToReplace;
            if(insertAfter == null) {
                parent.PrependChildren(nodeToReplace.ChildNodes);
            } else {
                foreach(var child in nodeToReplace.ChildNodes) {
                    parent.InsertAfter(child, insertAfter);
                    insertAfter = child;
                }
            }
            nodeToReplace.Remove();
        }
    }
}
