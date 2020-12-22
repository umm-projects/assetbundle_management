using System;
using UnityEngine;

// ReSharper disable ConvertToAutoProperty
// ReSharper disable UseStringInterpolation
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

// ReSharper disable VirtualMemberNeverOverridden.Global
// ReSharper disable UnusedMember.Global

namespace UnityModule.AssetBundleManagement
{
    public interface IURLResolver
    {
        Uri ResolveSingleManifest(string projectName);

        Uri Resolve(string projectName, string assetBundleName);

        /// <summary>
        /// Resolve file depends on project and platform
        /// </summary>
        /// <param name="projectName">the code name of project. e.g. "myproject_sample1"</param>
        /// <param name="fileName">target file name. e.g. sample.json</param>
        /// <returns>e.g. https://example.com/myproject_sample1/Android/sample.json </returns>
        Uri ResolveProjectPlatformFile(string projectName, string fileName);

        AssetBundleManifest GetSingleManifest();

        void SetSingleManifest(AssetBundleManifest assetBundleManifest);
    }
}
