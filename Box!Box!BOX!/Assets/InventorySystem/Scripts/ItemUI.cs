using UnityEngine;
using UnityEngine.EventSystems;

public class ItemUI : MonoBehaviour, IPointerClickHandler {
    public Item item; // assigned at runtime
    public GameObject itemDropPrefab; // assign the 3D/2D drop version in the Inspector
    public Transform playerTransform;  // will be found by tag "Player"

    private void Start() {
        // Automatically find the player by tag
        if (playerTransform == null) {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) {
                playerTransform = playerObj.transform;
            }
            else {
                Debug.LogWarning("Player object with tag 'Player' not found.");
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (item == null) {
            Debug.LogWarning("[ItemUI] Item is NULL on click!");
            return;
        }

        Debug.Log("[ItemUI] Pointer clicked: " + eventData.button);

        if (eventData.button == PointerEventData.InputButton.Left) {
            Debug.Log("[ItemUI] Left click detected");
            UseItem();
        }
        else if (eventData.button == PointerEventData.InputButton.Right) {
            Debug.Log("[ItemUI] Right click detected");
            DropItem();
        }
        else {
            Debug.Log("[ItemUI] Other click type: " + eventData.button);
        }
    }

    private void UseItem() {
        Debug.Log("Used item: " + item.itemName);
        // Optional: implement item effect logic here
        Destroy(this.gameObject);
    }

    private void DropItem() {
        if (itemDropPrefab == null || playerTransform == null) {
            Debug.LogWarning("Missing drop prefab or player reference.");
            return;
        }

        Vector2 dropPosition = new Vector2(playerTransform.position.x + 2, playerTransform.position.y);
        GameObject dropped = Instantiate(itemDropPrefab, dropPosition, Quaternion.identity);

        // Assign sprite to the dropped object
        SpriteRenderer sr = dropped.GetComponent<SpriteRenderer>();
        if (sr != null) {
            sr.sprite = item.icon;
        }

        // Assign item data
        ItemPickup pickup = dropped.GetComponent<ItemPickup>();
        if (pickup != null) {
            pickup.item = item;
        }

        Debug.Log("Dropped item: " + item.itemName);
        Destroy(this.gameObject);
    }
}