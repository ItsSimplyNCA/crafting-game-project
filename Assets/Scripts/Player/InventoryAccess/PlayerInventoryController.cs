using System;
using System.Collections.Generic;
using Game.Gameplay.Inventory.Runtime;
using Game.Gameplay.Items.Runtime;
using Unity.VisualScripting;
using UnityEngine;

namespace Game.Player.InventoryAccess {
    [DisallowMultipleComponent]
    public sealed class PlayerInventoryController : MonoBehaviour {
        [Header("Inventory Size")]
        [SerializeField, Min(1)] private int rows = 5;
        [SerializeField, Min(1)] private int columns = 5;

        [Header("Input")]
        [SerializeField] private bool allowToggleInput = true;
        [SerializeField] private KeyCode toggleKey = KeyCode.Tab;

        [Header("Cursor")]
        [SerializeField] private bool manageCursor = true;
        [SerializeField] private CursorLockMode openCursorLockMode = CursorLockMode.None;
        [SerializeField] private bool openCursorVisible = true;
        [SerializeField] private CursorLockMode closedCursorLockMode = CursorLockMode.Locked;
        [SerializeField] private bool closedCursorVisible = false;

        [Header("Startup")]
        [SerializeField] private bool startOpen = false;

        private InventoryContainer container;

        public event Action Changed;
        public event Action<bool> OpenStateChanged;

        public InventoryContainer Container => container;
        public IReadOnlyList<InventorySlot> Slots => container != null ? container.Slots : Array.Empty<InventorySlot>();
        public int Rows => rows;
        public int Columns => columns;
        public int Capacity => Mathf.Max(1, rows) * Mathf.Max(1, columns);
        public bool IsOpen { get; private set; }

        private void Awake() {
            EnsureContainer();
            SetOpen(startOpen, true);
        }

        private void OnDestroy() {
            UnbindContainer();
        }

        private void OnValidate() {
            rows = Mathf.Max(1, rows);
            columns = Mathf.Max(1, columns);

            if (Application.isPlaying) {
                ResizeGrid(rows, columns, true);
            }
        }

        private void Update() {
            if (!allowToggleInput) return;
            if (toggleKey != KeyCode.None && Input.GetKeyDown(toggleKey)) {
                Toggle();
            }
        }

        public void Toggle() {
            SetOpen(!IsOpen);
        }

        public void Open() {
            SetOpen(true);
        }

        public void Close() {
            SetOpen(false);
        }

        public void SetOpen(bool open, bool force = false) {
            if (!force && IsOpen == open) return;

            IsOpen = open;
            ApplyCursorState(open);
            OpenStateChanged?.Invoke(IsOpen);
        }

        public void ResizeGrid(int newRows, int newColumns, bool preserveContents = true) {
            newRows = Mathf.Max(1, newRows);
            newColumns = Mathf.Max(1, newColumns);

            if (
                rows == newRows &&
                columns == newColumns &&
                container != null &&
                container.Capacity == newRows * newColumns
            ) return;

            List<ItemStack> snapshot = preserveContents ? CaptureStacks() : null;

            rows = newRows;
            columns = newColumns;

            UnbindContainer();
            container = new InventoryContainer(Capacity);
            container.Changed += HandleContainerChanged;

            if (snapshot != null) {
                LoadFromStacks(snapshot, true);
            } else {
                NotifyChanged();
            }
        }

        public bool AddItem(InventoryItemData item, int amount = 1) {
            EnsureContainer();
            return container.AddItem(item, amount);
        }

        public bool RemoveItem(InventoryItemData item, int amount = 1) {
            EnsureContainer();
            return container.RemoveItem(item, amount);
        }

        public bool CanFit(InventoryItemData item, int amount = 1) {
            EnsureContainer();
            return container.CanFit(item, amount);
        }

        public int CountItem(InventoryItemData item) {
            EnsureContainer();
            return container.CountItem(item);
        }

        public InventorySlot GetSlot(int index) {
            EnsureContainer();
            return container.GetSlot(index);
        }

        public void ClearInventory() {
            EnsureContainer();
            container.Clear();
        }

        public void LoadFromStacks(IEnumerable<ItemStack> stacks, bool clearExisting = true) {
            EnsureContainer();

            if (clearExisting) container.Clear();
            if (stacks == null) return;

            foreach (ItemStack stack in stacks) {
                if (stack.IsEmpty) continue;
                container.AddItem(stack.Item, stack.Amount);
            }
        }

        public List<ItemStack> CaptureStacks() {
            EnsureContainer();

            List<ItemStack> result = new List<ItemStack>(container.Capacity);

            for (int i = 0; i < container.Slots.Count; i++) {
                InventorySlot slot = container.Slots[i];
                result.Add(slot != null ? slot.Stack : ItemStack.Empty);
            }

            return result;
        }

        private void EnsureContainer() {
            if (container != null) return;
            container = new InventoryContainer(Capacity);
            container.Changed += HandleContainerChanged;
        }

        private void UnbindContainer() {
            if (container != null) {
                container.Changed -= HandleContainerChanged;
            }
        }

        private void HandleContainerChanged() {
            NotifyChanged();
        }

        private void NotifyChanged() {
            Changed?.Invoke();
        }

        private void ApplyCursorState(bool open) {
            if (!manageCursor) return;
            Cursor.lockState = open ? openCursorLockMode : closedCursorLockMode;
            Cursor.visible = open ? openCursorVisible : closedCursorVisible;
        }
    }
}