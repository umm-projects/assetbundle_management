using System.IO;

namespace UnityModule.AssetBundleManagement
{
    public class HttpsURLResolver: URLResolverBase
    {
        private const int HashSubstringDigit = 2;

        private const string SingleManifestDirectoryName = "SingleManifests";

        public HttpsURLResolver(string hostName, string pathPrefix)
        {
            GenerateProtocol = () => "https";
            GenerateHostname = () => hostName;
            GenerateAssetBundlePath =
                (snakeCaseProjectName, assetBundleName) =>
                {
                    var hashString = GetSingleManifest().GetAssetBundleHash(assetBundleName).ToString();
                    return Path.Combine(
                        pathPrefix,
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
                        pathPrefix,
                        AppendPathPrefix ? DefaultPathPrefix : string.Empty,
                        snakeCaseProjectName,
                        GetPlatformPathName(),
                        SingleManifestDirectoryName,
                        ResolveSingleManifestVersion().ToString()
                    );
            GenerateProjectPlatformFilePath =
                (snakeCaseProjectName, fileName) =>
                    Path.Combine(
                        pathPrefix,
                        AppendPathPrefix ? DefaultPathPrefix : string.Empty,
                        snakeCaseProjectName,
                        GetPlatformPathName(),
                        fileName
                    );
            AppendPathPrefix = true;
        }
    }
}
