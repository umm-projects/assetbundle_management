using System.IO;
using UnityEngine;

namespace UnityModule.AssetBundleManagement
{
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
}