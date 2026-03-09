using UnityEngine;

namespace Xsolla.SDK.Common
{
    /// <summary>
    /// ScriptableObject that stores and manages the Xsolla client settings.
    /// </summary>
    public class XsollaClientSettingsAsset : ScriptableObject
    {
        /// <summary>
        /// The filename used to locate the settings asset in the Resources folder.
        /// </summary>
        public const string SettingsFileName = "XsollaSDKSettings";

        /// <summary>
        /// The actual settings object used by the Xsolla SDK.
        /// This field is serialized to enable editing via the Unity Inspector.
        /// </summary>
        [SerializeField]
        public XsollaClientSettings settings = XsollaClientSettings.Builder.Empty();

        /// <summary>
        /// Serializes this settings asset to a JSON string.
        /// </summary>
        /// <returns>A JSON string representation of this settings asset.</returns>
        private string ToJson() => XsollaClientHelpers.ToJson(this);

        /// <summary>
        /// Returns a JSON string representation of this object.
        /// </summary>
        /// <returns>A JSON-formatted string.</returns>
        public override string ToString() => ToJson();

        /// <summary>
        /// Default constructor for the XsollaClientSettingsAsset class.
        /// </summary>
        public XsollaClientSettingsAsset() { }

        /// <summary>
        /// Attempts to load the settings asset from the Unity Resources folder.
        /// If not found, a new instance is created.
        /// </summary>
        /// <returns>
        /// An existing instance of <see cref="XsollaClientSettingsAsset"/> from Resources,
        /// or a newly created instance if none was found.
        /// </returns>
        protected static XsollaClientSettingsAsset TryToLoad()
        {
            var inst = Resources.Load(SettingsFileName) as XsollaClientSettingsAsset;
            if (inst == null) inst = Create();
            return inst;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="XsollaClientSettingsAsset"/> in memory.
        /// </summary>
        /// <returns>A new instance of <see cref="XsollaClientSettingsAsset"/>.</returns>
        private static XsollaClientSettingsAsset Create() => CreateInstance<XsollaClientSettingsAsset>();

        /// <summary>
        /// Returns an instance of the settings asset, loading it if necessary.
        /// </summary>
        /// <returns>An instance of <see cref="XsollaClientSettingsAsset"/>.</returns>
        public static XsollaClientSettingsAsset Instance() => TryToLoad();
    }
}
