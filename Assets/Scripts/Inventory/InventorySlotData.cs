using System;
using UnityEngine;

[Serializable]
public class InventorySlotData {
    public InventoryItemData item;
    public int amount;

    public bool IsEmpty => item == null || amount <= 0;

    public void Set(InventoryItemData newItem, int newAmount) {
        item = newItem;
        amount = Mathf.Max(0, newAmount);

        if (amount == 0) Clear();
    }

    public void Clear() {
        item = null;
        amount = 0;
    }
}
