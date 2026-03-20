using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour {
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Transform slotParent;
    [SerializeField] private InventorySlotUI slotPrefab;

    private readonly List<InventorySlotUI> spawnedSlots = new();
    private InventorySystem inventory;

    private void Awake() {
        if (panelRoot != null) panelRoot.SetActive(false);
    }
    
    private void Start() {
        inventory = InventorySystem.Instance;

        if (inventory == null) {
            Debug.LogError("InvetoryUI: nincs InvetorySystem a scene-ben.");
            enabled = false;
            return;
        }

        BuildSlots();

        inventory.OnInventoryChanged += Refresh;
        inventory.OnInventoryOpenChanged += HandleOpenChanged;

        HandleOpenChanged(inventory.IsOpen);
        Refresh();
    }

    private void OnDestroy() {
        if (inventory == null) return;

        inventory.OnInventoryChanged -= Refresh;
        inventory.OnInventoryOpenChanged -= HandleOpenChanged;
    }

    private void BuildSlots() {
        if (slotParent == null || slotPrefab == null) {
            Debug.LogError("InventoryUI: hiányzik a slotParent vagy a slotPrefab referencia.");
            return;
        }

        for (int i = slotParent.childCount - 1; i >= 0; i--) {
            Destroy(slotParent.GetChild(i).gameObject);
        }

        spawnedSlots.Clear();

        for (int i = 0; i < inventory.Capacity; i++) {
            InventorySlotUI slot = Instantiate(slotPrefab, slotParent);
            spawnedSlots.Add(slot);
        }
    }

    private void Refresh() {
        for (int i = 0; i < spawnedSlots.Count; i++) {
            spawnedSlots[i].Refresh(inventory.GetSlot(i));
        }
    }

    private void HandleOpenChanged(bool isOpen) {
        if (panelRoot != null) {
            panelRoot.SetActive(isOpen);
        }
    }
}
