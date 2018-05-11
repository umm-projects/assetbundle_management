using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityModule.ContextManagement;

#pragma warning disable 649

namespace UnityModule.AssetBundleManagement
{
    [PublicAPI]
    public interface IDownloadableProjectContext : IProjectContext
    {
        int AssetBundleSingleManifestVersion { get; }

        string InitialSceneName { get; }

        IURLResolver AssetBundleURLResolverSingleManifest { get; }

        IURLResolver AssetBundleURLResolver { get; }
    }

    // XXX: VersionResolver 的なクラスを用意して、固定の値と RemoteConfig とで切り替えられるようにする
    [Serializable]
    [PublicAPI]
    public class DownloadableProjectContext : ProjectContext, IDownloadableProjectContext
    {
        [SerializeField] private int assetBundleSingleManifestVersion;

        public int AssetBundleSingleManifestVersion
        {
            get { return assetBundleSingleManifestVersion; }
        }

        [SerializeField] private string initialSceneName;

        public string InitialSceneName
        {
            get { return initialSceneName; }
        }

        public IURLResolver AssetBundleURLResolverSingleManifest { get; set; }

        public IURLResolver AssetBundleURLResolver { get; set; }
    }
}