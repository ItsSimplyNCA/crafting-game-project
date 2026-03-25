using System.Collections.Generic;
using UnityEngine;

public class BuildingSystem : MonoBehaviour {
    public enum ToolMode {
        None,
        Build,
        Remove
    }

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private WorldGrid worldGrid;
    [SerializeField] private LayerMask placementMask;
    [SerializeField] private LayerMask removeMask = ~0;
    [SerializeField] private float maxBuildDistance = 8f;

    [Header("Pleaceables")]
    [SerializeField] private PlacedObject[] placeablePrefabs;

    [Header("Preview")]
    [SerializeField] private Material validPreviewMaterial;
    [SerializeField] private Material invalidPreviewMaterial;

    private int selectedIndex = 0;
    private int rotationSteps = 0;
    private ToolMode activeMode = ToolMode.None;

    private PlacedObject previewInstance;
    private readonly List<Renderer> previewRenderers = new();

    public ToolMode ActiveMode => activeMode;
    public bool IsBuildModeActive => activeMode == ToolMode.Build;
    public bool IsRemoveModeActive => activeMode == ToolMode.Remove;

    private void Start() {
        if (playerCamera == null) playerCamera = Camera.main;
        if (worldGrid == null) worldGrid = WorldGrid.Instance;
        if (placeablePrefabs != null && placeablePrefabs.Length > 0) {
            SelectPrefab(0);
        }

        HidePreview();
    }

    private void Update() {
        if (placeablePrefabs == null || placeablePrefabs.Length == 0 || worldGrid == null || playerCamera == null) {
            return;
        }

        switch (activeMode) {
            case ToolMode.Build:
                HandleSelection();
                HandleRotation();
                HandleBuildMode();
                break;
            case ToolMode.Remove:
                HandleRemoveMode();
                break;
            default:
                HidePreview();
                break;
        }
    }

    public void EnterBuildMode() {
        activeMode = ToolMode.Build;
        EnsurePreviewExists();
    }

    public void ExitBuildMode() {
        if (activeMode != ToolMode.Build) return;
        activeMode = ToolMode.None;
        HidePreview();
    }

    public void EnterRemoveMode() {
        activeMode = ToolMode.Remove;
        HidePreview();
    }

    public void ExitRemoveMode() {
        if (activeMode != ToolMode.Remove) return;
        activeMode = ToolMode.None;
    }

    public void CancelCurrentMode() {
        activeMode = ToolMode.None;
        HidePreview();
    }

