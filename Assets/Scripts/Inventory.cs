using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour {

    public List<GameObject> holdableItems;
    public GameObject book;
    Animator bookAnim;
    public KeyCode inventoryKey = KeyCode.E;

    bool inventory => Input.GetKeyDown(inventoryKey);

    public bool inventoryOpen;

    public List<InventoryItem> inventoryItems = new List<InventoryItem>();

    public LayerMask inventoryItemLayer;

    PlayerController player;
    public GameObject titleText;
    public GameObject descriptionText;

    bool descriptionOpen = false;

    void Start() {
        bookAnim = book.GetComponent<Animator>();
        player = GetComponent<PlayerController>();
    }

    void Update() {

        if (inventory) {
            if (inventoryOpen) {
                bookAnim.Play("CloseBook");
                Cursor.lockState = CursorLockMode.Locked;
                titleText.SetActive(false);
                descriptionText.SetActive(false);
                descriptionOpen = false;
                if (player.currentItemIndex != 0) {
                    holdableItems[player.currentItemIndex - 1].SetActive(true);
                }
            } else {
                bookAnim.Play("OpenBook");
                Cursor.lockState = CursorLockMode.Confined;
                if (player.currentItemIndex != 0) {
                    holdableItems[player.currentItemIndex - 1].SetActive(false);
                }
            }
            player.crosshair.gameObject.SetActive(inventoryOpen);
            inventoryOpen = !inventoryOpen;
        }

        if (inventoryOpen) {
            if (!descriptionOpen) {
                RaycastHit hit;
                float screenRatio = Screen.width / 320f;
                //Debug.Log(screenRatio);
                Physics.Raycast(player.cam.ScreenPointToRay(Input.mousePosition / screenRatio), out hit, 1, inventoryItemLayer);
                if (hit.transform != null) {
                    titleText.SetActive(true);
                    titleText.GetComponentInChildren<Text>().text = GetTitleFromItemInstance(hit.transform.gameObject);
                } else {
                    titleText.SetActive(false);
                }
                if (Input.GetKeyDown(KeyCode.Mouse0)) {
                    if (hit.transform != null) {
                        titleText.SetActive(false);
                        descriptionText.SetActive(true);
                        descriptionText.GetComponentInChildren<Text>().text = GetDescriptionFromItemInstance(hit.transform.gameObject);
                        descriptionOpen = true;
                    }
                }
            } else {
                if (Input.GetKeyDown(KeyCode.Escape)) {
                    descriptionText.SetActive(false);
                    descriptionOpen = false;
                }
            }
        }
    }

    public void AddInventoryItem(int index) {
        inventoryItems[index].instance.SetActive(true);
        inventoryItems[index].acquired = true;
    }

    public string GetTitleFromItemInstance(GameObject instance) {
        for (int i = 0; i < inventoryItems.Count; i++) {
            if (inventoryItems[i].instance == instance) {
                return inventoryItems[i].title;
            }
        }
        return null;
    }

    public string GetDescriptionFromItemInstance(GameObject instance) {
        for (int i = 0; i < inventoryItems.Count; i++) {
            if (inventoryItems[i].instance == instance) {
                return inventoryItems[i].description;
            }
        }
        return null;
    }
}

[System.Serializable]
public class InventoryItem {
    public GameObject instance;
    public bool acquired;

    public string title;
    public string description;
}