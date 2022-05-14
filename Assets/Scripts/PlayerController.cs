using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {

    [Header("Movement")]
    public float speed;
    public float sprintMultiplier;

    CharacterController controller;
    bool sprinting => Input.GetKey(sprintKey) && !exhausted && !crouching;
    Vector3 moveDir;

    [Header("Crouching")]
    public float crouchingHeightMulitplier;
    public float crouchingSpeedMultiplier;
    public float crouchAnimLength;

    bool crouch => Input.GetKeyDown(crouchKey) && !crouchAnimPlaying && grounded;
    bool crouching;
    bool crouchAnimPlaying;
    float standingHeight;
    Vector3 standingCenter = Vector3.zero;

    [Header("Physics")]
    public float gravity;
    public float maxGroundDistance;
    public LayerMask groundMask;
    public float defaultVelocity;

    Transform feet;
    Vector3 velocity;
    [SerializeField]
    bool grounded;
    bool fallen = false;

    [Header("Camera")]
    public float sensitivity;
    public float bobSpeed;
    public float bobAmount;
    public Camera cam;

    float xRotation;
    float defaultCamPos;
    float cameraTimer;

    [Header("Footsteps")]
    public float defaultFootstepVol;
    public AudioClip[] stoneAudioClips;
    public AudioClip[] grassAudioClips;
    AudioSource footstepAudioSource;

    bool stepped = false;

    [Header("Stamina")]
    public float maxStaminaAmount;
    public RawImage staminaBar;
    public float exhaustionMultiplier;

    float staminaAmount;
    bool exhausted = false;
    int staminaBarPixelHeight = 16;

    [Header("Interaction")]
    public Inventory inventory;
    public float reach;
    public LayerMask interactableMask;
    public Image crosshair;

    Animator crosshairAnim;
    string crosshairAnimName = "CrosshairInteract";
    bool interact => Input.GetKeyDown(interactKey);
    public int currentItemIndex = 0;
    HoldableItem currentItem;

    [Header("Controls")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.C;
    public KeyCode interactKey = KeyCode.Mouse0;

    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        int customWidth = Mathf.RoundToInt((4f / 3f) * Screen.currentResolution.height);
        Screen.SetResolution(customWidth, Screen.currentResolution.height, FullScreenMode.FullScreenWindow, 60);
        controller = GetComponent<CharacterController>();
        footstepAudioSource = GetComponent<AudioSource>();
        crosshairAnim = crosshair.GetComponent<Animator>();
        cam = transform.GetChild(0).GetComponent<Camera>();
        feet = transform.GetChild(1);
        defaultCamPos = cam.transform.localPosition.y;
        standingHeight = controller.height;
        staminaAmount = maxStaminaAmount;
    }

    void Update() {
        Physics();
        Stamina();
        if (!inventory.inventoryOpen) {
            Camera();
            Movement();
            Interaction();
        }
    }

    void Camera() {
        // Recive input from two axises, adjust sensitivity
        Vector2 input = new Vector2(Input.GetAxis("Mouse X") * sensitivity, Input.GetAxis("Mouse Y") * sensitivity);

        // Clamp X rotation
        xRotation -= input.y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Apply rotations to cam
        cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * input.x);

        // Head bob only when grounded and moving
        if (grounded) {
            if (moveDir != Vector3.zero) {
                // Calculate bob speed and amount based on mulitiplers, bob is simple sin wave
                cameraTimer += Time.deltaTime * (crouching ? bobSpeed * crouchingSpeedMultiplier : sprinting ? bobSpeed * sprintMultiplier : exhausted ? bobSpeed * exhaustionMultiplier : bobSpeed);
                float bob = defaultCamPos + Mathf.Sin(cameraTimer) * (crouching ? bobAmount * crouchingSpeedMultiplier : sprinting ? bobAmount * sprintMultiplier : exhausted ? bobAmount * exhaustionMultiplier : bobAmount);

                // Apply bob and pass through to footstep function
                cam.transform.localPosition = new Vector3(cam.transform.localPosition.x, bob, cam.transform.localPosition.z);
                Footsteps(bob);
            } else {
                //(REMOVED FOR NOW, MIGHT REIMPLEMENT (slightly jarring)) Reset to default pos when stopped
                cam.transform.localPosition = new Vector3(cam.transform.localPosition.x, defaultCamPos, cam.transform.localPosition.z);
            }
        }

    }

    void Footsteps(float bob) {
        if (!stepped) {
            // Change step volume based on player state (ex. Quieter when crouching)
            footstepAudioSource.volume = crouching ? crouchingSpeedMultiplier * defaultFootstepVol : sprinting ? sprintMultiplier * defaultFootstepVol : exhausted ? exhaustionMultiplier * defaultFootstepVol : defaultFootstepVol;

            // Checks if close to lowest point of head bob
            if (Mathf.Sin(cameraTimer) < 0) {
                PlayStepSound();
                stepped = true;
            }
        } else if (Mathf.Sin(cameraTimer) > 0) {
            stepped = false;
        }
    }

    void PlayStepSound() {
        // Raycast below to detect ground type
        Transform hitTransform;
        RaycastHit hit;
        UnityEngine.Physics.Raycast(cam.transform.position, Vector3.down, out hit, controller.height + 0.5f);
        hitTransform = hit.transform;

        // If raycast returns null, use backup sphere cast to detect ground type if player is walking along edge of object
        if (hit.transform == null) {
            hitTransform = UnityEngine.Physics.OverlapSphere(feet.transform.position, 1f)[0].transform;
        }

        // Ground type determined by tag, play random audio clip from array of sounds to corresponding ground type
        switch (hitTransform.tag) {
            case "Stone":
                footstepAudioSource.PlayOneShot(stoneAudioClips[Random.Range(0, stoneAudioClips.Length - 1)]);
                break;
            case "Grass":
                footstepAudioSource.PlayOneShot(grassAudioClips[Random.Range(0, grassAudioClips.Length - 1)]);
                break;
        }
    }

    void Movement() {
        if (crouch || (crouching && Input.GetKeyDown(sprintKey))) StartCoroutine(Crouch());

        // Get movement input for axises and translate into local Vector3
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        moveDir = transform.right * input.x + transform.forward * input.y;

        // Apply movement to player 
        controller.Move(moveDir * (crouching ? speed * crouchingSpeedMultiplier : sprinting ? speed * sprintMultiplier : exhausted ? speed * exhaustionMultiplier : speed) * Time.deltaTime);
    }

    void Physics() {
        // Check if grounded with cast sphere at feet
        grounded = UnityEngine.Physics.CheckSphere(crouching ? feet.position + Vector3.up : feet.position, maxGroundDistance, groundMask);

        // If grounded set to default velocity and play sound
        if (grounded) {
            if (!fallen && velocity.y <= -defaultVelocity * 2) {
                footstepAudioSource.volume = sprintMultiplier * defaultFootstepVol;
                PlayStepSound();
                fallen = true;
            }
            velocity.y = -defaultVelocity;
        } else {
            fallen = false;
        }


        // Continously add gravity and apply velocity to player
        velocity.y -= gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    IEnumerator Crouch() {
        // Check if object above player before standing
        if (crouching && UnityEngine.Physics.Raycast(transform.position, Vector3.up, 2f)) yield break;

        crouchAnimPlaying = true;

        // Reset time of animation to 0 and set variables according to mulipliers
        float elapsedTime = 0;
        float targetHeight = crouching ? standingHeight : standingHeight * crouchingHeightMulitplier;
        float currentHeight = controller.height;
        Vector3 targetCenter = crouching ? standingCenter : standingCenter + (Vector3.up * crouchingHeightMulitplier);
        Vector3 currentCenter = controller.center;

        while (elapsedTime < crouchAnimLength) {
            // Lerp both height and center of player controller from starting to goal height and center
            controller.height = Mathf.Lerp(currentHeight, targetHeight, elapsedTime / crouchAnimLength);
            controller.center = Vector3.Lerp(currentCenter, targetCenter, elapsedTime / crouchAnimLength);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        controller.height = targetHeight;
        controller.center = targetCenter;

        crouching = !crouching;

        crouchAnimPlaying = false;
    }

    void Stamina() {
        // Increase stamina when not sprinting and not maxed out
        if (!sprinting || (sprinting && moveDir == Vector3.zero)) {
            if (staminaAmount != maxStaminaAmount) {
                staminaBar.transform.parent.gameObject.SetActive(true);
                if (staminaAmount <= maxStaminaAmount - Time.deltaTime) {
                    // Increase over time
                    staminaAmount += Time.deltaTime;
                } else {
                    // When close enough to max set stamina to max and ensure exhausted is false
                    staminaAmount = maxStaminaAmount;
                    exhausted = false;
                }
            } else {
                // Turn off stamina when full
                staminaBar.transform.parent.gameObject.SetActive(false);
            }

        } else {
            // Stamina is always shown when going down
            staminaBar.transform.parent.gameObject.SetActive(true);
            if (staminaAmount != 0) {
                if (staminaAmount >= Time.deltaTime) {
                    // Decrease over time
                    staminaAmount -= Time.deltaTime;
                } else {
                    // Set to 0 if close enough and set exhaustion to true
                    staminaAmount = 0;
                    exhausted = true;
                }
            }
        }

        // Send the quantized result to the image bar
        staminaBar.rectTransform.offsetMax = new Vector2(staminaBar.rectTransform.offsetMax.x, Mathf.Floor(-(staminaBarPixelHeight - ((staminaAmount / maxStaminaAmount) * staminaBarPixelHeight))));
    }

    void Interaction() {
        // Raycast forward to any interactable layer objects within reach
        RaycastHit hit;
        UnityEngine.Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, reach, interactableMask);
        if (hit.transform != null) {
            // If not already animating play crosshair to eye animation
            if (!crosshairAnim.GetCurrentAnimatorStateInfo(0).IsName(crosshairAnimName)) {
                crosshairAnim.Play(crosshairAnimName);
            }
            if (interact) {
                hit.transform.GetComponent<InteractableItem>().Action(this);
            }
        } else {
            if (interact) {
                if (currentItemIndex > 0) {
                    inventory.holdableItems[currentItemIndex - 1].GetComponent<HoldableItem>().ItemAction();
                }
            }
            // If not already animating play eye to crosshair animation
            if (!crosshairAnim.GetCurrentAnimatorStateInfo(0).IsName(crosshairAnimName + "Reverse")) {
                crosshairAnim.Play(crosshairAnimName + "Reverse");
            }
        }

        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0) {
            if (currentItemIndex != 0) inventory.holdableItems[currentItemIndex - 1].SetActive(false);
            currentItemIndex++;
        } else if (Input.GetAxis("Mouse ScrollWheel") < 0) {
            if (currentItemIndex != 0) inventory.holdableItems[currentItemIndex - 1].SetActive(false);
            currentItemIndex--;
        }

        if (currentItemIndex > inventory.holdableItems.Count) currentItemIndex = 0;
        else if (currentItemIndex < 0) currentItemIndex = inventory.holdableItems.Count;

        if (currentItemIndex > 0) inventory.holdableItems[currentItemIndex - 1].SetActive(true);
    }
}
