using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;
using UnityStandardAssets.Characters.FirstPerson;

public class CamInteraction : MonoBehaviour
{

    //Loockat check

    public bool TalkToNpcBool = false;
    public bool TalkToCopBool = false;

    //Loockat check

    public LookAtFunc lookAtFunc;

    public Text InteractionText;

    private float InteractionDis = 5f;

    public bool CanInteraction = true;


    //look at

    public CinemachineCamera PlayerCam;
    public CinemachineCamera TalkNpcCam;
    public CinemachineCamera CopZoomCam;

    public FirstPersonController FpsController;
    //look at


    //talk

    public GameObject TalkPanel;
    public GameObject ChoicePack;
    public Text SubText;
    string holder;
    float time = 0.05f;

    //talk

    //audio

    public AudioSource TalkSrc;

    //audio


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (CanInteraction == true)
        {
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, InteractionDis))
            {
                if (hit.collider.CompareTag("NPC"))
                {
                    InteractionText.text = "Talk To Him";

                    //talk

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        CanInteraction = false;
                        StartCoroutine(TalkToNPC());
                    }
                }
                else if (hit.collider.CompareTag("Cop"))
                {
                    InteractionText.text = "Talk To Cop";

                    //talk

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        CanInteraction = false;
                        StartCoroutine(TalkToCop());
                    }
                }

                else
                {
                    InteractionText.text = "";

                }
            }

            else
            {
                InteractionText.text = "";
            }
        }

    }

    IEnumerator TalkToCop()
    {
        //chechk bool

        TalkToCopBool = true;

        //chechk bool


        InteractionText.text = "";
        FpsController.enabled = false;
        CopZoomCam.enabled = true;
        TalkNpcCam.enabled = false;
        PlayerCam.enabled = false;

        //look at

        lookAtFunc.IKActive = true;


        //look at

        yield return new WaitForSeconds(1f);

        TalkPanel.SetActive(true);

        //audio

        TalkSrc.Play();

        //audio


        SubText.text = "Me: ";
        holder = "Why are you fat?";
        foreach (char c in holder)
        {
            SubText.text += c;
            yield return new WaitForSeconds(time);
        }

        //audio
        TalkSrc.Stop();
        //audio

        yield return MousePressed();

        TalkSrc.Play();


        SubText.text = "Cop: ";
        holder = "I eat a lot of donuts";
        foreach (char c in holder)
        {
            SubText.text += c;
            yield return new WaitForSeconds(time);
        }

        TalkSrc.Stop();


        yield return MousePressed();

        StartCoroutine(Final());

    }

    IEnumerator TalkToNPC()
    {

        //chechk bool

        TalkToNpcBool = true;

        //chechk bool

        InteractionText.text = "";
        FpsController.enabled = false;
        TalkNpcCam.enabled = true;
        PlayerCam.enabled = false;
        CopZoomCam.enabled= false;

        // look at
        lookAtFunc.IKActive = true;
        //look at

        yield return new WaitForSeconds(1f);   
        
        //cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        //cursor 

        TalkPanel.SetActive(true);

        //audio

        TalkSrc.Play();

        //audio


        SubText.text = "Me: ";
        holder = "Hello, are you okay ?";
        foreach(char c in holder)
        {
            SubText.text += c;
            yield return new WaitForSeconds(time);
        }

        //audio
        TalkSrc.Stop();
        //audio

        yield return MousePressed();

        TalkSrc.Play();


        SubText.text = "Man: ";
        holder = "Yes sir";
        foreach (char c in holder)
        {
            SubText.text += c;
            yield return new WaitForSeconds(time);
        }

        TalkSrc.Stop();


        yield return MousePressed();

        TalkSrc.Play();


        SubText.text = "Man: ";
        holder = "Are you lost";
        foreach (char c in holder)
        {
            SubText.text += c;
            yield return new WaitForSeconds(time);
        }

        TalkSrc.Stop();

        yield return new WaitForSeconds(1f);

        ChoicePack.SetActive(true);       

        
    }

    public void Choice1Func()
    {
        StartCoroutine(Choise1());
    }
    public void Choice2Func()
    {
        StartCoroutine(Choise2());
    }

    IEnumerator Choise1()
    {
        ChoicePack.SetActive(false);

        TalkSrc.Play();


        SubText.text = "Me: ";
        holder = "No, im from here";
        foreach (char c in holder)
        {
            SubText.text += c;
            yield return new WaitForSeconds(time);
        }

        TalkSrc.Stop();

        yield return new WaitForSeconds(3f);

        StartCoroutine(Final());
    }
    IEnumerator Choise2()
    {

        ChoicePack.SetActive(false);


        TalkSrc.Play();

        SubText.text = "Me: ";
        holder = "Yes, i will ask for help later";
        foreach (char c in holder)
        {
            SubText.text += c;
            yield return new WaitForSeconds(time);
        }

        TalkSrc.Stop();

        yield return new WaitForSeconds(3f);

        StartCoroutine(Final());

    }

    IEnumerator Final() 
    {
        //chechk bool

        TalkToCopBool = false;
        TalkToNpcBool = false;

        //chechk bool

        TalkPanel.SetActive(false);
        ChoicePack.SetActive(false);
        SubText.text = "";

        //look at
        lookAtFunc.IKActive = false;
        //look at

        FpsController.enabled = true;
        PlayerCam.enabled = true;
        TalkNpcCam.enabled = false;
        CopZoomCam.enabled = false;

        CanInteraction = true;

        yield return null;
    }

    IEnumerator MousePressed()
    {
        while(!Input.GetMouseButtonDown(0))
        {
            yield return null;
        }
    }
}