    private void HandleSelection() {
        int maxKeys = Mathf.Min(placeablePrefabs.Length, 9);

        for (int i = 0; i < maxKeys; i++) {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i)) {
                SelectPrefab(i);
                break;
            }
        }
    }

    private void SelectPrefab(int index) {
        selectedIndex = Mathf.Clamp(index, 0, placeablePrefabs.Length - 1);
        rotationSteps = 0;
        CreatePreview();
    }

    private void HandleRotation() {
        if (Input.GetKeyDown(KeyCode.R)) {
            rotationSteps = (rotationSteps + 1) % 4;
            UpdatePreviewRotation();
        }
    }

    private void HandleBuildMode() {
        if (!TryGetBuildTargetCell(out _, out Vector3Int targetCell)) {
            HidePreview();
            return;
        }

        UpdatePreview(targetCell);

        if (Input.GetMouseButtonDown(0)) {
            TryPlace(targetCell);
        }
    }

    private void HandleRemoveMode() {
        HidePreview();

        if (!Input.GetMouseButtonDown(0)) return;
        if (!TryGetRemoveHit(out RaycastHit hit)) return;

        TryRemove(hit);
    }

    private bool TryGetBuildTargetCell(out RaycastHit hit, out Vector3Int cell) {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out hit, maxBuildDistance, placementMask)) {
            Vector3 point = hit.point + hit.normal * 0.02f;
            cell = worldGrid.WorldToCell(point);
            return true;
        }

        cell = default;
        return false;
    }

    private bool TryGetRemoveHit(out RaycastHit hit) {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        return Physics.Raycast(ray, out hit, maxBuildDistance, removeMask, QueryTriggerInteraction.Ignore);
    }
    
    private void TryPlace(Vector3Int cell) {
        PlacedObject prefab = placeablePrefabs[selectedIndex];

        if (!worldGrid.CanPlace(cell, prefab.size, prefab.height, rotationSteps)) {
            return;
        }

        Vector3 worldPosition = worldGrid.GetPlacementWorldPosition(cell, prefab.size, prefab.height, rotationSteps);
        Quaternion rotation = Quaternion.Euler(0f, rotationSteps * 90f, 0f);

        PlacedObject placed = Instantiate(prefab, worldPosition, rotation);
        placed.originCell = cell;
        placed.rotationSteps = rotationSteps;

        worldGrid.RegisterObject(placed);
    }

    private void TryRemove(RaycastHit hit) {
        ConveyorBelt belt = hit.collider.GetComponentInParent<ConveyorBelt>();
        PlacedObject placed = belt != null ? belt : hit.collider.GetComponentInParent<PlacedObject>();

        if (placed == null) return;

        if (previewInstance != null && placed.gameObject == previewInstance.gameObject) return;

        if (belt != null && !belt.TryCollectItemsToInventory()) return;

        worldGrid.UnregisterObject(placed);
        Destroy(placed.gameObject);
    }

    private void EnsurePreviewExists() {
        if (previewInstance == null && placeablePrefabs != null && placeablePrefabs.Length > 0) {
            CreatePreview();
        }
    }

    private void CreatePreview() {
        if (previewInstance != null) {
            Destroy(previewInstance.gameObject);
        }

        PlacedObject prefab = placeablePrefabs[selectedIndex];
        previewInstance = Instantiate(prefab);
        previewInstance.name = prefab.name + "_Preview";

        foreach (MonoBehaviour behaviour in previewInstance.GetComponentsInChildren<MonoBehaviour>()) {
            behaviour.enabled = false;
        }

        foreach (Collider col in previewInstance.GetComponentsInChildren<Collider>()) {
            col.enabled = false;
        }

        foreach (Rigidbody rb in previewInstance.GetComponentsInChildren<Rigidbody>()) {
            rb.isKinematic = true;
        }

        previewRenderers.Clear();
        previewRenderers.AddRange(previewInstance.GetComponentsInChildren<Renderer>());

        UpdatePreviewRotation();
        HidePreview();
    }
    
    private void UpdatePreview(Vector3Int cell) {
        if (previewInstance == null) return;

        PlacedObject prefab = placeablePrefabs[selectedIndex];
        bool canPlace = worldGrid.CanPlace(cell, prefab.size, prefab.height, rotationSteps);

        previewInstance.gameObject.SetActive(true);
        previewInstance.transform.position = worldGrid.GetPlacementWorldPosition(cell, prefab.size, prefab.height, rotationSteps);
        previewInstance.transform.rotation = Quaternion.Euler(0f, rotationSteps * 90f, 0f);

        ApplyPreviewMaterial(canPlace ? validPreviewMaterial : invalidPreviewMaterial);
    }

    private void UpdatePreviewRotation() {
        if (previewInstance == null) return;

        previewInstance.transform.rotation = Quaternion.Euler(0f, rotationSteps * 90f, 0f);
    }

    private void HidePreview() {
        if (previewInstance != null) {
            previewInstance.gameObject.SetActive(false);
        }
    }
    
    private void ApplyPreviewMaterial(Material material) {
        if (material == null) return;

        foreach (Renderer renderer in previewRenderers) {
            if (renderer == null) continue;

            Material[] mats = new Material[renderer.sharedMaterials.Length];

            for (int i = 0; i < mats.Length; i++) {
                mats[i] = material;
            }

            renderer.sharedMaterials = mats;
        }
    }
}
