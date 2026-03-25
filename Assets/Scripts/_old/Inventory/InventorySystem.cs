using System;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour {
    public static InventorySystem Instance { get; private set; }

    [Header("Invetory Size")]
    [SerializeField, Min(1)] private int rows = 5;
    [SerializeField, Min(1)] private int columns = 5;

    [SerializeField] private List<InventorySlotData> slots = new();

    public event Action OnInventoryChanged;
    public event Action<bool> OnInventoryOpenChanged;

    public bool IsOpen { get; private set; }
    public int Capacity => rows * columns;
    public IReadOnlyList<InventorySlotData> Slots => slots;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        EnsureSlotCount();
        SetOpen(false, true);
    }

    private void OnValidate() {
        rows = Mathf.Max(1, rows);
        columns = Mathf.Max(1, columns);
        EnsureSlotCount();
    }

    public void Toggle() => SetOpen(!IsOpen);
    public void Open() => SetOpen(true);
    public void Close() => SetOpen(false);

    public InventorySlotData GetSlot(int index) {
        if (index < 0 || index >= slots.Count) return null;
        return slots[index];
    }

    public bool AddItem(InventoryItemData item, int amount = 1) {
        if (item == null || amount <= 0) return false;

        int remaining = amount;

        for (int i = 0; i < slots.Count; i++) {
            InventorySlotData slot = slots[i];

            if (slot.IsEmpty || slot.item != item || slot.amount >= item.maxStack) continue;

            int addAmount = Mathf.Min(remaining, item.maxStack - slot.amount);
            slot.amount += addAmount;
            remaining -= addAmount;

            if (remaining <= 0) {
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        for (int i = 0; i < slots.Count; i++) {
            InventorySlotData slot = slots[i];

            if (!slot.IsEmpty) continue;

            int addAmount = Mathf.Min(remaining, item.maxStack);
            slot.Set(item, addAmount);
            remaining -= addAmount;

            if (remaining <= 0) {
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        OnInventoryChanged?.Invoke();
        return remaining < amount;
    }

    public bool RemoveItem(InventoryItemData item, int amount = 1) {
        if (item == null || amount <= 0) return false;

        int remaining = amount;
        bool changed = false;

        for (int i = 0; i < slots.Count; i++) {
            InventorySlotData slot = slots[i];

            if (slot.IsEmpty || slot.item != item) continue;

            int removeAmount = Mathf.Min(remaining, slot.amount);
            slot.amount -= removeAmount;
            remaining -= removeAmount;
            changed = true;

            if (slot.amount <= 0) slot.Clear();
            if (remaining <= 0) break;
        }

        if (changed) {
            OnInventoryChanged?.Invoke();
        }

        return remaining <= 0;
    }

    private void SetOpen(bool open, bool force = false) {
        if (!force && IsOpen == open) return;

        IsOpen = open;

        if (IsOpen) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        } else {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        OnInventoryOpenChanged?.Invoke(IsOpen);
    }

    private void EnsureSlotCount() {
        if (slots == null) {
            slots = new List<InventorySlotData>();
        }

        while (slots.Count < Capacity) {
            slots.Add(new InventorySlotData());
        }

        while (slots.Count > Capacity) {
            slots.RemoveAt(slots.Count - 1);
        }
    }

    [ContextMenu("Debug Print Inventory")]
    public void DebugPrintInventory() {
        Debug.Log("=== INVETORY DEBUG ===");
        for (int i = 0; i < slots.Count; i++) {
            InventorySlotData slot = slots[i];

            if (slot == null) {
                Debug.Log($"Slot {i}: NULL");
                continue;
            }

            if (slot.IsEmpty) {
                Debug.Log($"Slot {i}: EMPTY");
                continue;
            }

            Debug.Log($"Slot {i}: {slot.item.itemName} x{slot.amount}");
        }
    }
}
