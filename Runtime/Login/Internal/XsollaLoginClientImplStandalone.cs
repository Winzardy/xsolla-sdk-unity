#if UNITY_STANDALONE || UNITY_WEBGL || UNITY_EDITOR //&& DISABLED_TMP
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Xsolla.Auth;
using Xsolla.Core;
using Xsolla.SDK.Common;
using Xsolla.UserAccount;
using Xsolla.SDK.Utils;

namespace Xsolla.SDK.Login
{
    internal class XsollaLoginClientImplStandalone : IXsollaLoginClient
    {
        private const string Tag = "XsollaLoginClientImplSDK";
        private XsollaSettings _settings;

        public void Login(XsollaClientConfiguration configuration, LoginResultFunc onSuccess, ErrorFunc onError)
        {
            _settings = FillFromConfiguration(configuration);
            
            XsollaLogger.Debug(Tag, "Login");

            LoginSilently(configuration, onSuccess: onSuccess, onError: error =>
            {
                XsollaLogger.Debug(Tag, $"LoginSilently onError: {error}, try to login with widget");
                
                var locale = configuration.GetCurrentLocale();
                XsollaLogger.Debug(Tag, $"CurrentLocale: {locale}");
                
                // In Editor there is no way to use login widget, try to login with device ID
                if (Application.isEditor && string.IsNullOrEmpty(EditorProvider.Handler?.DeeplinkUrl))
                {
                    XsollaAuth.AuthViaDeviceID(
                        _settings,
                        onSuccess: () =>
                        {
                            XsollaLogger.Debug(Tag, "Login onSuccess");
                            onSuccess?.Invoke(TokenCurrent(_settings));
                        },
                        onError: error =>
                        {
                            XsollaLogger.Debug(Tag, $"Authenticate onError {error}");
                            onError?.Invoke(error.ToString());
                        }
                    );
                    return;
                }
  
                // If silent login failed, try to login with widget
                XsollaAuth.AuthWithXsollaWidget(
                    _settings,
                    onSuccess: () =>
                    {
                        XsollaLogger.Debug(Tag, "Login onSuccess");
                        onSuccess?.Invoke(TokenCurrent(_settings));
                    },
                    onError: error =>
                    {
                        XsollaLogger.Debug(Tag, $"Login onError {error}");
                        onError?.Invoke(error.ToString());
                    },
                    onCancel: () =>
                    {
                        XsollaLogger.Debug(Tag, "Login onCancel");
                        onError?.Invoke("Cancel");
                    },
                    locale: locale?.ToString()
                );
            });
        }

        public void LoginSilently(XsollaClientConfiguration configuration, LoginResultFunc onSuccess, ErrorFunc onError)
        {
            _settings = FillFromConfiguration(configuration);
            
            XsollaLogger.Debug(Tag, "LoginSilently");

            XsollaAuth.AuthViaXsollaLauncher(
                _settings,
                onSuccess: () =>
                {
                    XsollaLogger.Debug(Tag, "Authenticate Via Launcher onSuccess");
                    onSuccess?.Invoke(TokenCurrent(_settings));
                },
                onError: error =>
                {
                    XsollaLogger.Debug(Tag, $"Authenticate Via Launcher onError {error}, trying to auth by saved token");
                    
                    XsollaAuth.AuthBySavedToken(
                        _settings,
                        onSuccess: () =>
                        {
                            XsollaLogger.Debug(Tag, "Authenticate By Saved Token onSuccess");
                            onSuccess?.Invoke(TokenCurrent(_settings));
                        },
                        onError: error =>
                        {
                            XsollaLogger.Debug(Tag, $"Authenticate By Saved Token onError: {error}");
                            onError?.Invoke($"Failed to login silently ({error})");
                        }
                    );  
                }
            );
        }
        
        public void LoginWithSocialAccount(XsollaClientConfiguration configuration, string provider, string accountToken, LoginResultFunc onSuccess, ErrorFunc onError)
        {
            _settings = FillFromConfiguration(configuration);
            
            XsollaAuth.AuthWithSocialNetworkAccessToken(
                _settings, 
                accessToken: accountToken, accessTokenSecret: null, openId: null, 
                provider: provider,
                onSuccess: () =>
                {
                    XsollaLogger.Debug(Tag, "LoginWithXsollaAccount onSuccess");
                    onSuccess?.Invoke(TokenCurrent(_settings));
                },
                onError: error =>
                {
                    XsollaLogger.Debug(Tag, $"LoginWithXsollaAccount onError {error}");
                    onError?.Invoke(error.ToString());
                }
            );
            
            
        }

