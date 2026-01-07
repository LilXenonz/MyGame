using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Water;

public class StockBoxController : MonoBehaviour
{

    public StockInfo info;

    public List<Transform> soapPoints, soapBottlePoints, soapBoxPoints, towelPoints;

    public List<StockObject> stockInBox;

    public bool testFill;

    public Rigidbody RB;
    public Collider col;

    private bool isHeld;

    public float moveSpeed = 5f;

    public GameObject flap1, flap2;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (testFill == true)
        {
            testFill = false;
            SetupBox(info);
        }

        if (isHeld == true)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, Vector3.zero, moveSpeed * Time.deltaTime);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.identity, moveSpeed * Time.deltaTime);
        }
    }


    public void SetupBox(StockInfo stockType)
    {
        info = stockType;

        List<Transform> activePoints = new List<Transform>();

        switch (info.typeOfStock)
        {
            case StockInfo.StockType.soap:

                activePoints.AddRange(soapPoints);

                break;
            case StockInfo.StockType.soapBottle:

                activePoints.AddRange(soapBottlePoints);

                break;
            case StockInfo.StockType.soapBox:

                activePoints.AddRange(soapBoxPoints);

                break;
            case StockInfo.StockType.towel:

                activePoints.AddRange(towelPoints);

                break;          
                   
        }

        if(stockInBox.Count ==0)
        {
            for (int i = 0; i < activePoints.Count; i++)
            {
                StockObject stock = Instantiate(stockType.stockObject, activePoints[i]);
                stock.transform.localPosition = Vector3.zero;
                stock.transform.localRotation = Quaternion.identity;

                stockInBox.Add(stock);

                stock.PlaceInBox();
            }
        }

    }

    public void PickUp()
    {
        RB.isKinematic = true;

        col.enabled = false;

        isHeld = true;
    }

    public void Release()
    {
        RB.isKinematic = false;

        col.enabled = true;

        isHeld = false;
    }

    public void OpenClose()
    {
        if (flap1.activeSelf == true)
        {
            flap1.SetActive(false);
            flap2.SetActive(false);
        }
        else
        {
            flap1.SetActive(true);
            flap2.SetActive(true);
        }
    }

    public void PlaceStockOnShelf(ShelfSpaceController shelf)
    {
        if (stockInBox.Count > 0)
        {
            shelf.placeStock(stockInBox[stockInBox.Count - 1]);

            if (stockInBox[stockInBox.Count - 1].isPlaced == true)
            {
                stockInBox.RemoveAt(stockInBox.Count - 1);
            }
        }

        if (flap1.activeSelf == true) 
        {
            OpenClose();
        }
    }

}
