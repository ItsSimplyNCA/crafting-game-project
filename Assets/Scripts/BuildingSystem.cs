using System.Collections.Generic;
using UnityEngine;

public class BuildingSystem : MonoBehaviour {
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private WorldGrid worldGrid;
    [SerializeField] private LayerMask placementMask;
    [SerializeField] private float maxBuildDistance = 8f;

    [Header("Pleaceables")]
    [SerializeField] private PlacedObject[] placeablePrefabs;

    [Header("Preview")]
    [SerializeField] private Material validPreviewMaterial;
    [SerializeField] private Material invalidPreviewMaterial;

    private int selectedIndex = 0;
    private int rotationSteps = 0;

    private PlacedObject previewInstance;
    private readonly List<Renderer> previewRenderers = new();

    private void Start() {
        if (playerCamera == null) playerCamera = Camera.main;
        if (worldGrid == null) worldGrid = WorldGrid.Instance;
        if (placeablePrefabs != null && placeablePrefabs.Length > 0) {
            SelectPrefab(0);
        }
    }

    private void Update() {
        if (placeablePrefabs == null || placeablePrefabs.Length == 0 || worldGrid == null || playerCamera == null) {
            return;
        }

        HandleSelection();
        HandleRotation();
        HandleBuildAndRemove();
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

    private void HandleBuildAndRemove() {
        if (!TryGetTargetCell(out RaycastHit hit, out Vector3Int targetCell)) {
            if (previewInstance != null) {
                previewInstance.gameObject.SetActive(false);
            }

            return;
        }

        UpdatePreview(targetCell);

        if (Input.GetMouseButtonDown(0)) {
            TryPlace(targetCell);
        }

        if (Input.GetMouseButtonDown(1)) {
            TryRemove(hit);
        }
    }

    private bool TryGetTargetCell(out RaycastHit hit, out Vector3Int cell) {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out hit, maxBuildDistance, placementMask)) {
            Vector3 point = hit.point + hit.normal * 0.02f;
            cell = worldGrid.WorldToCell(point);
            return true;
        }

        cell = default;
        return false;
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
        PlacedObject placed = hit.collider.GetComponentInParent<PlacedObject>();

        if (placed == null) return;

        if (previewInstance != null && placed.gameObject == previewInstance.gameObject) {
            return;
        }

        worldGrid.UnregisterObject(placed);
        Destroy(placed.gameObject);
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
