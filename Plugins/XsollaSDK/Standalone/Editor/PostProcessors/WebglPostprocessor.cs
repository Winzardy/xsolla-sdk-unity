#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Compilation;
using UnityEngine;

namespace Xsolla.Core.Editor
{
    internal class WebglPostprocessor : IPostprocessBuildWithReport
    {
        const string SOURCE_PROXY_PAGE_FILENAME = "widget-proxy-page.html";
        const string DEST_PROXY_PAGE_FILENAME = "xl-widget.html";

        public int callbackOrder => 1000;

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.WebGL)
                return;

            XDebug.Log("SDK is now processing build", true);

            var sourcePath = GetSourceFilePath();
            XDebug.Log($"Source path: {sourcePath}");

            if (!File.Exists(sourcePath))
                throw new FileNotFoundException($"'{SOURCE_PROXY_PAGE_FILENAME}' not found at: " + sourcePath);

            var destPath = Path.Combine(report.summary.outputPath, DEST_PROXY_PAGE_FILENAME);
            XDebug.Log("Destination path: " + destPath);

            if (File.Exists(destPath))
                File.Delete(destPath);

            File.Copy(sourcePath, destPath);
        }

        private static string GetSourceFilePath()
        {
            var assemblyName = typeof(WebglPostprocessor).Assembly.GetName().Name;
            var assembly = CompilationPipeline
                .GetAssemblies(AssembliesType.Editor)
                .FirstOrDefault(assembly => assembly.name == assemblyName);

            if (assembly == null)
                throw new FileNotFoundException($"Cannot resolve assembly for {typeof(WebglPostprocessor).FullName}");

            var scriptPath = assembly.sourceFiles.FirstOrDefault(sf =>
                string.Equals(Path.GetFileName(sf), "WebglPostprocessor.cs", System.StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrEmpty(scriptPath))
                throw new FileNotFoundException("Could not locate WebglPostprocessor.cs in its declaring assembly.");

            var dir = Path.GetDirectoryName(scriptPath);
            if (string.IsNullOrEmpty(dir))
                throw new DirectoryNotFoundException("Could not resolve script directory.");

            return Path.Combine(Path.Combine(dir, "Resources"), "widget-proxy-page.html");
        }
    }
}
#endif
