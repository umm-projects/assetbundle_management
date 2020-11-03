using System.IO;
using UnityModule.ContextManagement;

namespace UnityModule.AssetBundleManagement
{
    public sealed class AmazonCloudFrontURLResolver : URLResolverBase
    {
        private const int HashSubstringDigit = 2;

        private const string SingleManifestDirectoryName = "SingleManifests";

        private const string DynamicProjectContextListJsonName = "dynamic_project_context_list.json";

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
                        ResolveSingleManifestVersion().ToString()
                    );
            GenerateDynamicProjectContextListJson =
                () =>
                    Path.Combine(
                        AppendPathPrefix ? DefaultPathPrefix : string.Empty,
                        ContextManager.CurrentProject.Name,
                        GetPlatformPathName(),
                        DynamicProjectContextListJsonName
                    );
            AppendPathPrefix = true;
        }
    }
}