using UnityEngine;
using UnityEditor;

namespace razz
{
    public class Links : Editor
    {
        public static string onlineDocName = "Docs";
        public static string onlineDocDesc = "Go to online documentation and videos";
        [MenuItem("Window/Interactor/Online Documentation", priority = 1)]
        public static void OnlineDocumentataion()
        {
            Application.OpenURL("https://negengames.com/interactor");
        }

        public static string forumName = "Forum";
        public static string forumDesc = "Go to official Discussions thread for questions, suggestions, or bug reports. I'm answering daily there! Unlike Discord, the forum makes information accessible to everyone, forever.";
        public static void Forum()
        {
            Application.OpenURL("https://negengames.com/interactor/forum.html");
        }

        public static string messageName = "Support";
        public static string messageDesc = "Get a support within hours via those channels!";
        public static void Message()
        {
            Support();
        }

        public static string storeName = "Store Page";
        public static string storeDesc = "Open Asset Page. Your reviews are vital in helping me dedicate more time to Interactor, rather than taking on freelance work to support myself.";
        public static void Store(bool reviewPage)
        {
            if (reviewPage)
            {
#if UNITY_2020_OR_NEWER
            Application.OpenURL("https://assetstore.unity.com/packages/slug/178062#reviews");
#else
                UnityEditorInternal.AssetStore.Open("content/178062/reviews");
#endif
            }
            else
            {
#if UNITY_2020_OR_NEWER
            Application.OpenURL("https://assetstore.unity.com/packages/slug/178062");
#else
                UnityEditorInternal.AssetStore.Open("content/178062");
#endif
            }
        }
        public static void Changelog()
        {
            Application.OpenURL("https://negengames.com/interactor/changelog.html#changelog");
        }
        public static void Support()
        {
            Application.OpenURL("https://negengames.com/interactor/documentation.html#support");
        }
        public static void Tutorials()
        {
            Application.OpenURL("https://negengames.com/interactor/videos.html#videos");
        }

        public static string interactorScriptName = "Interactor Main Loop";
        public static string interactorScriptDesc = "If you like to edit main Interactor script, this goes into codes.";
        public static void InteractorScript()
        {
            MonoScript ms = (MonoScript)AssetDatabase.LoadAssetAtPath("Assets/Interactor/Core/Scripts/Interactor.cs", typeof(MonoScript));
            AssetDatabase.OpenAsset(ms, 2132);
        }

        public static string interactorEditorScriptName = "Expose Properties";
        public static string interactorEditorScriptDesc = "If you like to add more properties here, this goes into editor script codes.";
        public static void InteractorEditorScript()
        {
            MonoScript ms = (MonoScript)AssetDatabase.LoadAssetAtPath("Assets/Interactor/Core/Scripts/Editor/InteractorEditor.cs", typeof(MonoScript));
            AssetDatabase.OpenAsset(ms, 1425);
        }

        public static string shaderLinkText = "Online Documentation      ";
        public static void ShaderLinks()
        {
            Application.OpenURL("https://negengames.com/interactor/documentation.html#shaders");
        }

        public static string downloadExamplesURL = "https://negengames.com/interactor/downloadexamples.html";
        public static string downloadTextures1URL = "https://negengames.com/interactor/downloadtextures1.html";
        public static string downloadTextures2URL = "https://negengames.com/interactor/downloadtextures2.html";
        public static string texturepack4k = "https://negengames.com/interactor/texturepack.html";
    }
}
