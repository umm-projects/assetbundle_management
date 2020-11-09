using System.IO;

namespace UnityModule.AssetBundleManagement
{
    public sealed class AmazonCloudFrontURLResolver : URLResolverBase
    {
        private const int HashSubstringDigit = 2;

        private const string SingleManifestDirectoryName = "SingleManifests";

        public AmazonCloudFrontURLResolver(string domainNamePrefix)
        {
            GenerateProtocol = () => "https";
            GenerateHostname = () => string.Format("{0}.cloudfront.net", domainNamePrefix);
            GenerateAssetBundlePath =
                (snakeCaseProjectName, assetBundleName) =>
                {
                    var hashString = GetSingleManifest().GetAssetBundleHash(assetBundleName).ToString();
                    return Path.Combine(
                        AppendPathPrefix ? DefaultPathPrefix : string.Empty,
                        snakeCaseProjectName,
                        GetPlatformPathName(),
                        hashString.Substring(0, HashSubstringDigit),
                        hashString
                    );
                };
            GenerateSingleManifestPath =
                (snakeCaseProjectName) =>
                    Path.Combine(
                        AppendPathPrefix ? DefaultPathPrefix : string.Empty,
                        snakeCaseProjectName,
                        GetPlatformPathName(),
                        SingleManifestDirectoryName,
                        ResolveSingleManifestVersion().ToString()
                    );
            GenerateProjectPlatformFilePath =
                (snakeCaseProjectName, fileName) =>
                    Path.Combine(
                        AppendPathPrefix ? DefaultPathPrefix : string.Empty,
                        snakeCaseProjectName,
                        GetPlatformPathName(),
                        fileName
                    );
            AppendPathPrefix = true;
        }
    }
}