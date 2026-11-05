
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IDropHandler
{
    public Item item;

    [SerializeField] private Image itemIcon;

    public Inventory Owner { get; private set; }
    public int SlotIndex { get; private set; }

    private InventoryDragController dragController;

    public void Initialize(
        Inventory owner,
        int index,
        InventoryDragController controller)
    {
        Owner = owner;
        SlotIndex = index;
        dragController = controller;

        Clear();
    }

    public void SetItem(Item newItem)
    {
        item = newItem;

        if (itemIcon != null)
        {
            if (item != null)
            {
                itemIcon.sprite = null;
                itemIcon.enabled = true;
            }
            else
            {
                itemIcon.color = Color.white;
                itemIcon.enabled = false;
            }
        }
    }

    public void Clear()
    {
        SetItem(null);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (item != null)
            dragController.BeginDrag(this, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        dragController.Drag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        dragController.EndDrag();
    }

    public void OnDrop(PointerEventData eventData)
    {
        dragController.DropOn(this);
    }
}
