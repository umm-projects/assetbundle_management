using System.IO;

namespace UnityModule.AssetBundleManagement
{
    public sealed class AmazonS3URLResolver : URLResolverBase
    {
        private const int HashSubstringDigit = 2;

        private const string SingleManifestDirectoryName = "SingleManifests";

        private const string DynamicProjectContextListJsonName = "dynamic_project_context_list.json";

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
                (snakeCaseProjectName, assetBundleName) =>
                {
                    var hashString = GetSingleManifest().GetAssetBundleHash(assetBundleName).ToString();
                    return Path.Combine(
                        UseS3Protocol ? string.Empty : BucketName,
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
                        UseS3Protocol ? string.Empty : BucketName,
                        AppendPathPrefix ? DefaultPathPrefix : string.Empty,
                        snakeCaseProjectName,
                        GetPlatformPathName(),
                        SingleManifestDirectoryName,
                        ResolveSingleManifestVersion().ToString()
                    );
            GenerateProjectPlatformFilePath =
                (snakeCaseProjectName, fileName) =>
                    Path.Combine(
                        UseS3Protocol ? string.Empty : BucketName,
                        AppendPathPrefix ? DefaultPathPrefix : string.Empty,
                        snakeCaseProjectName,
                        GetPlatformPathName(),
                        fileName
                    );
            AppendPathPrefix = true;
        }
    }
}