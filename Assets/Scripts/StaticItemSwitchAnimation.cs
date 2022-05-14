using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticItemSwitchAnimation : MonoBehaviour, InteractableItem {
    public Animator anim;
    bool open = false;

    public int keyIndex;

    public void Action(PlayerController player) {
        if (player.inventory.inventoryItems[keyIndex].acquired && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1) {
            open = !open;
            if (open) anim.Play("DoorOpen");
            else anim.Play("DoorClose");
        }
    }

}
