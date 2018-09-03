using System;
using UnityEngine;

namespace UnityModule.Settings
{
    public class BuildAssetBundleSetting : Setting<BuildAssetBundleSetting>, IEnvironmentSetting
    {
        private const string EnvironmentVariableKeyBuildAssetBundleForceUpload = "BUILD_ASSETBUNDLE_FORCE_UPLOAD";

        [SerializeField] private bool forceUpload = Environment.GetEnvironmentVariable(EnvironmentVariableKeyBuildAssetBundleForceUpload) == "true";

        public bool ForceUpload => forceUpload;
    }
}