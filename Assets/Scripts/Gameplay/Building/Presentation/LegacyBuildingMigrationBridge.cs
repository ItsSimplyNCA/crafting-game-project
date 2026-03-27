using System.Collections.Generic;
using System.Reflection;
using Game.Gameplay.Building.Data;
using Game.Gameplay.Building.Runtime;
using Game.Gameplay.WorldEntities.Data;
using Game.Gameplay.WorldEntities.Presentation;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Gameplay.Building.Presentation {
    [DisallowMultipleComponent]
    public sealed class LegacyBuildingMigrationBridge : MonoBehaviour {
        [Header("Legacy")]
        [SerializeField] private BuildingSystem legacyBuildingSystem;

        [Header("New Stack")]
        [SerializeField] private BuildModeController buildModeController;
        [SerializeField] private BuildPreviewController buildPreviewController;
        [SerializeField] private BuildRaycastController buildRaycastController;
        [SerializeField] private PlacementService placementService;
        [SerializeField] private DismantleService dismantleService;

        [Header("Migration Options")]
        [SerializeField] private bool autoResolveReferences = true;
        [SerializeField] private bool migrateOnAwake = false;
        [SerializeField] private bool copyRaycastSettings = true;
        [SerializeField] private bool copyPreviewMaterials = true;
        [SerializeField] private bool rebuildCatalogFromLegacyPrefabs = true;
        [SerializeField] private bool disableLegacyBuildingSystemAfterMigration = true;
        [SerializeField] private bool verboseLogging = true;

        public BuildingSystem LegacyBuildingSystem => legacyBuildingSystem;

        private void OnValidate() {
            if (!Application.isPlaying && autoResolveReferences) {
                ResolveMissingReferences();
            }
        }

        [ContextMenu("Resolve Missing References")]
        public void ResolveMissingReferences() {
            if (legacyBuildingSystem == null) {
                legacyBuildingSystem = FindObjectOfType<BuildingSystem>();
            }

            if (buildModeController == null) {
                buildModeController = FindObjectOfType<BuildModeController>();
            }

            if (buildPreviewController == null) {
                buildPreviewController = FindObjectOfType<BuildPreviewController>();
            }

            if (buildRaycastController == null) {
                buildRaycastController = FindObjectOfType<BuildRaycastController>();
            }

            if (placementService == null) {
                placementService = FindObjectOfType<PlacementService>();
            }

            if (dismantleService == null) {
                dismantleService = FindObjectOfType<DismantleService>();
            }
        }

        [ContextMenu("Migrate Legacy Building System")]
        public void Migrate() {
            if (legacyBuildingSystem == null) {
                Debug.LogWarning("LegacyBuildingMigrationBridge: nincs legacy BuildingSystem.", this);
                return;
            }

            ResolveMissingReferences();

            if (copyRaycastSettings) {
                MigrateRaycastSettings();
            }

            if (copyPreviewMaterials) {
                MigratePreviewMaterials();
            }

            if (rebuildCatalogFromLegacyPrefabs) {
                RebuildCatalogFromLegacyPrefabs();
            }

            if (buildModeController != null) {
                buildModeController.ResolveMissingReferences();
            }

            if (buildPreviewController != null) {
                buildPreviewController.ResolveMissingReferences();
                buildPreviewController.RefreshPreview();
            }

            if (disableLegacyBuildingSystemAfterMigration && legacyBuildingSystem != null) {
                legacyBuildingSystem.enabled = false;
            }

            if (verboseLogging) {
                Debug.Log("LegacyBuildingMigrationBridge: migráció kész.", this);
            }
        }

        private void MigrateRaycastSettings() {
            if (buildRaycastController == null) return;

            Camera legacyCamera = GetPrivateField<Camera>(legacyBuildingSystem, "playerCamera");
            LayerMask legacyPlacementMask = GetPrivateField<LayerMask>(legacyBuildingSystem, "placementMask");
            float legacyMaxBuildDistance = GetPrivateField<float>(legacyBuildingSystem, "maxBuildDistance");

            if (legacyCamera != null) {
                SetPrivateField(buildRaycastController, "playerCamera", legacyCamera);
            }

            SetPrivateField(buildRaycastController, "placementMask", legacyPlacementMask);

            if (legacyMaxBuildDistance > 0f) {
                SetPrivateField(buildRaycastController, "maxBuildDistance", legacyMaxBuildDistance);
            }

            if (verboseLogging) {
                Debug.Log("LegacyBuildingMigrationBridge: migráció kész.", this);
            }
        }

        private void MigratePreviewMaterials() {
            if (buildPreviewController == null) return;

            Material validPreviewMaterial = GetPrivateField<Material>(legacyBuildingSystem, "validPreviewMaterial");
            Material invalidPreviewMaterial = GetPrivateField<Material>(legacyBuildingSystem, "invalidPreviewMaterial");

            if (validPreviewMaterial != null) {
                SetPrivateField(buildPreviewController, "validPreviewMaterial", validPreviewMaterial);
            }

            if (invalidPreviewMaterial != null) {
                SetPrivateField(buildPreviewController, "invalidPreviewMaterial", invalidPreviewMaterial);
            }

            if (verboseLogging) {
                Debug.Log("LegacyBuildingMigrationBridge: preview materialok átmásolva.", this);
            }
        }

        private void RebuildCatalogFromLegacyPrefabs() {
            if (buildModeController == null) return;

            PlacedObject[] legacyPrefabs = GetPrivateField<PlacedObject[]>(legacyBuildingSystem, "placeablePrefabs");

            if (legacyPrefabs == null || legacyPrefabs.Length == 0) {
                Debug.Log("LegacyBuildingMigrationBridge: preview materialok átmásolva.", this);
                return;
            }

            List<BuildCatalogEntry> entries = new List<BuildCatalogEntry>();

            for (int i = 0; i < legacyPrefabs.Length; i++) {
                PlacedObject prefab = legacyPrefabs[i];

                if (prefab == null) continue;

                PlaceableDefinitionAuthoring authoring = prefab.GetComponent<PlaceableDefinitionAuthoring>();

                if (authoring == null || !authoring.HasDefinition) {
                    Debug.LogWarning(
                        $"LegacyBuildingMigrationBridge: a(z) '{prefab.name}' prefab nem tartalmaz PlaceableDefinitionAuthoring komponenst definícióval.",
                        prefab
                    );
                    continue;
                }

                PlaceableDefinition definition = authoring.Definition;
                KeyCode key = i >= 0 && i <= 8 ? KeyCode.Alpha1 + i : KeyCode.None;

                entries.Add(BuildCatalogEntry.Create(definition, key));
            }

            SetPrivateField(buildModeController, "buildCatalog", entries.ToArray());

            if (entries.Count > 0) {
                buildModeController.TrySelectByIndex(0);
            }

            if (verboseLogging) {
                Debug.Log($"LegacyBuildingMigrationBridge: {entries.Count} catalog entry létrehozva.", this);
            }
        }

        private static T GetPrivateField<T>(object target, string fieldName) {
            if (target == null) return default;

            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
            );

            if (field == null) {
                Debug.LogError($"LegacyBuildingMigrationBridge: nem található mező: {fieldName}");
                return default;
            }

            object value = field.GetValue(target);

            if (value == null) return default;

            return value is T typed ? typed : default;
        }

        private static void SetPrivateField(object target, string fieldName, object value) {
            if (target == null) return;

            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
            );

            if (field == null) {
                Debug.LogError($"LegacyBuildingMigrationBridge: nem található mező: {fieldName}");
                return;
            }

            field.SetValue(target, value);
        }
    }
}