
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    [SerializeField] private Inventory inventory;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (inventory != null)
            {
                inventory.gameObject.SetActive(
                    !inventory.gameObject.activeSelf
                );
            }
        }
    }
}
