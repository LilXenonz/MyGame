using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{

    [Header("Inventory Settings")]
    public Transform[] handsPos;
    [Header("Items database script ")]
    public ItemsDatabase database;
    [HideInInspector]
    public int CurrentItemID;
    [HideInInspector]
    public GameObject CurrentItem;
    [Tooltip("Layer ID for item camera culling mask (10 - Item) When we pick up item we make layer 10 (Item) for culling")]
    public int itemCameraLayerID;
    [Tooltip("Layer ID for interact (8 - Interact layer) When we drop item we make layer of this item to 8 (Interact) if we wanna pick up item later")]
    public int itemLayerID;
    private int currentHandPos;

    private bool isPlaced = false;

    public float moveSpeed;

    public Transform ShelfPos;

    void Update()
    {
        if (isPlaced && CurrentItem != null)
        {
            CurrentItem.transform.localPosition = Vector3.MoveTowards(CurrentItem.transform.localPosition, ShelfPos.transform.position, moveSpeed * Time.deltaTime);
            CurrentItem.transform.localRotation = Quaternion.Slerp(CurrentItem.transform.localRotation, Quaternion.identity, moveSpeed * Time.deltaTime);
                
        }
        if (isPlaced && CurrentItem != null &&
    Vector3.Distance(CurrentItem.transform.position, ShelfPos.position) < 0.01f)
        {
            isPlaced = false;
            CurrentItem = null;
        }
    }

    private void Awake()
    {
        CurrentItemID = -1;
    }

    public void AddItem(int ID, GameObject ItemGO)
    {
        if (CurrentItem)
        {
            DropItem();
        }

        CurrentItemID = ID;
        CurrentItem = ItemGO;
        CurrentItem.GetComponent<Rigidbody>().isKinematic = true;
        int dataBaseID = GetDatabaseID(CurrentItemID);
        currentHandPos = database.Items[dataBaseID].handPosID;
        CurrentItem.transform.parent = handsPos[currentHandPos];
        CurrentItem.transform.localPosition = Vector3.zero;
        CurrentItem.transform.rotation = handsPos[currentHandPos].rotation;
        CurrentItem.layer = itemCameraLayerID;

        isPlaced = false;

    }

    public void PlayItemAnim(string animName)
    {
        handsPos[currentHandPos].GetComponent<Animation>().Play(animName);
    }

    public void DropItem()
    {
        if (CurrentItem != null)
        {
            CurrentItem.layer = itemLayerID;
            CurrentItemID = -1;
            CurrentItem.transform.parent = null;
            CurrentItem.GetComponent<Rigidbody>().isKinematic = false;
            CurrentItem = null;
        }
    }

    public void PlaceOnShelf()
    {
        if (CurrentItem != null)
        {
            isPlaced= true;

            CurrentItem.layer = itemLayerID;
            CurrentItemID = -1;
            CurrentItem.transform.parent = null;

            //CurrentItem.transform.position = PlacePos.position;
            //CurrentItem.transform.rotation = PlacePos.rotation; 

            //CurrentItem = null;

        }
    }

    public void RemoveItem()
    {

        if (CurrentItem != null)
        {
            Destroy(CurrentItem);
            CurrentItemID = -1;
            CurrentItem = null;
        }

    }

    private int GetDatabaseID(int id)
    {
        for (int i = 0; i < database.Items.Count; i++)
        {
            if (database.Items[i].id == id)
            {
                return i;
            }
        }

        return -1;
    }
}
