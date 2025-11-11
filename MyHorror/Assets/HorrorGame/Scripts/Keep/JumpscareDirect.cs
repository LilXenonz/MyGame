using System;
using UnityEngine;

[InspectorHeader("Jumpscare Direct")]
public class JumpscareDirect : MonoBehaviour
{
    [Serializable]
    public struct DirectModel
    {
        public string ModelID;
        public GameObject ModelObject;
    }

    public DirectModel[] JumpscareDirectModels;

    private GameObject directModel;
    private float directDuration;

    public void ShowDirectJumpscare(string modelID, float duration)
    {
        foreach (var direct in JumpscareDirectModels)
        {
            if (direct.ModelID == modelID)
            {
                direct.ModelObject.SetActive(true);
                directModel = direct.ModelObject;
                break;
            }
        }

        if (directModel != null) directDuration = duration;
    }

    private void Update()
    {
        if (directDuration > 0f)
        {
            directDuration -= Time.deltaTime;
        }
        else if (directModel != null)
        {
            directModel.SetActive(false);
            directModel = null;
            directDuration = 0f;
        }
    }
}
