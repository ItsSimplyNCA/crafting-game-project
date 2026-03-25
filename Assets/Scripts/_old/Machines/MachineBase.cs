using System.Collections.Generic;
using UnityEngine;

public abstract class MachineBase : PlacedObject {
    [Header("Machine Inventory")]
    [SerializeField, Min(1)] protected int slotCount = 1;
    [SerializeField] protected List<InventorySlotData> slots = new();

    public IReadOnlyList<InventorySlotData> Slots => slots;

    protected virtual void Awake() {
        EnsureSlotCount();
    }

    protected virtual void OnValidate() {
        size.x = Mathf.Max(1, size.x);
        size.y = Mathf.Max(1, size.y);
        height = Mathf.Max(1, height);

        slotCount = Mathf.Max(1, slotCount);
        EnsureSlotCount();
    }

    public InventorySlotData GetSlot(int index) {
        if (index < 0 || index >= slots.Count) return null;
        return slots[index];
    }

    protected bool CanAddToSlot(int index, InventoryItemData item, int amount = 1) {
        if (item == null || amount <= 0) return false;

        InventorySlotData slot = GetSlot(index);
        if (slot == null) return false;

        if (slot.IsEmpty) return amount <= item.maxStack;

        if (slot.item != item) return false;

        return slot.amount + amount <= item.maxStack;
    }

    protected bool TryAddToSlot(int index, InventoryItemData item, int amount = 1) {
        if (!CanAddToSlot(index, item, amount)) return false;

        InventorySlotData slot = GetSlot(index);

        if (slot.IsEmpty) slot.Set(item, amount);
        else slot.amount += amount;

        return true;
    }

    public bool TryTakeFromSlot(int index, int amount, out InventoryItemData itemData) {
        itemData = null;

        if (amount <= 0) return false;

        InventorySlotData slot = GetSlot(index);
        if (slot == null || slot.IsEmpty || slot.amount < amount) return false;

        itemData = slot.item;
        slot.amount -= amount;

        if (slot.amount <= 0) slot.Clear();

        return true;
    }

    public bool IsSlotFull(int index) {
        InventorySlotData slot = GetSlot(index);

        if (slot == null || slot.IsEmpty || slot.item == null) {
            return false;
        }

        return slot.amount >= slot.item.maxStack;
    }

    protected void EnsureSlotCount() {
        if (slots == null) {
            slots = new List<InventorySlotData>();
        }

        while (slots.Count < slotCount) {
            slots.Add(new InventorySlotData());
        }

        while (slots.Count > slotCount) {
            slots.RemoveAt(slots.Count - 1);
        }
    }
}
