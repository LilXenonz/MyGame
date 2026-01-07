using Unity.Cinemachine;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{

    public CinemachineCamera cam;

    public float NormalFOV = 60f;
    public float ZoomFOV = 40f;
    public float ZoomSpeed = 5f;

    private bool Zooming = false;


    void Update()
    {
        if (Input.GetMouseButtonDown(1)) // if press  right mouse 
        {
            Zooming = true;
        }

        if (Input.GetMouseButtonUp(1))
        {
            Zooming = false;
        }

        if (Zooming)
        {
            cam.Lens.FieldOfView = Mathf.Lerp(cam.Lens.FieldOfView, ZoomFOV, Time.deltaTime * ZoomSpeed);
        }
        else
        {

            cam.Lens.FieldOfView = Mathf.Lerp(cam.Lens.FieldOfView, NormalFOV, Time.deltaTime * ZoomSpeed);
        }
    }
}
