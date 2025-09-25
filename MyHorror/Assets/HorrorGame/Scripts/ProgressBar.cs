using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{

    public Slider slider;

    public float duration = 5f;

    private float GoneTime = 0f;

    //

    public bool RunProgress = false;

    public CamInteraction CamInteract;

    private bool DidAlready = false;

    private bool TalkToNPCFunc = false;

    private bool InteractWithDish = false;

    public GameObject SliderGB;

    public Text UnderText;

    //


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        SliderGB.gameObject.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {


        if (RunProgress == true)
        {
            GoneTime += Time.deltaTime;
            slider.value = Mathf.Clamp01(GoneTime / duration);

            if (GoneTime >= duration && DidAlready == false)
            {


                Debug.Log("Done");
                DidAlready = true;
                RunProgress = false;

                UnderText.text = "";

                if(TalkToNPCFunc == true)
                {
                    TalkToNPCFunc = false;
                    SliderGB.SetActive(false);


                    //CamInteract

                    CamInteract.TalkToNpcVoid();

                    //CamInteract
                }
                else if (InteractWithDish == true)
                {
                    InteractWithDish = false;
                    SliderGB.SetActive(false);
                }
            }
        }


    }



    public void TalkNpcProgress()
    {
        duration = 2f;
        SliderGB.SetActive(true);
        RunProgress = true;
        TalkToNPCFunc = true;

        UnderText.text = " talking to npc";
    }

    public void ResetProgressBar()
    {
        RunProgress = false;
        GoneTime = 0f;
        slider.value += 0f;
        DidAlready = false;

        //reset func

        TalkToNPCFunc= false;
        InteractWithDish= false;

        //reset func
    }
}
