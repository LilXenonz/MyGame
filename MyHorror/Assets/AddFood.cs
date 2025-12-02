using UnityEngine;
using static UnityEditor.PlayerSettings;
using static UnityEngine.Rendering.DebugUI.Table;

public class AddFood : MonoBehaviour
{
    public Inventory inventory;

    public int itemAddID;

    public GameObject ItemGB;

    public void addItemID()
    {
        GameObject instance = Instantiate(ItemGB);

        inventory.AddItem(itemAddID, instance);
    }


}
