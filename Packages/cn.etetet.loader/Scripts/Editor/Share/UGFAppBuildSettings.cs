using UnityEditor;
using UnityEngine;
using YooAsset;
using YooAsset.Editor;

namespace ET
{
    [FilePath("ProjectSettings/UGFAppBuildSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class UGFAppBuildSettings : ScriptableSingleton<UGFAppBuildSettings>
    {
        public string PackageName = "DefaultPackage";
        public string PackageVersion = string.Empty;
        public string PlayerBuildDir = "Release";

        public EBuildPipeline BuildPipeline = EBuildPipeline.BuiltinBuildPipeline;
        public ECompressOption CompressOption = ECompressOption.LZ4;
        public EFileNameStyle FileNameStyle = EFileNameStyle.HashName;
        public EBuildinFileCopyOption BuildinFileCopyOption = EBuildinFileCopyOption.ClearAndCopyAll;

        public bool ClearBuildCacheFiles;
        public bool UseAssetDependencyDB;
        public bool BuildHotfixDll = true;
        public bool CopyAotDlls;
        public bool BuildYooAssets = true;
        public bool BuildPlayer = true;
        public bool SwitchToClientCodeMode = true;
        public bool RevealOutput = true;
        public BuildOptions PlayerBuildOptions;

        public void Save()
        {
            Save(true);
        }
    }
}
