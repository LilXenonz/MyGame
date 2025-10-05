using UnityEngine;
using UnityEngine.EventSystems;

namespace razz
{
    public class AutoInputModuleFixer : MonoBehaviour
    {//Checks if you use only new input system and changes UI component to compatible version to prevent errors.
        void Awake()
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            StandaloneInputModule standaloneModule = GetComponent<StandaloneInputModule>();

            if (standaloneModule != null)
            {
                DestroyImmediate(standaloneModule);

                gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }
#endif
        }
    }

}
