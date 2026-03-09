namespace Xsolla.SDK.Login
{
    /// <summary>
    /// Delegate representing a function that handles an error message.
    /// </summary>
    /// <param name="error">A string describing the error that occurred.</param>
    public delegate void ErrorFunc(string error);

    /// <summary>
    /// Delegate representing a function that handles a successful login result.
    /// </summary>
    /// <param name="token">The login token returned upon successful authentication.</param>
    public delegate void LoginResultFunc(XsollaLoginToken token);

    /// <summary>
    /// Delegate representing a function called after a token is successfully cleared.
    /// </summary>
    public delegate void ClearTokenResultFunc();

    /// <summary>
    /// Delegate representing a function that provides the dismiss URL for a WebView session.
    /// </summary>
    /// <param name="url">The WebView dismiss URL data.</param>
    public delegate void WebViewDismissUrlFunc(XsollaLoginClientWebViewDismissUrl url);
}