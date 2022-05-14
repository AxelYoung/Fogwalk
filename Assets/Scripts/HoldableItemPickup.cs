using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldableItemPickup : MonoBehaviour, InteractableItem {
    public Vector3 holdPos;
    public Vector3 holdRot;
    public Vector3 holdScale;

    public void Action(PlayerController player) {
        player.inventory.holdableItems.Add(gameObject);
        SetLayerRecursively(gameObject, 8);
        transform.parent = player.cam.transform;
        transform.localPosition = holdPos;
        transform.localEulerAngles = holdRot;
        transform.localScale = holdScale;
        gameObject.SetActive(false);
    }


    public void SetLayerRecursively(GameObject obj, int layer) {
        obj.layer = layer;
        foreach (Transform child in obj.transform) {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}
