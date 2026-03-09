#if UNITY_IOS
using UnityEditor.Callbacks;
using UnityEditor;
using UnityEditor.iOS.Xcode;

using System.IO;
using UnityEditor.iOS.Xcode.Extensions;
using UnityEngine;

namespace Xsolla.SDK.Editor
{
    public class XsollaSDKXcodePostProcess
    {
        [PostProcessBuild]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string buildPath)
        {
            if (buildTarget != BuildTarget.iOS) return;

            AddUrlTypes(buildPath);
            AddFramework(buildPath);
        }
        
        static void AddUrlTypes(string buildPath)
        {
            string plistInfoPath = Path.Combine(buildPath, "Info.plist");
            PlistDocument plistInfo = new PlistDocument();
            plistInfo.ReadFromFile(plistInfoPath);

            var cfBundleURLTypes = plistInfo.root["CFBundleURLTypes"] != null 
                ? plistInfo.root["CFBundleURLTypes"].AsArray() 
                : plistInfo.root.CreateArray("CFBundleURLTypes");
            PlistElementDict urlTypeDict = cfBundleURLTypes.AddDict();
            urlTypeDict.SetString("CFBundleTypeRole", "Editor");
            PlistElementArray urlSchemesArray = urlTypeDict.CreateArray("CFBundleURLSchemes");
            urlSchemesArray.AddString("$(PRODUCT_BUNDLE_IDENTIFIER)");

            plistInfo.WriteToFile(plistInfoPath);
        }

        static void AddFramework(string buildPath)
        {
            string projectPath = PBXProject.GetPBXProjectPath(buildPath);
            PBXProject pbxProject = new PBXProject();
            pbxProject.ReadFromFile(projectPath);

            string unityFrameworkTarget = pbxProject.GetUnityFrameworkTargetGuid();
            string mainTarget = pbxProject.GetUnityMainTargetGuid();

            string searchRoot = Application.dataPath;
            string frameworkName = "XsollaMobileSDK.framework";

            string[] frameworksFound = Directory.GetDirectories(searchRoot, frameworkName, SearchOption.AllDirectories);

            if (frameworksFound.Length == 0)
            {
                // try searching in cache path
                string cacheRoot = Application.temporaryCachePath;
                frameworksFound = Directory.GetDirectories(cacheRoot, frameworkName, SearchOption.AllDirectories);
            }

            if (frameworksFound.Length == 0)
            {
                string packageCacheRoot = Path.Combine(Application.dataPath, "../Library/PackageCache");
                frameworksFound = Directory.GetDirectories(packageCacheRoot, frameworkName, SearchOption.AllDirectories);
            }

            if (frameworksFound.Length == 0)
            {
                Debug.LogError($"[PostProcess] {frameworkName} not found in project.");
                return;
            }

            string frameworkSourcePath = frameworksFound[0];

            string frameworksDestinationFolder = Path.Combine(buildPath, "Frameworks");
            if (!Directory.Exists(frameworksDestinationFolder))
            {
                Directory.CreateDirectory(frameworksDestinationFolder);
            }

            string finalFrameworkPath = Path.Combine(frameworksDestinationFolder, Path.GetFileName(frameworkSourcePath));
            
            CopyAndReplaceDirectory(frameworkSourcePath, finalFrameworkPath);

            string fileGuid = pbxProject.AddFile(
                Path.Combine("Frameworks", Path.GetFileName(frameworkSourcePath)),
                Path.Combine("Frameworks", Path.GetFileName(frameworkSourcePath)),
                PBXSourceTree.Source
            );

            pbxProject.AddFileToBuild(unityFrameworkTarget, fileGuid);

            pbxProject.AddFileToEmbedFrameworks(mainTarget, fileGuid);
            
            File.WriteAllText(projectPath, pbxProject.WriteToString());
        }
        
        static void CopyAndReplaceDirectory(string srcPath, string dstPath)
        {
            if (Directory.Exists(dstPath))
            {
                Directory.Delete(dstPath, true);
            }
            if (File.Exists(dstPath))
            {
                File.Delete(dstPath);
            }
            Directory.CreateDirectory(dstPath);

            foreach (var file in Directory.GetFiles(srcPath))
            {
                File.Copy(file, Path.Combine(dstPath, Path.GetFileName(file)), true);
            }

            foreach (var dir in Directory.GetDirectories(srcPath))
            {
                CopyAndReplaceDirectory(dir, Path.Combine(dstPath, Path.GetFileName(dir)));
            }
        }
    }
}
#endif