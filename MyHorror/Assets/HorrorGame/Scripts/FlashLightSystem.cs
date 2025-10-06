using UnityEngine;

public class FlashLightSystem : MonoBehaviour
{

    private bool FlashlightOn = false;


    [SerializeField] private Light Light;
    [SerializeField] private AudioSource FlashSound;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Light.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.F))
        {

            FlashlightOn = !FlashlightOn;

            FlashSound.Play();

            if (FlashlightOn == true)
            {

                Light.enabled = true;

            }

            else
            {

                Light.enabled = false;

            }
        }       

    }
}
