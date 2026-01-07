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

    private bool isTalking = false;

    public CinemachineCamera TalkZoomVcam;

    public PlayerController playerController ;


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
        if (checkoutScreen.activeSelf == true && customersInQueue.Count > 0 && !isTalking) 
        {
            HidePrice();

            //add money to balance not done but in StoreController.instance.AddMoney(customersInQueue[0].GetTotalSpent);
            /*customersInQueue[0].StartLeaving();
            customersInQueue.RemoveAt(0);
            UpdateQueue();*/

            isTalking = true;
            Customer c = customersInQueue[0];
            CustomerDialogueManager.instance.StartDialogue(
                c.dialogueIndex,
                c.lookAtScript,
                OnTalkFinished
            );

            playerController.enabled = false;
            TalkZoomVcam.Priority = 4;
            TalkZoomVcam.LookAt = customersInQueue[0].transform;
        }

    }

    private void OnTalkFinished()
    {
        isTalking = false;

        playerController.enabled = true;
        TalkZoomVcam.Priority = 0;

        TalkZoomVcam.LookAt = null;


        if (customersInQueue.Count > 0)
        {
            //add money to balance not done but in StoreController.instance.AddMoney(customersInQueue[0].GetTotalSpent);
            customersInQueue[0].StartLeaving();
            customersInQueue.RemoveAt(0);
            UpdateQueue();
        }
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
    }
}
