using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityModule.ContextManagement;

namespace UnityModule.AssetBundleManagement
{
    public abstract class URLResolverBase : IURLResolver
    {
        private const string RemoteSettingKeyFormat = "SingleManifestVersion{0}{1}";

        protected const string DefaultPathPrefix = "AssetBundles";

        private static readonly Dictionary<RuntimePlatform, string> PlatformNameMap =
            new Dictionary<RuntimePlatform, string>()
            {
                {RuntimePlatform.IPhonePlayer, "iOS"},
                {RuntimePlatform.Android, "Android"},
                {RuntimePlatform.LinuxEditor, "Standalone"},
                {RuntimePlatform.OSXEditor, "Standalone"},
                {RuntimePlatform.WindowsEditor, "Standalone"},
            };

#if UNITY_EDITOR
        private static readonly Dictionary<BuildTarget, string> PlatformNameMapForEditor =
            new Dictionary<BuildTarget, string>()
            {
                {BuildTarget.iOS, "iOS"},
                {BuildTarget.Android, "Android"},
            };
#endif

        protected virtual Func<string> GenerateProtocol { get; set; } = () => "";

        protected virtual Func<string> GenerateHostname { get; set; } = () => "";

        protected virtual Func<string, string, string> GenerateAssetBundlePath { get; set; } = (snakeCaseProjectName, assetBundleName) => "";

        protected virtual Func<string, string> GenerateSingleManifestPath { get; set; } = (snakeCaseProjectName) => "";

        protected virtual Func<string, string, string> GenerateProjectPlatformFilePath { get; set; } = (snakeCaseProjectName, fileName) => "";

        protected virtual bool AppendPathPrefix { get; set; } = true;

        private AssetBundleManifest SingleManifest { get; set; }

        /// <param name="projectName">e.g. project_name</param>
        public Uri ResolveSingleManifest(string projectName)
        {
            return new UriBuilder
            {
                Scheme = GenerateProtocol(),
                Host = GenerateHostname(),
                Path = GenerateSingleManifestPath(projectName),
            }.Uri;
        }

        public Uri Resolve(string projectName, string assetBundleName)
        {
            return new UriBuilder
            {
                Scheme = GenerateProtocol(),
                Host = GenerateHostname(),
                Path = GenerateAssetBundlePath(projectName, assetBundleName),
            }.Uri;
        }

        public Uri ResolveProjectPlatformFile(string projectName, string fileName)
        {
            return new UriBuilder
            {
                Scheme = GenerateProtocol(),
                Host = GenerateHostname(),
                Path = GenerateProjectPlatformFilePath(projectName, fileName),
            }.Uri;
        }

        public AssetBundleManifest GetSingleManifest()
        {
            return SingleManifest;
        }

        public void SetSingleManifest(AssetBundleManifest assetBundleManifest)
        {
            SingleManifest = assetBundleManifest;
        }

        protected virtual string PrependGeneralPathElements(string path)
        {
            return Path.Combine(AppendPathPrefix ? DefaultPathPrefix : string.Empty, GetPlatformPathName(), path);
        }

        protected static string GetPlatformPathName()
        {
#if UNITY_EDITOR
            // PostprocessBuildAssetBundle などで iOS/Android 向けの URL を作成するなどの処理が必要になるため、現在向いている Platform を正として処理する
            if (PlatformNameMapForEditor.ContainsKey(EditorUserBuildSettings.activeBuildTarget))
            {
                return PlatformNameMapForEditor[EditorUserBuildSettings.activeBuildTarget];
            }
#endif
            return PlatformNameMap[Application.platform];
        }

        protected static int ResolveSingleManifestVersion()
        {
            var version = ContextManager.CurrentProject.As<IDownloadableProjectContext>()
                .AssetBundleSingleManifestVersion;
            var projectName = ContextManager.CurrentProject.SceneNamePrefix.TrimEnd('_');
            var delimiter = string.IsNullOrEmpty(projectName) ? string.Empty : "-";
            return Math.Max(version,
                RemoteSettings.GetInt(string.Format(RemoteSettingKeyFormat, delimiter, projectName), version));
        }
    }
}