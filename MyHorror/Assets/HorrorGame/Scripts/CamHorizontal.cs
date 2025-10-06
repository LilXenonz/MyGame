using UnityEngine;

public class CamHorizontal : MonoBehaviour
{
    public float sens = 2f;
    public float MaxLRot = 10f;
    public float MaxRRot = 50f;

    private float CurrentF;
    private float StartF;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        StartF = transform.localEulerAngles.y;
        CurrentF = 0;


    }

    // Update is called once per frame
    void Update()
    {

        float mouseX = Input.GetAxis("Mouse X") * sens;

        CurrentF += mouseX;

        CurrentF = Mathf.Clamp(CurrentF, -MaxLRot, MaxRRot);

        float final = StartF + CurrentF;
        transform.localRotation = Quaternion.Euler(0f, final, 0f);

    }
}
