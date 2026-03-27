using System.Reflection;
using Game.Gameplay.Grid.Runtime;
using UnityEngine;

namespace Game.Gameplay.Grid.Presentation {
    [DisallowMultipleComponent]
    public sealed class LegacyGridMigrationBridge : MonoBehaviour {
        [Header("References")]
        [SerializeField] private WorldGrid legacyWorldGrid;
        [SerializeField] private GridService gridService;

        [Header("Options")]
        [SerializeField] private bool autoResolveReferences = true;
        [SerializeField] private bool migrateOnAwake = false;
        [SerializeField] private bool verboseLogging = true;

        public WorldGrid LegacyWorldGrid => legacyWorldGrid;
        public GridService GridService => gridService;

        private void Awake() {
            if (autoResolveReferences) {
                ResolveMissingReferences();
            }

            if (migrateOnAwake) {
                MigrateSettings();
            }
        }

        private void OnValidate() {
            if (!Application.isPlaying && autoResolveReferences) {
                ResolveMissingReferences();
            }
        }

        [ContextMenu("Resolve Missing References")]
        public void ResolveMissingReferences() {
            if (legacyWorldGrid == null) {
                legacyWorldGrid = FindObjectOfType<WorldGrid>();
            }

            if (gridService == null) {
                gridService = FindObjectOfType<GridService>();
            }
        }

        [ContextMenu("Migrate Grid Settings")]
        public void MigrateSettings() {
            if (legacyWorldGrid == null || gridService == null) {
                Debug.LogWarning("LegacyGridMigrationBridge: hiányzik a legacy WorldGrid vagy az új GridService.", this);
                return;
            }

            SetSerializedField(gridService, "width", legacyWorldGrid.width);
            SetSerializedField(gridService, "length", legacyWorldGrid.length);
            SetSerializedField(gridService, "maxHeight", legacyWorldGrid.maxHeight);
            SetSerializedField(gridService, "cellSize", legacyWorldGrid.cellSize);
            SetSerializedField(gridService, "cellHeight", legacyWorldGrid.cellHeight);
            SetSerializedField(gridService, "origin", legacyWorldGrid.origin);

            if (verboseLogging) {
                Debug.Log(
                    "LegacyGridMigrationBridge: grid beállítások átmigrálva a WorldGrid-ről a GridService-be.",
                    this
                );
            }
        }

        private static void SetSerializedField<T>(object target, string fieldName, T value) {
            if (target == null) return;

            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
            );

            if (field == null) {
                Debug.LogError($"LegacyGridMigrationBridge: nem található mező: {fieldName}");
                return;
            }

            field.SetValue(target, value);
        }
    }


}