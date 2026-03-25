using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour {
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text amountText;

    public void Refresh(InventorySlotData slot) {
        bool hasItem = slot != null && !slot.IsEmpty;

        if (iconImage != null) {
            iconImage.enabled = hasItem;
            iconImage.sprite = hasItem ? slot.item.icon : null;
        }

        if (amountText != null) {
            amountText.text = hasItem && slot.amount > 1 ? slot.amount.ToString() : string.Empty;
        }
    }
}
