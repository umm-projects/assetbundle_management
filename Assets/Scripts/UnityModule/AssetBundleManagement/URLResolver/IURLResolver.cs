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
        Uri ResolveSingleManifest();

        Uri Resolve(string assetBundleName);
        
        Uri ResolveDynamicProjectContextListJson();

        AssetBundleManifest GetSingleManifest();

        void SetSingleManifest(AssetBundleManifest assetBundleManifest);
    }
}