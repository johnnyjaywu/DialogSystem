namespace ContentContent.Dialog.Editor
{
    using UnityEditor;

    public static class PackageTestExporter
    {
        [MenuItem("Tools/ContentContent/Export Test Suite Package")]
        public static void ExportTestPackage()
        {
            string[] paths = new[]
            {
                "Assets/Tests/Editor/ContentContent.Dialog.Tests.asmdef",
                "Assets/Tests/Editor/DialogServiceTests.cs",
                "Assets/Tests/Editor/DialogSourceTests.cs"
            };

            string exportPath = "ContentContent.Dialog.Tests-v1.0.0.unitypackage";

            AssetDatabase.ExportPackage(
                paths,
                exportPath,
                ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);

            UnityEngine.Debug.Log($"[{nameof(PackageTestExporter)}] Exported test suite to {exportPath}");
        }
    }
}