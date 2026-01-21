using System.Collections.Generic;
using TMPro;
using TMPro.EditorUtilities;
using Unity.VisualScripting;
using UnityEngine;

public class ShelfSpaceController : MonoBehaviour
{
    public StockInfo info;
    public StockInfo Testinginfo;

    //public int amountOnShelf;

    public List<StockObject> objectsOnShelf;

    public List<Transform> soapPoints, soapBottlePoints, soapBoxPoints, towelPoints;

    public TextMeshProUGUI shelfLabel;
    
    public bool testPlace ;

    public bool acceptEverything;
    public StockInfo.StockType allowedType;

    public int NumberToPlace;

    private void Start()
    {
        NumberToPlace = NumberToPlaceCount();

        if (testPlace == true)
        {

            StockObject stock = Testinginfo.stockObject;

            for (int i = 0; i < NumberToPlace; i++)
            {
                StockObject obj = Instantiate(stock);
                placeStock(obj);
            }
        }

       
    }

    public void placeStock(StockObject objectToPlace)
    {
        bool preventPlacing = true;

        if (!acceptEverything)
        {
            if (objectToPlace.info.typeOfStock != allowedType)
                return;
        }

        //if (amountOnShelf == 0)
        if (objectsOnShelf.Count == 0)
        {
            info = objectToPlace.info;
            preventPlacing = false;
            
        }
        else
        {
            if(info.name == objectToPlace.info.name)
            {
                preventPlacing = false;

                switch(info.typeOfStock)
                {
                    case StockInfo.StockType.soap:

                        NumberToPlace = soapPoints.Count;

                        if (objectsOnShelf.Count >= soapPoints.Count)
                        {
                            preventPlacing = true;

                        }

                        break;

                    case StockInfo.StockType.soapBottle:

                        NumberToPlace = soapBottlePoints.Count;


                        if (objectsOnShelf.Count >= soapBottlePoints.Count)
                        {
                            preventPlacing = true;
                        }

                        break;

                    case StockInfo.StockType.soapBox:

                        NumberToPlace = soapBoxPoints.Count;


                        if (objectsOnShelf.Count >= soapBoxPoints.Count)
                        {
                            preventPlacing = true;
                        }

                        break;

                    case StockInfo.StockType.towel:

                        NumberToPlace = towelPoints.Count;

                        if (objectsOnShelf.Count >= towelPoints.Count)
                        {
                            preventPlacing = true;
                        }

                        break;

                    
                }

                
            }
        }

        if (preventPlacing == false)
        {
            //objectToPlace.transform.SetParent(transform);
            objectToPlace.MakePlaced();


            if (info.name == objectToPlace.info.name)
            {
                preventPlacing = false;

                switch (info.typeOfStock)
                {
                    case StockInfo.StockType.soap:

                        objectToPlace.transform.SetParent(soapPoints[objectsOnShelf.Count]);

                        break;

                    case StockInfo.StockType.soapBottle:

                        objectToPlace.transform.SetParent(soapBottlePoints[objectsOnShelf.Count]);


                        break;

                    case StockInfo.StockType.soapBox:

                        objectToPlace.transform.SetParent(soapBoxPoints[objectsOnShelf.Count]);


                        break;

                    case StockInfo.StockType.towel:

                        objectToPlace.transform.SetParent(towelPoints[objectsOnShelf.Count]);

                        break;

                }

                //amountOnShelf += 1;
                objectsOnShelf.Add(objectToPlace);

                shelfLabel.text = "$" + objectsOnShelf[0].info.price.ToString();
            }
        }

        if (FurnitureController.instance.AreAllShelvesFull() == true && ObjectiveManager.instance.currentObjectiveIndex == 2)
        {
            ObjectiveManager.instance.CompleteCurrentObjective();
        }

    }

    public StockObject GetStock()
    {
        StockObject objectToReturn = null;


        if (objectsOnShelf.Count > 0) 
        {
            objectToReturn = objectsOnShelf[objectsOnShelf.Count - 1];

            objectsOnShelf.RemoveAt(objectsOnShelf.Count - 1);
        }

        if (objectsOnShelf.Count == 0) 
        {
            shelfLabel.text = "";
        }

        return objectToReturn;
    }

    public bool IsShelfFull()
    {
        if (objectsOnShelf.Count == 0 || info == null)
            return false;

        switch (info.typeOfStock)
        {
            case StockInfo.StockType.soap:
                return objectsOnShelf.Count >= soapPoints.Count;

            case StockInfo.StockType.soapBottle:
                return objectsOnShelf.Count >= soapBottlePoints.Count;

            case StockInfo.StockType.soapBox:
                return objectsOnShelf.Count >= soapBoxPoints.Count;

            case StockInfo.StockType.towel:
                return objectsOnShelf.Count >= towelPoints.Count;
        }

        return false;
    }

    private int NumberToPlaceCount()
    {
        switch (info.typeOfStock)
        {
            case StockInfo.StockType.soap:
                return soapPoints.Count;

            case StockInfo.StockType.soapBottle:
                return soapBottlePoints.Count;

            case StockInfo.StockType.soapBox:
                return soapBoxPoints.Count;

            case StockInfo.StockType.towel:
                return towelPoints.Count;

        }

        return 0;
    }


}
