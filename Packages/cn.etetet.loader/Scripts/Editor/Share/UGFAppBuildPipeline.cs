using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using YooAsset;
using YooAsset.Editor;

namespace ET
{
    public static class UGFAppBuildPipeline
    {
        private const string AppInitScene = "Packages/cn.etetet.statesync/Scenes/Init.unity";
        private const string GlobalConfigPath = "Packages/com.etetet.init/Resources/GlobalConfig.asset";

        public static void BuildHotfixDlls()
        {
            Debug.Log("[UGF App Builder] Build hotfix DLLs start.");
            AssemblyTool.DoCompile();
            EnsureHotfixDllsExist();
            Debug.Log("[UGF App Builder] Build hotfix DLLs success.");
        }

        public static void CopyAotDlls()
        {
            Debug.Log("[UGF App Builder] Copy AOT DLLs start.");
            HybridCLREditor.CopyAotDll();
            Debug.Log("[UGF App Builder] Copy AOT DLLs success.");
        }

        public static YooAsset.Editor.BuildResult BuildYooAssets(UGFAppBuildSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            string packageName = settings.PackageName;
            EnsureYooPackageExists(packageName);

            string packageVersion = GetPackageVersion(settings);
            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
            Debug.Log($"[UGF App Builder] Build YooAsset start. Package={packageName}, Version={packageVersion}, Target={buildTarget}, Pipeline={settings.BuildPipeline}");

            BuildParameters buildParameters = CreateBuildParameters(settings, packageVersion, buildTarget);
            YooAsset.Editor.BuildResult result = RunYooPipeline(settings.BuildPipeline, buildParameters);
            if (!result.Success)
            {
                throw new BuildFailedException($"YooAsset build failed. Task={result.FailedTask}\n{result.ErrorInfo}");
            }

            Debug.Log($"[UGF App Builder] Build YooAsset success. Output={result.OutputPackageDirectory}");
            return result;
        }

        public static UnityEditor.Build.Reporting.BuildReport BuildPlayer(UGFAppBuildSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            PrepareCodeMode(settings);
            AssetDatabase.Refresh();

            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup targetGroup = UnityEditor.BuildPipeline.GetBuildTargetGroup(target);
            string locationPathName = GetPlayerLocationPath(target, settings.PlayerBuildDir);
            Directory.CreateDirectory(Path.GetDirectoryName(locationPathName));

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { AppInitScene },
                target = target,
                targetGroup = targetGroup,
                locationPathName = locationPathName,
                options = settings.PlayerBuildOptions
            };

            Debug.Log($"[UGF App Builder] Build Player start. Target={target}, Output={locationPathName}");
            UnityEditor.Build.Reporting.BuildReport report = UnityEditor.BuildPipeline.BuildPlayer(options);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                throw new BuildFailedException($"Player build failed: {report.summary.result}");
            }

            Debug.Log($"[UGF App Builder] Build Player success. Output={locationPathName}");
            if (settings.RevealOutput)
            {
                EditorUtility.OpenWithDefaultApp(Path.GetDirectoryName(locationPathName));
            }

