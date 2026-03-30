using Game.Gameplay.Inventory.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Inventory {
    [DisallowMultipleComponent]
    public sealed class InventorySlotView : MonoBehaviour {
        [Header("UI")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text amountText;
        [SerializeField] private TMP_Text itemNameText;
        [SerializeField] private GameObject emptyStateRoot;
        [SerializeField] private GameObject filledStateRoot;

        public int SlotIndex { get; private set; }

        public void Setup(int slotIndex) {
            SlotIndex = slotIndex;
        }

        public void Bind(InventorySlot slot) {
            if (slot == null || slot.IsEmpty || slot.Item == null) {
                ShowEmpty();
                return;
            }

            if (emptyStateRoot != null) {
                emptyStateRoot.SetActive(false);
            }

            if (filledStateRoot != null) {
                filledStateRoot.SetActive(true);
            }

            if (iconImage != null) {
                iconImage.sprite = slot.Item.icon;
                iconImage.enabled = slot.Item.icon != null;
            }

            if (amountText != null) {
                amountText.text = slot.Amount > 1 ? slot.Amount.ToString() : string.Empty;
            }

            if (itemNameText != null) {
                itemNameText.text = slot.Item.itemName;
            }
        }

        public void ShowEmpty() {
            if (emptyStateRoot != null) {
                emptyStateRoot.SetActive(true);
            }

            if (filledStateRoot != null) {
                filledStateRoot.SetActive(false);
            }

            if (iconImage != null) {
                iconImage.sprite = null;
                iconImage.enabled = false;
            }

            if (amountText != null) {
                amountText.text = string.Empty;
            }

            if (itemNameText != null) {
                itemNameText.text = string.Empty;
            }
        }
    }
}