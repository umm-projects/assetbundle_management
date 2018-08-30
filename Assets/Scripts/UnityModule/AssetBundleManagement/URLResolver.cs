using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityModule.ContextManagement;

// ReSharper disable ConvertToAutoProperty
// ReSharper disable UseStringInterpolation
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

// ReSharper disable VirtualMemberNeverOverridden.Global
// ReSharper disable UnusedMember.Global

namespace UnityModule.AssetBundleManagement
{
    public interface IURLResolver
    {
        Uri ResolveSingleManifest();

        Uri Resolve(string assetBundleName);

        AssetBundleManifest GetSingleManifest();

        void SetSingleManifest(AssetBundleManifest assetBundleManifest);
    }

    public abstract class URLResolverBase : IURLResolver
    {
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

        protected virtual Func<string, string> GenerateAssetBundlePath { get; set; } = (assetBundleName) => "";

        protected virtual Func<string> GenerateSingleManifestPath { get; set; } = () => "";

        protected virtual bool AppendPathPrefix { get; set; } = true;

        private AssetBundleManifest SingleManifest { get; set; }

        public Uri ResolveSingleManifest()
        {
            return new UriBuilder
            {
                Scheme = GenerateProtocol(),
                Host = GenerateHostname(),
                Path = GenerateSingleManifestPath(),
            }.Uri;
        }

        public Uri Resolve(string assetBundleName)
        {
            return new UriBuilder
            {
                Scheme = GenerateProtocol(),
                Host = GenerateHostname(),
                Path = GenerateAssetBundlePath(assetBundleName),
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
    }

    public sealed class EditorURLResolver : URLResolverBase
    {
        public EditorURLResolver()
        {
            // file プロトコルの利用は推奨されていないが、エディタ実行という前提と、インタフェース統一のために妥協している。
            //   本来は AssetBundle.LoadFromFile() の利用が望ましいもよう。
            //   See also: https://unity3d.com/jp/learn/tutorials/topics/best-practices/assetbundle-fundamentals
            GenerateProtocol = () => "file";
            GenerateAssetBundlePath = (assetBundleName) => Path.Combine(Application.dataPath, PrependGeneralPathElements(assetBundleName));
            GenerateSingleManifestPath = () => Path.Combine(Application.dataPath, PrependGeneralPathElements(GetPlatformPathName()));
        }
    }

    public sealed class AmazonS3URLResolver : URLResolverBase
    {
        private const int HashSubstringDigit = 2;

        private const string SingleManifestDirectoryName = "SingleManifests";

        private string Region { get; set; }

        private string BucketName { get; set; }

        private bool UseS3Protocol { get; set; }

        public AmazonS3URLResolver(string region, string bucketName, bool useS3Protocol = false)
        {
            Region = region;
            BucketName = bucketName;
            UseS3Protocol = useS3Protocol;
            GenerateProtocol = () => UseS3Protocol ? "s3" : "https";
            GenerateHostname = () => UseS3Protocol ? BucketName : string.Format("s3-{0}.amazonaws.com", Region);
            GenerateAssetBundlePath =
                (assetBundleName) =>
                {
                    var hashString = GetSingleManifest().GetAssetBundleHash(assetBundleName).ToString();
                    return Path.Combine(
                        UseS3Protocol ? string.Empty : BucketName,
                        AppendPathPrefix ? DefaultPathPrefix : string.Empty,
                        ContextManager.CurrentProject.Name,
                        GetPlatformPathName(),
                        hashString.Substring(0, HashSubstringDigit),
                        hashString
                    );
                };
            GenerateSingleManifestPath =
                () =>
                    Path.Combine(
                        UseS3Protocol ? string.Empty : BucketName,
                        AppendPathPrefix ? DefaultPathPrefix : string.Empty,
                        ContextManager.CurrentProject.Name,
                        GetPlatformPathName(),
                        SingleManifestDirectoryName,
                        ContextManager.CurrentProject.As<IDownloadableProjectContext>().AssetBundleSingleManifestVersion.ToString()
                    );
            AppendPathPrefix = true;
        }
    }

    public sealed class AmazonCloudFrontURLResolver : URLResolverBase
    {
        private const int HashSubstringDigit = 2;

        private const string SingleManifestDirectoryName = "SingleManifests";

        public AmazonCloudFrontURLResolver(string domainNamePrefix)
        {
            GenerateProtocol = () => "https";
            GenerateHostname = () => string.Format("{0}.cloudfront.net", domainNamePrefix);
            GenerateAssetBundlePath =
                (assetBundleName) =>
                {
                    var hashString = GetSingleManifest().GetAssetBundleHash(assetBundleName).ToString();
                    return Path.Combine(
                        AppendPathPrefix ? DefaultPathPrefix : string.Empty,
                        ContextManager.CurrentProject.Name,
                        GetPlatformPathName(),
                        hashString.Substring(0, HashSubstringDigit),
                        hashString
                    );
                };
            GenerateSingleManifestPath =
                () =>
                    Path.Combine(
                        AppendPathPrefix ? DefaultPathPrefix : string.Empty,
                        ContextManager.CurrentProject.Name,
                        GetPlatformPathName(),
                        SingleManifestDirectoryName,
                        ContextManager.CurrentProject.As<IDownloadableProjectContext>().AssetBundleSingleManifestVersion.ToString()
                    );
            AppendPathPrefix = true;
        }
    }
}