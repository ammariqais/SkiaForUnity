using Unity.PlasticSCM.Editor.AssetUtils.Processor;

namespace Unity.PlasticSCM.Editor.AssetUtils
{
    internal static class RefreshAsset
    {
        internal static void UnityAssetDatabase()
        {
            UnityEditor.AssetDatabase.Refresh(
                UnityEditor.ImportAssetOptions.Default);

            UnityEditor.VersionControl.Provider.ClearCache();

            AssetPostprocessor.SetIsRepaintInspectorNeededAfterAssetDatabaseRefresh();
        }

        internal static void VersionControlCache()
        {
            UnityEditor.VersionControl.Provider.ClearCache();

            RepaintInspector.All();
        }
    }
}