        public void ClearToken(XsollaClientConfiguration configuration, ClearTokenResultFunc onSuccess, ErrorFunc onError) 
        {
            _settings = FillFromConfiguration(configuration);
            
            _settings.XsollaToken.DeleteSavedInstance();
            onSuccess?.Invoke();
        }

        public bool CanRefreshToken =>
            (_settings.OAuthClientId != -1 && _settings.OAuthClientId != 0) && 
            !string.IsNullOrEmpty(_settings.XsollaToken.RefreshToken);

        public void RefreshToken(XsollaClientConfiguration configuration, XsollaLoginToken token, LoginResultFunc onSuccess, ErrorFunc onError)
        {
            _settings = FillFromConfiguration(configuration);
            
            if (_settings.OAuthClientId != -1 && _settings.OAuthClientId != 0) {
                if (!string.IsNullOrEmpty(_settings.XsollaToken.RefreshToken)) {
                    XsollaAuth.RefreshToken(
                        _settings,
                        onSuccess: () => {
                            XsollaLogger.Debug(Tag, $"Refresh onSuccess: accessToken={_settings.XsollaToken.AccessToken}");
                            XsollaLogger.Debug(Tag, $"Refresh onSuccess: refreshToken={_settings.XsollaToken.RefreshToken}");
                            XsollaLogger.Debug(Tag, $"Refresh onSuccess: expirationTime={_settings.XsollaToken.ExpirationTime}");

                            onSuccess?.Invoke(TokenCurrent(_settings));
                        },
                        onError: err => {
                            XsollaLogger.Debug(Tag, $"Refresh onError {err}");

                            onError?.Invoke(err.ToString());
                        }
                    );
                } else {
                    onError?.Invoke("Refresh token is absent, cannot initiate the access token refresh.");
                }
            } else {
                onError?.Invoke("The access token refresh requires OAuth2 authentication.");
            }
        }

        private static XsollaSettings FillFromConfiguration(XsollaClientConfiguration configuration)
        {
            Info.SDK_NAME = Common.Constants.SDK_NAME;
            Info.SDK_VERSION = Common.Constants.SDK_VERSION;

            var settings = new XsollaSettings();
            
            settings.LogLevel = configuration.logLevel switch
            {
                XsollaLogLevel.Debug => LogLevel.InfoWarningsErrors,
                XsollaLogLevel.Warning => LogLevel.WarningsErrors,
                XsollaLogLevel.Error => LogLevel.Errors,
                _ => LogLevel.Errors
            };
            
            var callback = XsollaLogger.GetOnLogCallback();
            if (callback != null) {
                XDebug.SetOnLogCallback( (lvl, msg) =>
                {
                    callback?.Invoke(
                        lvl switch
                        {
                            LogLevel.InfoWarningsErrors => XsollaLogLevel.Debug,
                            LogLevel.WarningsErrors => XsollaLogLevel.Warning,
                            LogLevel.Errors => XsollaLogLevel.Error,
                            _ => XsollaLogLevel.None
                        },
                        msg
                    );
                });
            }
            
            settings.StoreProjectId = configuration.settings.projectId.ToString();
            settings.IsSandbox = configuration.sandbox;
            settings.LoginId = configuration.settings.loginId;
            settings.OAuthClientId = configuration.settings.oauthClientId;
            
            settings.InAppBrowserEnabled = false;

            return settings;
        }
        
        [NotNull]
        static XsollaLoginToken TokenCurrent(XsollaSettings settings) => new XsollaLoginToken(
            accessToken: settings.XsollaToken.AccessToken,
            refreshToken: settings.XsollaToken.RefreshToken,
            expirationDate: (settings.OAuthClientId != -1 &&  settings.OAuthClientId != 0) ? settings.XsollaToken.ExpirationTime : 0
        );
        
    }
}

#endif