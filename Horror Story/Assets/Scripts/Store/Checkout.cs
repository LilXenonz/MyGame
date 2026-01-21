using DialogueEditor;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class Checkout : MonoBehaviour
{
    public static Checkout instance;

    public TextMeshProUGUI priceText;
    public GameObject checkoutScreen;

    public Transform queuePoint;

    public List<Customer> customersInQueue = new List<Customer>();

    public CinemachineCamera TalkZoomVcam;

    public PlayerController playerController ;

    //public CinemachineCamera playerCam;
    public CinemachineCamera NPCCam;

    private LookAtFunc currentLookAtScript;



    private void Awake()
    {
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //ShowPrice(45.32f);
        HidePrice();
    }

    // Update is called once per frame
    void Update()
    {
        if (customersInQueue.Count > 0 && checkoutScreen.activeSelf == false) 
        {
            if (Vector3.Distance(customersInQueue[0].transform.position, queuePoint.position) < 0.1f) 
            {
                ShowPrice(customersInQueue[0].GetTotalSpent());
            }
        }
    }

    public void ShowPrice(float priceTotal)
    {
        checkoutScreen.SetActive(true);

        priceText.text = "$" + priceTotal.ToString("F2");
    }

    public void HidePrice()
    {
        checkoutScreen.SetActive(false);
    }

    public void CheckoutCustomer()
    {
        if (checkoutScreen.activeSelf == true && customersInQueue.Count > 0) 
        {
            HidePrice();

            //add money to balance not done but in StoreController.instance.AddMoney(customersInQueue[0].GetTotalSpent);

            ConversationManager.Instance.StartConversation(customersInQueue[0].conversation);

            /*customersInQueue[0].StartLeaving();
            customersInQueue.RemoveAt(0);
            UpdateQueue();*/

            TalkZoomVcam.Priority = 4;
            TalkZoomVcam.LookAt = customersInQueue[0].transform;
        }

    }

    void FinishCheckout()
    {
        customersInQueue[0].StartLeaving();
        customersInQueue.RemoveAt(0);
        UpdateQueue();
    }



    public void AddCustomerToQueue(Customer newCustomer)
    {
        customersInQueue.Add(newCustomer);

        UpdateQueue();
    }

    public void UpdateQueue()
    {
        for (int i = 0; i < customersInQueue.Count; i++)
        {
            customersInQueue[i].UpdateQueuePoint(queuePoint.position + (queuePoint.forward * i * 1.6f));
        }

        if (customersInQueue.Count > 0)
        {
            currentLookAtScript = customersInQueue[0].lookAtScript;
        }
        else
        {
            currentLookAtScript = null;
        }
    }
    private void onDialogue()
    {
        NPCCam.Priority = 5;
        PlayerController.instance.lockPlayer = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        currentLookAtScript.IKActive = true;
    }

    private void offDialogue()
    {
        NPCCam.Priority = 0;
        PlayerController.instance.lockPlayer = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        currentLookAtScript.IKActive = false;
    }

    private void OnEnable()
    {
        ConversationManager.OnConversationStarted += ConversationStart;
        ConversationManager.OnConversationEnded += ConversationEnd;
    }

    private void OnDisable()
    {
        ConversationManager.OnConversationStarted -= ConversationStart;
        ConversationManager.OnConversationEnded -= ConversationEnd;
    }

    private void ConversationStart()
    {
        onDialogue();
    }

    private void ConversationEnd()
    {
        offDialogue();
        FinishCheckout();
    }

}
