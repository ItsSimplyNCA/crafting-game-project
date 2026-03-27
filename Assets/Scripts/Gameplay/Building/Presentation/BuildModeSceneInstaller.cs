using Game.Gameplay.Building.Runtime;
using Game.Gameplay.Grid.Presentation;
using Game.Gameplay.Grid.Runtime;
using UnityEngine;

namespace Game.Gameplay.Building.Presentation {
    [DefaultExecutionOrder(-800)]
    [DisallowMultipleComponent]
    public sealed class BuildModeSceneInstaller : MonoBehaviour {
        [Header("Host")]
        [SerializeField] private GameObject componentHost;

        [Header("Auto Create")]
        [SerializeField] private bool createMissingComponents = true;

        [Header("Core Components")]
        [SerializeField] private GridService gridService;
        [SerializeField] private PlacementService placementService;
        [SerializeField] private DismantleService dismantleService;
        [SerializeField] private BuildRaycastController buildRaycastController;
        [SerializeField] private BuildPreviewController buildPreviewController;
        [SerializeField] private BuildModeController buildModeController;

        [Header("Migration Bridges")]
        [SerializeField] private LegacyGridMigrationBridge legacyGridMigrationBridge;
        [SerializeField] private LegacyBuildingMigrationBridge legacyBuildingMigrationBridge;

        [Header("Run")]
        [SerializeField] private bool installOnStart = true;
        [SerializeField] private bool verboseLogging = true;

        public GridService GridService => gridService;
        public PlacementService PlacementService => placementService;
        public BuildModeController BuildModeControll => buildModeController;

        private void Start() {
            if (installOnStart) Install();
        }

        [ContextMenu("Install Build Mode Stack")]
        public void Install() {
            ResolveOrCreateCoreComponents();
            ResolveOrCreateMigrationBridges();

            if (legacyGridMigrationBridge != null) {
                legacyGridMigrationBridge.ResolveMissingReferences();
                legacyGridMigrationBridge.MigrateSettings();
            }

            if (legacyBuildingMigrationBridge != null) {
                legacyBuildingMigrationBridge.ResolveMissingReferences();
                legacyBuildingMigrationBridge.Migrate();
            }

            if (buildRaycastController != null) {
                buildRaycastController.ResolveMissingReferences();
                buildRaycastController.RefreshTarget();
            }

            if (buildPreviewController != null) {
                buildPreviewController.ResolveMissingReferences();
                buildPreviewController.RefreshPreview();
            }

            if (buildModeController != null) {
                buildModeController.ResolveMissingReferences();
            }

            if (verboseLogging) {
                Debug.Log("BuildModeSceneInstaller: az új build stack telepítve.", this);
            }
        }

        private void ResolveOrCreateCoreComponents() {
            GameObject host = GetHostObject();

            gridService = ResolveOrCreate(gridService, host);
            placementService = ResolveOrCreate(placementService, host);
            dismantleService = ResolveOrCreate(dismantleService, host);
            buildRaycastController = ResolveOrCreate(buildRaycastController, host);
            buildPreviewController = ResolveOrCreate(buildPreviewController, host);
            buildModeController = ResolveOrCreate(buildModeController, host);
        }

        private void ResolveOrCreateMigrationBridges() {
            GameObject host = GetHostObject();

            legacyGridMigrationBridge = ResolveOrCreate(legacyGridMigrationBridge, host);
            legacyBuildingMigrationBridge = ResolveOrCreate(legacyBuildingMigrationBridge, host);
        }

        private GameObject GetHostObject() {
            if (componentHost != null) {
                return componentHost;
            }
            
            return gameObject;
        }

        private T ResolveOrCreate<T>(T existing, GameObject host) where T : Component {
            if (existing != null) return existing;

            T found = FindObjectOfType<T>();

            if (found != null) return found;

            if (!createMissingComponents || host == null) {
                return null;
            }

            return host.GetComponent<T>() ?? host.AddComponent<T>();
        }
    }
}