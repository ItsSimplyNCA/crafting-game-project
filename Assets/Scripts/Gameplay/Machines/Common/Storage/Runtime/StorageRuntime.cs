using System;
using Game.Gameplay.Inventory.Runtime;
using Game.Gameplay.WorldEntities.Runtime;
using UnityEngine;

namespace Game.Gameplay.Machines.Storage.Runtime {
    [Serializable]
    public sealed class StorageRuntime {
        private readonly string runtimeId;
        private readonly PlaceableRuntime placeableRuntime;
        private readonly InventoryContainer storage;

        public event Action<StorageRuntime> Changed;

        public string RuntimeId => runtimeId;
        public PlaceableRuntime PlaceableRuntime => placeableRuntime;
        public InventoryContainer Storage => storage;

        public StorageRuntime(PlaceableRuntime placeableRuntime, int slotCount) {
            if (placeableRuntime == null) {
                throw new ArgumentNullException(nameof(placeableRuntime));
            }

            runtimeId = Guid.NewGuid().ToString("N");
            this.placeableRuntime = placeableRuntime;
            storage = new InventoryContainer(Mathf.Max(1, slotCount));
            storage.Changed += HandleStorageChanged;
        }

        public bool AddItem(InventoryItemData item, int amount = 1) {
            return storage.AddItem(item, amount);
        }

        public bool RemoveItem(InventoryItemData item, int amount = 1) {
            return storage.RemoveItem(item, amount);
        }

        public bool CanFit(InventoryItemData item, int amount = 1) {
            return storage.CanFit(item, amount);
        }

        public int CountItem(InventoryItemData item) {
            return storage.CountItem(item);
        }

        public InventorySlot GetSlot(int index) {
            return storage.GetSlot(index);
        }

        public void Clear() {
            storage.Clear();
        }

        private void HandleStorageChanged() {
            Changed?.Invoke(this);
        }
    }
}