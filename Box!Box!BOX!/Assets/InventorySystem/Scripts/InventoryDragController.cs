
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

        // Use the item's icon when available.
        if (source.item.icon != null)
        {
            dragIcon.sprite = source.item.icon;
            dragIcon.color = Color.white;
        }
        else
        {
            // Otherwise, use its configured color.
            dragIcon.sprite = null;
        }

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

        Inventory.TransferItem(sourceSlot, destination);
    }

    public void EndDrag()
    {
        sourceSlot = null;

        dragIcon.sprite = null;
        dragIcon.color = Color.white;
        dragIcon.gameObject.SetActive(false);
    }
}
