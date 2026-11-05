using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject {
    public string itemName = "New Item";
    public Sprite icon = null;
    public string description = "Item description";
    public bool isStackable = false;
    public int maxStack = 1;

}
