using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Game.Gameplay.Items.Runtime;
using Game.Player.InventoryAccess;
using UnityEngine;

namespace Game.Gameplay.Inventory.Presentation {
    [DisallowMultipleComponent]
    public sealed class LegacyInventorySystemAdapter : MonoBehaviour {
        [Header("References")]
        [SerializeField] private PlayerInventoryController playerInventoryController;
        [SerializeField] private MonoBehaviour legacyInventorySystemComponent;

        [Header("Import")]
        [SerializeField] private bool autoResolveReferences = true;
        [SerializeField] private bool importOnStart = true;
        [SerializeField] private bool importGridSize = true;
        [SerializeField] private bool importContents = true;
        [SerializeField] private bool importOpenState = true;
        [SerializeField] private bool clearNewInventoryBeforeImport = true;

        [Header("Legacy State")]
        [SerializeField] private bool disableLegacyComponentAfterImport = false;
        [SerializeField] private bool mirrorPlayerOpenStateBackToLegacy = false;
        [SerializeField] private bool verboseLogging = true;

        private bool lastKnownPlayerOpenState;

        private void Awake() {
            if (autoResolveReferences) {
                ResolveMissingReferences();
            }
        }

        private void Start() {
            if (importOnStart) {
                ImportFromLegacy();
            }

            if (playerInventoryController != null) {
                lastKnownPlayerOpenState = playerInventoryController.IsOpen;
            }
        }

        private void Update() {
            if (
                !mirrorPlayerOpenStateBackToLegacy ||
                playerInventoryController == null ||
                legacyInventorySystemComponent == null
            ) {
                return;
            }

            if (lastKnownPlayerOpenState == playerInventoryController.IsOpen) {
                return;
            }

            lastKnownPlayerOpenState = playerInventoryController.IsOpen;
            PushOpenStateToLegacy(lastKnownPlayerOpenState);
        }

        [ContextMenu("Resolve Missing References")]
        public void ResolveMissingReferences() {
            if (playerInventoryController == null) {
                playerInventoryController = FindObjectOfType<PlayerInventoryController>();
            }

            if (legacyInventorySystemComponent == null) {
                MonoBehaviour[] behaviours = FindObjectsOfType<MonoBehaviour>();

                for (int i = 0; i < behaviours.Length; i++) {
                    MonoBehaviour candidate = behaviours[i];

                    if (candidate == null) continue;

                    if (candidate.GetType().Name == "InventorySystem") {
                        legacyInventorySystemComponent = candidate;
                        break;
                    }
                }
            }
        }

        [ContextMenu("Import From Legacy")]
        public void ImportFromLegacy() {
            ResolveMissingReferences();

            if (playerInventoryController == null) {
                Debug.LogWarning("LegacyInventorySystemAdapter: nincs PlayerInventoryController.", this);
                return;
            }

            if (legacyInventorySystemComponent == null) {
                Debug.LogWarning("LegacyInventorySystemAdapter: nincs legacy InventorySystem komponens.", this);
                return;
            }

            if (importGridSize) {
                ImportLegacyGridSize();
            }

            if (importContents) {
                ImportLegacyOpenState();
            }

            if (disableLegacyComponentAfterImport) {
                legacyInventorySystemComponent.enabled = false;
            }

            if (verboseLogging) {
                Debug.LogWarning("LegacyInventorySystemAdapter: nincs legacy InventorySystem komponens.", this);
            }
        }

        private void ImportLegacyGridSize() {
            if (TryReadIntMember(legacyInventorySystemComponent, "rows", out int rows) &&
                TryReadIntMember(legacyInventorySystemComponent, "columns", out int columns) &&
                rows > 0 && columns > 0) {
                    playerInventoryController.ResizeGrid(rows, columns, false);
                    return;
            }

            if (TryReadIntMember(legacyInventorySystemComponent, "Capacity", out int capacity) &&
                capacity > 0) {
                    playerInventoryController.ResizeGrid(1, capacity, false);
            }
        }

        private void ImportLegacyContents() {
            IEnumerable slotsEnumerable = ReadMemberValue(legacyInventorySystemComponent, "Slots") as IEnumerable;

            if (slotsEnumerable == null) {
                if (verboseLogging) {
                    Debug.LogWarning("LegacyInventorySystemAdapter: a legacy Slots lista nem olvasható.", this);
                }
                return;
            }

            List<ItemStack> importedStacks = new List<ItemStack>();

            foreach (object slotObject in slotsEnumerable) {
                if (slotObject == null) {
                    importedStacks.Add(ItemStack.Empty);
                    continue;
                }

                InventoryItemData item = ReadMemberValue(slotObject, "item") as InventoryItemData ??
                    ReadMemberValue(slotObject, "Item") as InventoryItemData;

                int amount = ReadIntMember(slotObject, "amount", 0);
                if (amount <= 0) {
                    amount = ReadIntMember(slotObject, "Amount", 0);
                }

                importedStacks.Add(item != null && amount > 0 
                    ? new ItemStack(item, amount)
                    : ItemStack.Empty);
            }

            playerInventoryController.LoadFromStacks(importedStacks, clearNewInventoryBeforeImport);
        }

        private void ImportLegacyOpenState() {
            if (!TryReadBoolMember(legacyInventorySystemComponent, "IsOpen", out bool isOpen)) {
                return;
            }

            playerInventoryController.SetOpen(isOpen, true);
        }

        private void PushOpenStateToLegacy(bool open) {
            if (legacyInventorySystemComponent == null) return;

            MethodInfo openMethod = legacyInventorySystemComponent.GetType().GetMethod(
                open ? "Open" : "Close",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );

            if (openMethod != null) {
                openMethod.Invoke(legacyInventorySystemComponent, null);
            }
        }

        private static bool TryReadIntMember(object target, string memberName, out int value) {
            object raw = ReadMemberValue(target, memberName);

            if (raw is int intValue) {
                value = intValue;
                return true;
            }

            value = 0;
            return false;
        }

        private static int ReadIntMember(object target, string memberName, int fallback) {
            return TryReadIntMember(target, memberName, out int value) ? value : fallback;
        }

        private static bool TryReadBoolMember(object target, string memberName, out bool value) {
            object raw = ReadMemberValue(target, memberName);

            if (raw is bool boolValue) {
                value = boolValue;
                return true;
            }

            value = false;
            return false;
        }

        private static object ReadMemberValue(object target, string memberName) {
            if (target == null || string.IsNullOrWhiteSpace(memberName)) {
                return null;
            }

            Type type = target.GetType();

            PropertyInfo property = type.GetProperty(
                memberName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );

            if (property != null) {
                return property.GetValue(target);
            }

            FieldInfo field = type.GetField(
                memberName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );

            return field != null ? field.GetValue(target) : null;
        }
    }
}