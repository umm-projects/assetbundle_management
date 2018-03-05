using UnityEditor;

namespace UnityModule.AssetBundleManagement {

    public interface IAssetBundleNameResolver {

        string Resolve(string guid);

    }

    public class DefaultAssetBundleNameResolver : IAssetBundleNameResolver {

        public string Resolve(string guid) {
            return $"{AssetDatabase.GUIDToAssetPath(guid).ToLower()}{Constants.ASSET_BUNDLE_EXTENSION}";
        }

    }

}