
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Item item;

    [SerializeField] private Inventory playerInventory;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (playerInventory == null)
        {
            Debug.LogWarning("Player inventory is not assigned!");
            return;
        }

        if (playerInventory.AddItem(item))
        {
            Destroy(gameObject);
        }
    }
}
