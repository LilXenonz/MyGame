using UnityEngine;

[System.Serializable]
public class StockInfo  
{
    public string name; 

    public enum StockType
    {
        soap, soapBottle, soapBox, towel
    }

    public StockType typeOfStock;

    public float price;

    public StockObject stockObject;
}
 