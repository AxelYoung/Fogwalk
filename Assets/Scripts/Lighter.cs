using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lighter : MonoBehaviour, HoldableItem {

    public Animator anim;
    bool open = false;

    public void ItemAction() {
        open = !open;
        if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1) {
            if (open) anim.Play("OpenLighter");
            else anim.Play("CloseLighter");
        }
    }

    void OnEnable() {
        if (open) anim.Play("OpenLighter", 0, 1);
        else anim.Play("CloseLighter", 0, 1);
    }
}
