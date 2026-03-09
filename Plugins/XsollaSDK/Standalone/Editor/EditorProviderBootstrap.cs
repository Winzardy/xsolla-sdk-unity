#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace Xsolla.Core.Editor
{
    internal static class EditorProviderBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            EditorProvider.Register(new EditorProviderBootstrapHandler());
        }

        private class EditorProviderBootstrapHandler : IEditorHandler
        {
            public string GetActiveBuildTargetAsString()
            {
                var target = EditorUserBuildSettings.activeBuildTarget;

                return target switch {
                    BuildTarget.StandaloneOSX            => "standaloneosx",
                    BuildTarget.StandaloneWindows        => "standalonewindows",
                    BuildTarget.StandaloneWindows64      => "standalonewindows64",
                    BuildTarget.StandaloneLinux64        => "standalonelinux64",
                    BuildTarget.WebGL                    => "webgl",
                    BuildTarget.WSAPlayer                => "wsaplayer",
                    BuildTarget.Android                  => "android",
                    BuildTarget.iOS                      => "ios",
                    BuildTarget.PS4                      => "ps4",
                    BuildTarget.XboxOne                  => "xboxone",
                    BuildTarget.tvOS                     => "tvos",
                    BuildTarget.Switch                   => "switch",
                    BuildTarget.GameCoreXboxOne          => "gamecorexboxone",
                    BuildTarget.PS5                      => "ps5",
                    BuildTarget.EmbeddedLinux            => "embeddedlinux",
                    BuildTarget.NoTarget                 => "notarget",
                    _                                    => target.ToString().ToLowerInvariant()
                };
            }
            
            public event Action<string> DeeplinkEvent;
            public string DeeplinkUrl { get; set; }

            public void SubscribeOnDeeplinkEvent(Action<string> callback)
            {
                DeeplinkEvent -= callback;
                DeeplinkEvent += callback;
            }

            public void UnsubscribeOnDeeplinkEvent(Action<string> callback) => DeeplinkEvent -= callback;
            public void OnDeeplinkEvent(string url) => DeeplinkEvent?.Invoke(url);
        }
    }
}

#endif