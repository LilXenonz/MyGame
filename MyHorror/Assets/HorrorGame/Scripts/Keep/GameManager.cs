using UnityEngine;
using UnityEngine.Rendering;
using UnityStandardAssets.Characters.FirstPerson;

public class GameManager : Singleton<GameManager>
{
    public Volume GlobalPPVolume;

    [SerializeField] private FirstPersonController  FPSController;


    public T GetStack<T>() where T : VolumeComponent
    {
        if (GlobalPPVolume.profile.TryGet(out T component))
            return component;

        return default;
    }

    public void FreezePlayer(bool state, bool showCursor = false, bool lockInput = true)
    {  

        FPSController.m_BlockLook = state;
        FPSController.m_BlockMovement = state;


        if (lockInput)
        {
            Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = showCursor;
        }
    }



}
