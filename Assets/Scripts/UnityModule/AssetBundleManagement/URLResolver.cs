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

namespace UnityModule.AssetBundleManagement {

    public interface IURLResolver {

        Uri Resolve();

        Uri Resolve(string assetBundleName);

        AssetBundleManifest GetSingleManifest();

        void SetSingleManifest(AssetBundleManifest assetBundleManifest);

    }

    public abstract class URLResolverBase : IURLResolver {

        protected const string DefaultPathPrefix = "AssetBundles";

        private static readonly Dictionary<RuntimePlatform, string> PlatformNameMap = new Dictionary<RuntimePlatform, string>() {
            { RuntimePlatform.IPhonePlayer , "iOS" },
            { RuntimePlatform.Android      , "Android" },
            { RuntimePlatform.LinuxEditor  , "Standalone" },
            { RuntimePlatform.OSXEditor    , "Standalone" },
            { RuntimePlatform.WindowsEditor, "Standalone" },
        };

#if UNITY_EDITOR
        private static readonly Dictionary<BuildTarget, string> PlatformNameMapForEditor = new Dictionary<BuildTarget, string>() {
            { BuildTarget.iOS    , "iOS" },
            { BuildTarget.Android, "Android" },
        };
#endif

        private Func<string> protocolResolver;

        protected virtual Func<string> ProtocolResolver {
            get {
                return protocolResolver ?? (protocolResolver = () => "");
            }
            set {
                protocolResolver = value;
            }
        }

        private Func<string> hostnameResolver;

        protected virtual Func<string> HostnameResolver {
            get {
                return hostnameResolver ?? (hostnameResolver = () => "");
            }
            set {
                hostnameResolver = value;
            }
        }

        private Func<string, string> pathResolver;

        protected virtual Func<string, string> PathResolver {
            get {
                return pathResolver ?? (pathResolver = (assetBundleName) => "");
            }
            set {
                pathResolver = value;
            }
        }

        private bool appendPathPrefix = true;

        protected virtual bool AppendPathPrefix {
            get {
                return appendPathPrefix;
            }
            set {
                appendPathPrefix = value;
            }
        }

        private AssetBundleManifest SingleManifest { get; set; }

        public Uri Resolve() {
            return Resolve(null);
        }

        public Uri Resolve(string assetBundleName) {
            return new UriBuilder {
                Scheme = ProtocolResolver(),
                Host = HostnameResolver(),
                Path = PathResolver(assetBundleName),
            }.Uri;
        }

        public AssetBundleManifest GetSingleManifest() {
            return SingleManifest;
        }

        public void SetSingleManifest(AssetBundleManifest assetBundleManifest) {
            SingleManifest = assetBundleManifest;
        }

        protected virtual string DefaultPathResolver(string assetBundleName) {
            return string.IsNullOrEmpty(assetBundleName) ? CreateSingleManifestPath() : CreatePath(assetBundleName);
        }

        protected virtual string CreateSingleManifestPath() {
            return Path.Combine(AppendPathPrefix ? DefaultPathPrefix : string.Empty, GetPlatformPathName(), GetPlatformPathName());
        }

        protected virtual string CreatePath(string assetBundleName) {
            return Path.Combine(AppendPathPrefix ? DefaultPathPrefix : string.Empty, GetPlatformPathName(), assetBundleName);
        }

        protected static string GetPlatformPathName() {
#if UNITY_EDITOR
            // PostprocessBuildAssetBundle などで iOS/Android 向けの URL を作成するなどの処理が必要になるため、現在向いている Platform を正として処理する
            if (PlatformNameMapForEditor.ContainsKey(EditorUserBuildSettings.activeBuildTarget)) {
                return PlatformNameMapForEditor[EditorUserBuildSettings.activeBuildTarget];
            }
#endif
            return PlatformNameMap[Application.platform];
        }

    }

    public sealed class EditorURLResolver : URLResolverBase {

        public EditorURLResolver() {
            // file プロトコルの利用は推奨されていないが、エディタ実行という前提と、インタフェース統一のために妥協している。
            //   本来は AssetBundle.LoadFromFile() の利用が望ましいもよう。
            //   See also: https://unity3d.com/jp/learn/tutorials/topics/best-practices/assetbundle-fundamentals
            ProtocolResolver = () => "file";
            PathResolver = DefaultPathResolver;
        }

        protected override string DefaultPathResolver(string assetBundleName) {
            return Path.Combine(Application.dataPath, base.DefaultPathResolver(assetBundleName));
        }

    }

    public sealed class AmazonS3URLResolver : URLResolverBase {

        private const int HashSubstringDigit = 2;

        private const string SingleManifestDirectoryName = "SingleManifests";

        private string Region { get; set; }

        private string BucketName { get; set; }

        private bool UseS3Protocol { get; set; }

        public AmazonS3URLResolver(string region, string bucketName, bool useS3Protocol = false) {
            Region = region;
            BucketName = bucketName;
            UseS3Protocol = useS3Protocol;
            ProtocolResolver = () => UseS3Protocol ? "s3" : "https";
            HostnameResolver = () => UseS3Protocol ? BucketName : string.Format("s3-{0}.amazonaws.com", Region);
            PathResolver = DefaultPathResolver;
            AppendPathPrefix = true;
        }

        protected override string CreateSingleManifestPath() {
            return Path.Combine(
                UseS3Protocol ? string.Empty : BucketName,
                AppendPathPrefix ? DefaultPathPrefix : string.Empty,
                ContextManager.CurrentProject.Name,
                GetPlatformPathName(),
                SingleManifestDirectoryName,
                ContextManager.CurrentProject.As<IDownloadableProjectContext>().AssetBundleSingleManifestVersion.ToString()
            );
        }

        protected override string CreatePath(string assetBundleName) {
            string hashString = GetSingleManifest().GetAssetBundleHash(assetBundleName).ToString();
            return Path.Combine(
                UseS3Protocol ? string.Empty : BucketName,
                AppendPathPrefix ? DefaultPathPrefix : string.Empty,
                ContextManager.CurrentProject.Name,
                GetPlatformPathName(),
                hashString.Substring(0, HashSubstringDigit),
                hashString
            );
        }

    }

    public sealed class AmazonCloudFrontURLResolver : URLResolverBase {

        private const int HashSubstringDigit = 2;

        private const string SingleManifestDirectoryName = "SingleManifests";

        public AmazonCloudFrontURLResolver(string domainNamePrefix) {
            ProtocolResolver = () => "https";
            HostnameResolver = () => string.Format("{0}.cloudfront.net", domainNamePrefix);
            PathResolver = DefaultPathResolver;
            AppendPathPrefix = true;
        }

        protected override string CreateSingleManifestPath() {
            return Path.Combine(
                AppendPathPrefix ? DefaultPathPrefix : string.Empty,
                ContextManager.CurrentProject.Name,
                GetPlatformPathName(),
                SingleManifestDirectoryName,
                ContextManager.CurrentProject.As<IDownloadableProjectContext>().AssetBundleSingleManifestVersion.ToString()
            );
        }

        protected override string CreatePath(string assetBundleName) {
            string hashString = GetSingleManifest().GetAssetBundleHash(assetBundleName).ToString();
            return Path.Combine(
                AppendPathPrefix ? DefaultPathPrefix : string.Empty,
                ContextManager.CurrentProject.Name,
                GetPlatformPathName(),
                hashString.Substring(0, HashSubstringDigit),
                hashString
            );
        }

    }

}