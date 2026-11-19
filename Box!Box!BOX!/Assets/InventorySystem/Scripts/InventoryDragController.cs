
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryDragController : MonoBehaviour
{
    [SerializeField] private Image dragIcon;

    private InventorySlot sourceSlot;

    private void Awake()
    {
        dragIcon.gameObject.SetActive(false);
    }

    public void BeginDrag(
        InventorySlot source,
        PointerEventData eventData)
    {
        if (source == null || source.item == null)
            return;

        sourceSlot = source;

        // Hide the original item icon.
        source.SetIconVisible(false);

        // Display the dragged item.
        dragIcon.sprite = source.item.icon;
        dragIcon.color = Color.white;
        dragIcon.preserveAspect = true;
        dragIcon.enabled = true;
        dragIcon.gameObject.SetActive(true);

        dragIcon.transform.position = eventData.position;
        dragIcon.transform.SetAsLastSibling();
    }

    public void Drag(PointerEventData eventData)
    {
        if (sourceSlot == null)
            return;

        dragIcon.transform.position = eventData.position;
    }

    public void DropOn(InventorySlot destination)
    {
        if (sourceSlot == null || destination == null)
            return;

        bool success = Inventory.TransferItem(
            sourceSlot,
            destination
        );

        // If the transfer failed, restore the original icon.
        if (!success)
        {
            sourceSlot.SetIconVisible(true);
        }
    }

    public void EndDrag()
    {
        // Restore the original icon if the item wasn't moved.
        if (sourceSlot != null && sourceSlot.item != null)
        {
            sourceSlot.SetIconVisible(true);
        }

        sourceSlot = null;

        dragIcon.sprite = null;
        dragIcon.color = Color.white;
        dragIcon.gameObject.SetActive(false);
    }
}
