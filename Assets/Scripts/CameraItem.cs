using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraItem : MonoBehaviour, HoldableItem {
    public Animator anim;

    public void ItemAction() {
        anim.Play("Flash");
    }

    void OnEnable() {
        anim.Play("Flash", 0, 1);
    }
}