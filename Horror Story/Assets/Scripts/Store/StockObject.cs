using UnityEngine;

public class StockObject : MonoBehaviour
{
    public StockInfo info;

    public float moveSpeed;

    public bool isPlaced;

    public Rigidbody RB;

    public Collider col;

    private bool inBag;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        info = StockInfoController.instance.GetInfo(info.name);
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlaced == true)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, Vector3.zero, moveSpeed * Time.deltaTime);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.identity, moveSpeed * Time.deltaTime);
        }
        if(inBag == true)
        {
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.zero, Time.deltaTime);
        }
    }

    public void PickUp()
    {
        RB.isKinematic = true;

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        isPlaced = false;

        col.enabled = false;
    }

    public void MakePlaced()
    {
        RB.isKinematic = true;

        isPlaced = true;

        col.enabled = false;
    }

    public void Release ()
    {
        RB.isKinematic = false;

        col.enabled = true;
    }

    public void PlaceInBox()
    {
        RB.isKinematic = true;

        col.enabled = false;
    }

    public void PlaceInBag()
    {
        inBag = true;

        MakePlaced();
    }
}
