using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;
using UnityStandardAssets.Characters.FirstPerson;


public class Friend : MonoBehaviour
{

    public LookAtFunc LookAtScript;


    // look at


    public CinemachineCamera PlayerVcam;
    public CinemachineCamera TalkZoomVcam;

    public FirstPersonController FpsController;


    // look at


    public GameObject TalkPanel;

    public GameObject ChoicePack;

    public Text SubText;
    string holder;
    float time = 0.05f;

    public AudioSource TalkSource;


    public void talkToFriend()
    {
        StartCoroutine(TalkToManeqCO());
    }

    IEnumerator TalkToManeqCO()
    {

        FpsController.enabled = false;
        TalkZoomVcam.enabled = true;
        PlayerVcam.enabled = false;

        // look at

        LookAtScript.IKActive = true;

        // look at


        yield return new WaitForSeconds(1f);


        FpsController.enabled = false;

        // cursor

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // cursor

        TalkPanel.SetActive(true);

        // audio

        TalkSource.Play();

        // audio

        SubText.text = "Me: ";
        holder = "Hello, are you OK ?";
        foreach (char c in holder)
        {

            SubText.text += c;
            yield return new WaitForSeconds(time);


        }

        // audio

        TalkSource.Stop();

        // audio


        yield return MousePress();


        TalkSource.Play();

        SubText.text = "Maneq: ";
        holder = "Yeah I'm fine.";
        foreach (char c in holder)
        {

            SubText.text += c;
            yield return new WaitForSeconds(time);


        }

        TalkSource.Stop();

        yield return MousePress();

        TalkSource.Play();

        SubText.text = "Maneq: ";
        holder = "Are you lost ?";
        foreach (char c in holder)
        {

            SubText.text += c;
            yield return new WaitForSeconds(time);


        }

        TalkSource.Stop();

        yield return new WaitForSeconds(1f);


        ChoicePack.SetActive(true);



    }



    public void Choice1Void()
    {

        StartCoroutine(Choice1CO());


    }

    public void Choice2Void()
    {


        StartCoroutine(Choice2CO());


    }


    IEnumerator Choice1CO()
    {

        ChoicePack.SetActive(false);


        TalkSource.Play();

        SubText.text = "Me: ";
        holder = "No, I'm a local";
        foreach (char c in holder)
        {

            SubText.text += c;
            yield return new WaitForSeconds(time);


        }

        TalkSource.Stop();

        yield return new WaitForSeconds(3f);


        StartCoroutine(FinalCO());


    }

    IEnumerator Choice2CO()
    {


        ChoicePack.SetActive(false);

        TalkSource.Play();

        SubText.text = "Me: ";
        holder = "Yes, I will ask for help later.";
        foreach (char c in holder)
        {

            SubText.text += c;
            yield return new WaitForSeconds(time);


        }

        TalkSource.Stop();

        yield return new WaitForSeconds(3f);

        StartCoroutine(FinalCO());


    }


    IEnumerator FinalCO()
    {

        TalkPanel.SetActive(false);
        FpsController.enabled = true;
        ChoicePack.SetActive(false);
        SubText.text = "";

        // look at

        LookAtScript.IKActive = false;

        // look at

        FpsController.enabled = true;
        PlayerVcam.enabled = true;
        TalkZoomVcam.enabled = false;


        yield return null;


    }



    IEnumerator MousePress()
    {



        while (!Input.GetMouseButtonDown(0))
        {


            yield return null;



        }


    }




}