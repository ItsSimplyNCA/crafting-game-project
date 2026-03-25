using System.Collections.Generic;
using UnityEngine;

namespace Game.Bootstrap {
    [DisallowMultipleComponent]
    public sealed class SceneReferences : MonoBehaviour {
        [Header("Scene Services")]
        [SerializeField] private WorldGrid worldGrid;
        [SerializeField] private InventorySystem inventorySystem;
        [SerializeField] private BuildingSystem buildingSystem;

        [Header("Player")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private SimpleFPSController playerController;
        [SerializeField] private CrafterPlayerInteractor crafterPlayerInteractor;

        [Header("UI")]
        [SerializeField] private CrafterUI crafterUI;

        [Header("Editor")]
        [SerializeField] private bool autoResolveInEditor = true;

        public WorldGrid WorldGrid => worldGrid;
        public BuildingSystem BuildingSystem => buildingSystem;
        public InventorySystem InventorySystem => inventorySystem;
        
        public Camera MainCamera => mainCamera;
        public SimpleFPSController PlayerController => playerController;
        public CrafterPlayerInteractor CrafterPlayerInteractor => crafterPlayerInteractor;

        public CrafterUI CrafterUI => crafterUI;

        public bool HasRequiredCoreReferences => 
            worldGrid != null &&
            mainCamera != null &&
            playerController != null;

        private void OnValidate() {
            if (!Application.isPlaying && autoResolveInEditor) {
                ResolveMissingReferences();
            }
        }

        [ContextMenu("Resolve Missing References")]
        public void ResolveMissingReferences() {
            if (worldGrid == null) worldGrid = FindObjectOfType<WorldGrid>();
            if (buildingSystem == null) buildingSystem = FindObjectOfType<BuildingSystem>();
            if (inventorySystem == null) inventorySystem = FindObjectOfType<InventorySystem>();

            if (mainCamera == null) {
                mainCamera = Camera.main;
                if (mainCamera == null) mainCamera = FindObjectOfType<Camera>();
            }

            if (playerController == null) playerController = FindObjectOfType<SimpleFPSController>();
            if (crafterPlayerInteractor == null) crafterPlayerInteractor = FindObjectOfType<CrafterPlayerInteractor>();
            if (crafterUI == null) crafterUI = FindObjectOfType<CrafterUI>();
        }

        public bool ValidateRequiredCore(out string errorMessage) {
            List<string> missing = new();

            if (worldGrid == null) missing.Add("WorldGrid");
            if (mainCamera == null) missing.Add("MainCamera");
            if (playerController == null) missing.Add("SimpleFPSController");

            if (missing.Count == 0) {
                errorMessage = string.Empty;
                return true;
            }

            errorMessage = "SceneReferences: hiányzó kötelező core referenciák:\n " + string.Join("\n- ", missing);

            return false;
        }

        public IEnumerable<Object> EnumerateAllBoundObjects() {
            if (worldGrid != null) yield return worldGrid;
            if (buildingSystem != null) yield return buildingSystem;
            if (inventorySystem != null ) yield return inventorySystem;
            if (mainCamera != null) yield return mainCamera;
            if (playerController != null) yield return playerController;
            if (crafterPlayerInteractor != null) yield return crafterPlayerInteractor;
            if (crafterUI != null) yield return crafterUI;
        }

        [ContextMenu("Log References")]
        public void LogReferences() {
            Debug.Log(
                "=== SceneReferences ===\n" +
                $"WorldGrid: {GetName(worldGrid)}\n" +
                $"BuildingSystem: {GetName(buildingSystem)}\n" +
                $"InventorySystem: {GetName(inventorySystem)}\n" +
                $"MainCamera: {GetName(mainCamera)}\n" +
                $"PlayerController: {GetName(playerController)}\n" +
                $"CrafterPlayerInteractor: {GetName(crafterPlayerInteractor)}\n" +
                $"CrafterUI: {GetName(crafterUI)}",
                this
            );
        }

        private static string GetName(Object obj) {
            return obj == null ? "NULL" : obj.name;
        }
    }
}
