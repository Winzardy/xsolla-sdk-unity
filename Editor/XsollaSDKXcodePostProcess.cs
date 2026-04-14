#if UNITY_IOS
using UnityEditor.Callbacks;
using UnityEditor;
using UnityEditor.iOS.Xcode;

using System.IO;

namespace Xsolla.SDK.Editor
{
    public class XsollaSDKXcodePostProcess
    {
        const string PackageUrl = "https://github.com/xsolla/xsolla-sdk-ios";
        const string PackageVersion = "3.9.0";
        const string PackageProduct = "XsollaMobileSDK";

        [PostProcessBuild]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string buildPath)
        {
            if (buildTarget != BuildTarget.iOS) return;

            AddUrlTypes(buildPath);
            AddSwiftPackage(buildPath);
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

        static void AddSwiftPackage(string buildPath)
        {
            string projectPath = PBXProject.GetPBXProjectPath(buildPath);
            PBXProject pbxProject = new PBXProject();
            pbxProject.ReadFromFile(projectPath);

            string mainTarget = pbxProject.GetUnityMainTargetGuid();
            string frameworkTarget = pbxProject.GetUnityFrameworkTargetGuid();

            string packageGuid = pbxProject.AddRemotePackageReferenceAtVersionUpToNextMinor(PackageUrl, PackageVersion);
            //pbxProject.AddRemotePackageFrameworkToProject(mainTarget, PackageProduct, packageGuid, false);
            pbxProject.AddRemotePackageFrameworkToProject(frameworkTarget, PackageProduct, packageGuid, false);
            AddSignatureCleanupPhase(pbxProject, frameworkTarget);

            pbxProject.WriteToFile(projectPath);
        }

        const string PhaseName = "Fix Xsolla Signature Collision";
        const string ShellPath = "/bin/sh";
        const string ShellScript = @"if [ ""${XCODE_VERSION_MAJOR:-0}"" -ge ""1500"" ]; then
            echo ""[Xsolla] Removing duplicate signature files""
            find ""$BUILD_DIR/$CONFIGURATION-iphoneos"" -name ""XsollaMobileSDK.xcframework-ios.signature"" -type f -delete 2>/dev/null || true
            fi";

        static void AddSignatureCleanupPhase(PBXProject pbxProject, string frameworkTarget)
        {
            var existing = pbxProject.GetShellScriptBuildPhaseForTarget(
                frameworkTarget, PhaseName, ShellPath, ShellScript);

            if (string.IsNullOrEmpty(existing))
            {
                pbxProject.AddShellScriptBuildPhaseBeforeTargetPostprocess(
                    frameworkTarget, PhaseName, ShellPath, ShellScript);
            }
        }
    }
}
#endif