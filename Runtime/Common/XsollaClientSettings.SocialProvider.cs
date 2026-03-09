using System;

namespace Xsolla.SDK.Common
{
    public partial class XsollaClientSettings
    {
        [Serializable]
        public struct SocialProvider
        {
            /// <summary>Social provider's name (ID).</summary>
            /// <code>
            /// "google"
            /// "facebook"
            /// "twitter"
            /// "linkedin"
            /// "naver"
            /// "baidu"
            /// "amazon"
            /// "apple"
            /// "battlenet"
            /// "discord"
            /// "github"
            /// "kakao"
            /// "mailru"
            /// "microsoft"
            /// "msn"
            /// "ok"
            /// "paypal"
            /// "psn"
            /// "qq"
            /// "reddit"
            /// "steam"
            /// "twitch"
            /// "vimeo"
            /// "vk"
            /// "wechat"
            /// "weibo"
            /// "yahoo"
            /// "yandex"
            /// "youtube"
            /// "xbox"
            /// "babka"
            /// "epicgames"
            /// </code>
            public string name;

            public string accessToken;
        }
    }
}
