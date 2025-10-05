using UnityEditor;

namespace razz
{
    public class InteractorImportHandler : AssetPostprocessor
    {
        private static bool isWaitingForCompilation = false;

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string assetPath in importedAssets)
            {
                if (assetPath.EndsWith(".unitypackage") && assetPath.Contains("Interactor"))
                {// Triggers only if file path has "Interactor" and ".unitypackage" in it when imported
                    WaitForCompilationAndOpenNote();
                }
            }
        }

        static void WaitForCompilationAndOpenNote()
        {
            if (!EditorApplication.isCompiling)
            {
                DelayedOpenNote();
                return;
            }

            isWaitingForCompilation = true;
            EditorApplication.update += CheckCompilationStatus;
        }

        static void CheckCompilationStatus()
        {
            if (!isWaitingForCompilation || EditorApplication.isCompiling)
                return;

            isWaitingForCompilation = false;
            EditorApplication.update -= CheckCompilationStatus;

            DelayedOpenNote();
        }

        static void DelayedOpenNote()
        {
            EditorApplication.delayCall += () => EditorApplication.delayCall += () =>
            {
                InteractorWelcomeWindow.OpenWelcome();
            };
        }
    }
}
