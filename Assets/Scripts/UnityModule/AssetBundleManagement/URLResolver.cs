using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityModule.ContextManagement;

// ReSharper disable VirtualMemberNeverOverridden.Global
// ReSharper disable UnusedMember.Global

namespace UnityModule.AssetBundleManagement {

    public interface IURLResolver {

        string Resolve();

        string Resolve(string assetBundleName);

        AssetBundleManifest GetSingleManifest();

        void SetSingleManifest(AssetBundleManifest assetBundleManifest);

    }

    public abstract class URLResolverBase : IURLResolver {

        protected const string DEFAULT_PATH_PREFIX = "AssetBundles";

        private static readonly Dictionary<RuntimePlatform, string> PLATFORM_NAME_MAP = new Dictionary<RuntimePlatform, string>() {
            { RuntimePlatform.IPhonePlayer , "iOS" },
            { RuntimePlatform.Android      , "Android" },
            { RuntimePlatform.LinuxEditor  , "Standalone" },
            { RuntimePlatform.OSXEditor    , "Standalone" },
            { RuntimePlatform.WindowsEditor, "Standalone" },
        };

        protected virtual Func<string> ProtocolResolver { get; set; } = () => "";

        protected virtual Func<string> HostnameResolver { get; set; } = () => "";

        protected virtual Func<string, string> PathResolver { get; set; } = (assetBundleName) => "";

        protected virtual bool AppendPathPrefix { get; set; } = true;

        private AssetBundleManifest SingleManifest { get; set; }

        public string Resolve() {
            return this.Resolve(null);
        }

        public string Resolve(string assetBundleName) {
            return new UriBuilder {
                Scheme = this.ProtocolResolver(),
                Host = this.HostnameResolver(),
                Path = this.PathResolver(assetBundleName),
            }.ToString();
        }

        public AssetBundleManifest GetSingleManifest() {
            return this.SingleManifest;
        }

        public void SetSingleManifest(AssetBundleManifest assetBundleManifest) {
            this.SingleManifest = assetBundleManifest;
        }

        protected virtual string DefaultPathResolver(string assetBundleName) {
            return string.IsNullOrEmpty(assetBundleName) ? this.CreateSingleManifestPath() : this.CreatePath(assetBundleName);
        }

        protected virtual string CreateSingleManifestPath() {
            return Path.Combine(this.AppendPathPrefix ? DEFAULT_PATH_PREFIX : string.Empty, GetPlatformPathName(), GetPlatformPathName());
        }

        protected virtual string CreatePath(string assetBundleName) {
            return Path.Combine(this.AppendPathPrefix ? DEFAULT_PATH_PREFIX : string.Empty, GetPlatformPathName(), assetBundleName);
        }

        protected static string GetPlatformPathName() {
            return PLATFORM_NAME_MAP[Application.platform];
        }

    }

    public sealed class EditorURLResolver : URLResolverBase {

        public EditorURLResolver() {
            // file プロトコルの利用は推奨されていないが、エディタ実行という前提と、インタフェース統一のために妥協している。
            //   本来は AssetBundle.LoadFromFile() の利用が望ましいもよう。
            //   See also: https://unity3d.com/jp/learn/tutorials/topics/best-practices/assetbundle-fundamentals
            this.ProtocolResolver = () => "file";
            this.PathResolver = this.DefaultPathResolver;
        }

        protected override string DefaultPathResolver(string assetBundleName) {
            return Path.Combine(Application.dataPath, base.DefaultPathResolver(assetBundleName));
        }

    }

    public sealed class AmazonS3URLResolver : URLResolverBase {

        private const int HASH_SUBSTRING_DIGIT = 2;

        private const string SINGLE_MANIFEST_DIRECTORY_NAME = "SingleManifests";

        private string Region { get; }

        private string BucketName { get; }

        public AmazonS3URLResolver(string region, string bucketName) {
            this.Region = region;
            this.BucketName = bucketName;
            this.ProtocolResolver = () => "https";
            this.HostnameResolver = () => $"s3-{this.Region}.amazonaws.com";
            this.PathResolver = this.DefaultPathResolver;
            this.AppendPathPrefix = true;
        }

        protected override string CreateSingleManifestPath() {
            return Path.Combine(
                this.BucketName,
                this.AppendPathPrefix ? DEFAULT_PATH_PREFIX : string.Empty,
                ContextManager.CurrentProject.Name,
                GetPlatformPathName(),
                SINGLE_MANIFEST_DIRECTORY_NAME,
                ContextManager.CurrentProject.As<IDownloadableProjectContext>().AssetBundleSingleManifestVersion.ToString()
            );
        }

        protected override string CreatePath(string assetBundleName) {
            string hashString = this.GetSingleManifest().GetAssetBundleHash(assetBundleName).ToString();
            return Path.Combine(
                this.BucketName,
                this.AppendPathPrefix ? DEFAULT_PATH_PREFIX : string.Empty,
                ContextManager.CurrentProject.Name,
                GetPlatformPathName(),
                hashString.Substring(0, HASH_SUBSTRING_DIGIT),
                hashString
            );
        }

    }

    public sealed class AmazonCloudFrontURLResolver : URLResolverBase {

        private const int HASH_SUBSTRING_DIGIT = 2;

        private const string SINGLE_MANIFEST_DIRECTORY_NAME = "SingleManifests";

        public AmazonCloudFrontURLResolver(string domainNamePrefix) {
            this.ProtocolResolver = () => "https";
            this.HostnameResolver = () => $"{domainNamePrefix}.cloudfront.net";
            this.PathResolver = this.DefaultPathResolver;
            this.AppendPathPrefix = true;
        }

        protected override string CreateSingleManifestPath() {
            return Path.Combine(
                this.AppendPathPrefix ? DEFAULT_PATH_PREFIX : string.Empty,
                ContextManager.CurrentProject.Name,
                GetPlatformPathName(),
                SINGLE_MANIFEST_DIRECTORY_NAME,
                ContextManager.CurrentProject.As<IDownloadableProjectContext>().AssetBundleSingleManifestVersion.ToString()
            );
        }

        protected override string CreatePath(string assetBundleName) {
            string hashString = this.GetSingleManifest().GetAssetBundleHash(assetBundleName).ToString();
            return Path.Combine(
                this.AppendPathPrefix ? DEFAULT_PATH_PREFIX : string.Empty,
                ContextManager.CurrentProject.Name,
                GetPlatformPathName(),
                hashString.Substring(0, HASH_SUBSTRING_DIGIT),
                hashString
            );
        }

    }

}