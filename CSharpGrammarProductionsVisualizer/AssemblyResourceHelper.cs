using System.IO;
using System.Reflection;

namespace CSharpGrammarProductionsVisualizer {
    static class AssemblyResourceHelper {
        public static string GetAsText(string resourceFullname) {
            if(string.IsNullOrWhiteSpace(resourceFullname)) {
                return null;
            }

            using(var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceFullname))
            using(var reader = new StreamReader(resourceStream)) {
                return reader.ReadToEnd();
            }
        }
    }
}
