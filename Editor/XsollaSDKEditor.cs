using UnityEditor;
using System.IO;
using UnityEngine;
using Xsolla.Core.Editor.AutoFillSettings;
using Xsolla.SDK.Common;

namespace Xsolla.SDK
{
    [InitializeOnLoad]
    public static class XsollaSDKEditor
    {
        const string AssetsPath = "Assets/";
        const string SettingsPath = AssetsPath + "Resources/";
        const string SettingsFileNameOld = "XsollaMobileSDKSettings";
        const string SettingsFileName = XsollaClientSettingsAsset.SettingsFileName;
        const string SettingsFullPathOld = SettingsPath + SettingsFileNameOld + ".asset";
        const string SettingsFullPath = SettingsPath + SettingsFileName + ".asset";
        
        static XsollaSDKEditor()
        {
            EditorApplication.delayCall += () =>
            {
                // migrate 2x to 3x settings
                if (File.Exists(SettingsFullPathOld) && !File.Exists(SettingsFullPath)) 
                {
                    Debug.Log($"[XsollaSDK] Migrate Asset: {SettingsFullPathOld} to {SettingsFullPath}");
                    
                    var oldInstance = Resources.Load(SettingsFileNameOld) as XsollaClientSettingsAsset;
                    var newInstance = XsollaClientSettingsAsset.Instantiate(oldInstance);
                    
                    AssetDatabase.CreateAsset(newInstance, SettingsFullPath);

                    if (AssetDatabase.DeleteAsset(SettingsFullPathOld))
                        Debug.Log($"[XsollaSDK] Migrate Asset: deleted old: " + SettingsFullPathOld);
                    else
                        Debug.LogWarning($"[XsollaSDK] Migrate Asset: Failed to delete asset: " + SettingsFullPathOld);
                    
                    AssetDatabase.Refresh();
                }
                
                
                if (File.Exists(SettingsFullPath)) 
                {
                    _instance = Resources.Load(SettingsFileName) as XsollaClientSettingsAsset;
                }
                /*else
                {
                    CreateDefaultSettingsFile();
                }*/
            };
        }
        
        private static XsollaClientSettingsAsset _instance; 
        
        static void CreateDefaultSettingsFile()
        {
            _instance = XsollaClientSettingsAsset.Instance();
            
            if (!Directory.Exists(SettingsPath))
                Directory.CreateDirectory(SettingsPath);

            AssetDatabase.CreateAsset(_instance, SettingsFullPath);
            AssetDatabase.Refresh();
        }
        
        [MenuItem("Window/Xsolla/SDK/Edit Settings", false, 1000)]
        public static void Edit()
        {
            if (_instance == null) 
                CreateDefaultSettingsFile();
            
            Selection.activeObject = _instance;
        }
        
        [MenuItem("Window/Xsolla/SDK/Auto Fill Settings", false, 1000)]
        public static void AutoFill()
        {
            if (_instance == null) 
                CreateDefaultSettingsFile();
            
            Selection.activeObject = _instance;
            
            AutoFillTool.OpenAutoFillTool((projectId, loginId, oauthId, redirectUrl) =>
            {
                _instance.settings.projectId = projectId;
                _instance.settings.loginId = loginId;
                _instance.settings.oauthClientId = oauthId;
                _instance.settings.redirectSettings.redirectUrl = redirectUrl;

                EditorUtility.SetDirty(_instance);
                AssetDatabase.SaveAssets();
            });
        }
    }
}