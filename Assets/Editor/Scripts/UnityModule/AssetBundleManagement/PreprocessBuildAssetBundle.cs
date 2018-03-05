using System.IO;
using SimpleBuild;
using UnityEditor;
using UnityEngine;

namespace UnityModule.AssetBundleManagement {

    public class PreprocessBuildAssetBundle : IPreprocessBuildAssetBundle {

        public int callbackOrder => 0;

        public void OnPreprocessBuildAssetBundle(BuildTarget buildTarget, string path) {
            if (!AssetBundleBuildOptions.HasKeepBuiltAssetBundles()) {
                AssetDatabase.DeleteAsset(path);
            }
            string fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", path));
            if (!Directory.Exists(fullPath)) {
                Directory.CreateDirectory(fullPath);
            }
        }

    }

}