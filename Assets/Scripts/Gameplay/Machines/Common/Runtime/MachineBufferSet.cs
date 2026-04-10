using System;
using System.Collections.Generic;
using Game.Gameplay.Inventory.Runtime;
using UnityEngine;

namespace Game.Gameplay.Machines.Common.Runtime {
    [Serializable]
    public sealed class MachineBufferSet {
        [SerializeField] private InventoryContainer input;
        [SerializeField] private InventoryContainer output;
        [SerializeField] private InventoryContainer internalStorage;

        public event Action Changed;

        public InventoryContainer Input => input;
        public InventoryContainer Output => output;
        public InventoryContainer InternalStorage => internalStorage;

        public bool HasInput => input != null;
        public bool HasOutput => output != null;
        public bool HasInternalStorage => internalStorage != null;

        public MachineBufferSet(int inputSlots, int outputSlots, int internalSlots) {
            Rebuild(inputSlots, outputSlots, internalSlots);
        }

        public void Rebuild(int inputSlots, int outputSlots, int internalSlots) {
            UnbindContainer();

            input = inputSlots > 0 ? new InventoryContainer(inputSlots) : null;
            output = outputSlots > 0 ? new InventoryContainer(outputSlots) : null;
            internalStorage = internalSlots > 0 ? new InventoryContainer(internalSlots) : null;

            BindContainers();
            NotifyChanged();
        }

        public bool CanFitInput(InventoryItemData item, int amount = 1) {
            return input != null && input.CanFit(item, amount);
        }

        public bool CanFitOutput(InventoryItemData item, int amount = 1) {
            return output != null && output.CanFit(item, amount);
        }

        public bool CanFitInternal(InventoryItemData item, int amount = 1) {
            return internalStorage != null && internalStorage.CanFit(item, amount);
        }

        public bool AddToInput(InventoryItemData item, int amount = 1) {
            return input != null && input.AddItem(item, amount);
        }

        public bool AddToOutput(InventoryItemData item, int amount = 1) {
            return output != null && output.AddItem(item, amount);
        }

        public bool AddToInternal(InventoryItemData item, int amount = 1) {
            return internalStorage != null && internalStorage.AddItem(item, amount);
        }

        public bool RemoveFromInput(InventoryItemData item, int amount = 1) {
            return input != null && input.RemoveItem(item, amount);
        }

        public bool RemoveFromOutput(InventoryItemData item, int amount = 1) {
            return output != null && output.RemoveItem(item, amount);
        }

        public bool RemoveFromInternal(InventoryItemData item, int amount = 1) {
            return internalStorage != null && internalStorage.RemoveItem(item, amount);
        }

        public int CountInput(InventoryItemData item) {
            return input != null ? input.CountItem(item) : 0;
        }

        public int CountOutput(InventoryItemData item) {
            return output != null ? output.CountItem(item) : 0;
        }

        public int CountInternal(InventoryItemData item) {
            return internalStorage != null ? internalStorage.CountItem(item) : 0;
        }

        public bool TryTakeFirstOutputStack(out InventoryItemData item, out int amount) {
            item = null;
            amount = 0;

            if (output == null) return false;

            for (int i = 0; i < output.Slots.Count; i++) {
                InventorySlot slot = output.GetSlot(i);

                if (slot == null || slot.IsEmpty || slot.Item == null) continue;

                item = slot.Item;
                amount = slot.Amount;
                slot.Clear();
                NotifyChanged();
                return true;
            }

            return false;
        }

        public bool TryTakeFromOutputSlot(int index, int amount, out InventoryItemData item) {
            item = null;
            if (output == null) return false;
            return output.TryTakeFromSlot(index, amount, out item);
        }

        public void ClearAll() {
            input?.Clear();
            output?.Clear();
            internalStorage?.Clear();
            NotifyChanged();
        }

        public IEnumerable<InventoryContainer> EnumerateContainers() {
            if (input != null) yield return input;
            if (output != null) yield return output;
            if (internalStorage != null) yield return internalStorage;
        }

        private void BindContainers() {
            if (input != null) input.Changed += HandleContainerChanged;
            if (output != null) output.Changed += HandleContainerChanged;
            if (internalStorage != null) internalStorage.Changed += HandleContainerChanged;
        }

        private void UnbindContainer() {
            if (input != null) input.Changed -= HandleContainerChanged;
            if (output != null) output.Changed -= HandleContainerChanged;
            if (internalStorage != null) internalStorage.Changed -= HandleContainerChanged;
        }

        private void HandleContainerChanged() {
            NotifyChanged();
        }

        private void NotifyChanged() {
            Changed?.Invoke();
        }
    }
}