            return report;
        }

        public static void BuildAll(UGFAppBuildSettings settings)
        {
            if (settings.BuildHotfixDll)
            {
                BuildHotfixDlls();
            }

            if (settings.CopyAotDlls)
            {
                CopyAotDlls();
            }

            if (settings.BuildYooAssets)
            {
                BuildYooAssets(settings);
            }

            if (settings.BuildPlayer)
            {
                BuildPlayer(settings);
            }
        }

        public static string GetPackageVersion(UGFAppBuildSettings settings)
        {
            if (!string.IsNullOrWhiteSpace(settings.PackageVersion))
            {
                return settings.PackageVersion.Trim();
            }

            return DateTime.Now.ToString("yyyy-MM-dd-HHmm");
        }

        private static BuildParameters CreateBuildParameters(UGFAppBuildSettings settings, string packageVersion, BuildTarget target)
        {
            if (settings.BuildPipeline == EBuildPipeline.BuiltinBuildPipeline)
            {
                return CreateBuiltinParameters(settings, packageVersion, target);
            }

            if (settings.BuildPipeline == EBuildPipeline.ScriptableBuildPipeline)
            {
                return CreateScriptableParameters(settings, packageVersion, target);
            }

            throw new BuildFailedException($"UGF App Builder first version only supports BuiltinBuildPipeline and ScriptableBuildPipeline: {settings.BuildPipeline}");
        }

        private static BuiltinBuildParameters CreateBuiltinParameters(UGFAppBuildSettings settings, string packageVersion, BuildTarget target)
        {
            return FillCommonParameters(new BuiltinBuildParameters
            {
                CompressOption = settings.CompressOption
            }, settings, packageVersion, target);
        }

        private static ScriptableBuildParameters CreateScriptableParameters(UGFAppBuildSettings settings, string packageVersion, BuildTarget target)
        {
            ScriptableBuildParameters parameters = FillCommonParameters(new ScriptableBuildParameters
            {
                CompressOption = settings.CompressOption,
                BuiltinShadersBundleName = GetBuiltinShaderBundleName(settings.PackageName)
            }, settings, packageVersion, target);
            return parameters;
        }

        private static T FillCommonParameters<T>(T parameters, UGFAppBuildSettings settings, string packageVersion, BuildTarget target)
            where T : BuildParameters
        {
            parameters.BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
            parameters.BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
            parameters.BuildPipeline = settings.BuildPipeline.ToString();
            parameters.BuildBundleType = (int)EBuildBundleType.AssetBundle;
            parameters.BuildTarget = target;
            parameters.PackageName = settings.PackageName;
            parameters.PackageVersion = packageVersion;
            parameters.EnableSharePackRule = true;
            parameters.VerifyBuildingResult = true;
            parameters.FileNameStyle = settings.FileNameStyle;
            parameters.BuildinFileCopyOption = settings.BuildinFileCopyOption;
            parameters.BuildinFileCopyParams = AssetBundleBuilderSetting.GetPackageBuildinFileCopyParams(settings.PackageName, settings.BuildPipeline.ToString());
            parameters.ClearBuildCacheFiles = settings.ClearBuildCacheFiles;
            parameters.UseAssetDependencyDB = settings.UseAssetDependencyDB;
            parameters.EncryptionServices = CreateEncryptionInstance(settings.PackageName, settings.BuildPipeline);
            return parameters;
        }

        private static YooAsset.Editor.BuildResult RunYooPipeline(EBuildPipeline pipeline, BuildParameters parameters)
        {
            if (pipeline == EBuildPipeline.BuiltinBuildPipeline)
            {
                return new BuiltinBuildPipeline().Run(parameters, true);
            }

            if (pipeline == EBuildPipeline.ScriptableBuildPipeline)
            {
                return new ScriptableBuildPipeline().Run(parameters, true);
            }

            throw new BuildFailedException($"Unsupported YooAsset build pipeline: {pipeline}");
        }

        private static IEncryptionServices CreateEncryptionInstance(string packageName, EBuildPipeline pipeline)
        {
            string encryptionClassName = AssetBundleBuilderSetting.GetPackageEncyptionServicesClassName(packageName, pipeline.ToString());
            if (string.IsNullOrEmpty(encryptionClassName))
            {
                return null;
            }

            Type classType = YooAsset.Editor.EditorTools.GetAssignableTypes(typeof(IEncryptionServices))
                .FirstOrDefault(type => type.FullName == encryptionClassName);
            return classType == null ? null : (IEncryptionServices)Activator.CreateInstance(classType);
        }

        private static string GetBuiltinShaderBundleName(string packageName)
        {
            bool uniqueBundleName = AssetBundleCollectorSettingData.Setting.UniqueBundleName;
            PackRuleResult packRuleResult = DefaultPackRule.CreateShadersPackRuleResult();
            return packRuleResult.GetBundleName(packageName, uniqueBundleName);
        }

        private static void EnsureYooPackageExists(string packageName)
        {
            bool exists = AssetBundleCollectorSettingData.Setting.Packages.Any(package => package.PackageName == packageName);
            if (!exists)
            {
                throw new BuildFailedException($"YooAsset package not found: {packageName}");
            }
        }

        private static void EnsureHotfixDllsExist()
        {
            foreach (string dllName in AssemblyTool.DllNames)
            {
                string dllPath = $"{Define.CodeDir}/{dllName}.dll.bytes";
                string pdbPath = $"{Define.CodeDir}/{dllName}.pdb.bytes";
                if (!File.Exists(dllPath) || !File.Exists(pdbPath))
                {
                    throw new BuildFailedException($"Hotfix output missing: {dllPath} or {pdbPath}");
                }
            }
        }

        private static void PrepareCodeMode(UGFAppBuildSettings settings)
        {
            GlobalConfig globalConfig = AssetDatabase.LoadAssetAtPath<GlobalConfig>(GlobalConfigPath);
            if (globalConfig == null)
            {
                throw new BuildFailedException($"GlobalConfig not found: {GlobalConfigPath}");
            }

            if (globalConfig.CodeMode == CodeMode.Client)
            {
                return;
            }

            if (!settings.SwitchToClientCodeMode)
            {
                throw new BuildFailedException("Build Player requires GlobalConfig.CodeMode = Client.");
            }

            globalConfig.CodeMode = CodeMode.Client;
            EditorUtility.SetDirty(globalConfig);
            AssetDatabase.SaveAssets();
            Debug.Log("[UGF App Builder] GlobalConfig.CodeMode switched to Client for player build.");
        }

        private static string GetPlayerLocationPath(BuildTarget target, string buildDir)
        {
            string root = string.IsNullOrWhiteSpace(buildDir) ? "Release" : buildDir.Trim();
            string productName = string.IsNullOrWhiteSpace(PlayerSettings.productName) ? "ET" : PlayerSettings.productName;

            switch (target)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return Path.Combine(root, $"{productName}.exe");
                case BuildTarget.Android:
                    return Path.Combine(root, $"{productName}.apk");
                case BuildTarget.StandaloneOSX:
                    return Path.Combine(root, $"{productName}.app");
                case BuildTarget.iOS:
                case BuildTarget.WebGL:
                    return Path.Combine(root, productName);
                case BuildTarget.StandaloneLinux64:
                    return Path.Combine(root, productName);
                default:
                    return Path.Combine(root, productName);
            }
        }
    }
}
