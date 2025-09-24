using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{

    public Slider slider;

    public float duration = 5f;

    private float GoneTime = 0f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        

        GoneTime += Time.deltaTime;
        slider.value = Mathf.Clamp01(GoneTime / duration);

        if(GoneTime >= duration)
        {
            Debug.Log("Done");
        }


    }
}
