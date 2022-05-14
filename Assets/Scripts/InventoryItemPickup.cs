using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryItemPickup : MonoBehaviour, InteractableItem {
    public int itemIndex;
    public void Action(PlayerController player) {
        player.inventory.AddInventoryItem(itemIndex);
        Destroy(gameObject);
    }
}
