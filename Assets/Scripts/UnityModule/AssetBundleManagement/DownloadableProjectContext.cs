using System;
using UnityEngine;
using UnityModule.ContextManagement;

#pragma warning disable 649

namespace UnityModule.AssetBundleManagement {

    public interface IDownloadableProjectContext : IProjectContext {

        int AssetBundleSingleManifestVersion { get; }

        IURLResolver AssetBundleURLResolverSingleManifest { get; set; }

        IURLResolver AssetBundleURLResolver { get; set; }

    }

    // XXX: VersionResolver 的なクラスを用意して、固定の値と RemoteConfig とで切り替えられるようにする
    [Serializable]
    public class DownloadableProjectContext : ProjectContext, IDownloadableProjectContext {

        [SerializeField]
        private int assetBundleSingleManifestVersion;

        public int AssetBundleSingleManifestVersion {
            get {
                return this.assetBundleSingleManifestVersion;
            }
        }

        public IURLResolver AssetBundleURLResolverSingleManifest { get; set; }

        public IURLResolver AssetBundleURLResolver { get; set; }

    }

}