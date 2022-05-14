using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTest : MonoBehaviour {

    public AudioSource front;
    public AudioSource back;
    public AudioSource right;
    public AudioSource left;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.W)) front.Play();

        if (Input.GetKeyDown(KeyCode.S)) back.Play();

        if (Input.GetKeyDown(KeyCode.D)) right.Play();

        if (Input.GetKeyDown(KeyCode.A)) left.Play();

    }
}
