
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [Header("Inventory Configuration")]
    [SerializeField] private int capacity = 6;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform slotsParent;

    [SerializeField] private Item[] initialItems;
    [SerializeField] private InventoryDragController dragController;

    private readonly List<InventorySlot> slots = new List<InventorySlot>();

    public int Capacity => capacity;
    public IReadOnlyList<InventorySlot> Slots => slots;

    private void Start()
    {
        InitializeInventory();
        SetInitialItems(initialItems);
    }

    public void InitializeInventory()
    {
        // Remove previously generated slots.
        foreach (Transform child in slotsParent)
        {
            Destroy(child.gameObject);
        }

        slots.Clear();

        // Create the required number of slots.
        for (int i = 0; i < capacity; i++)
        {
            GameObject slotObject = Instantiate(slotPrefab, slotsParent);

            InventorySlot slot = slotObject.GetComponent<InventorySlot>();

            slot.Initialize(this, i, dragController);

            slots.Add(slot);
        }
    }

    public bool AddItem(Item item)
    {
        if (item == null)
            return false;

        foreach (InventorySlot slot in slots)
        {
            if (slot.item == null)
            {
                slot.SetItem(item);
                Debug.Log("Added " + item.itemName + " to slot " + slot.SlotIndex);
                return true;
            }
        }

        Debug.Log("Inventory is full!");
        return false;
    }

    public void RemoveItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count)
            return;

        slots[slotIndex].Clear();
    }


    public void SetInitialItems(Item[] initialItems)
    {
        if (initialItems == null)
            return;

        for (int i = 0; i < initialItems.Length && i < slots.Count; i++)
        {
            if (initialItems[i] != null)
            {
                slots[i].SetItem(initialItems[i]);
            }
        }
    }


    public static bool TransferItem(
        InventorySlot source,
        InventorySlot destination)
    {
        if (source == null || destination == null)
            return false;

        if (source == destination)
            return false;

        if (source.item == null || destination.item != null)
            return false;

        Item movingItem = source.item;

        destination.SetItem(movingItem);
        source.Clear();

        Debug.Log(
            "Moved " + movingItem.itemName +
            " from inventory " + source.Owner.name +
            " to " + destination.Owner.name
        );

        return true;
    }


}
