using System;
using UnityEngine;
using UnityModule.ContextManagement;

#pragma warning disable 649

namespace UnityModule.AssetBundleManagement {

    public interface IDownloadableProjectContext : IProjectContext {

        int AssetBundleSingleManifestVersion { get; }

        IURLResolver AssetBundleURLResolverSingleManfest { get; set; }

        IURLResolver AssetBundleURLResolverNormal { get; set; }

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

        public IURLResolver AssetBundleURLResolverSingleManfest { get; set; }

        public IURLResolver AssetBundleURLResolverNormal { get; set; }

    }

}