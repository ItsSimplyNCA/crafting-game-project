using UnityEngine;

public class ConveyorTestSpawner : MonoBehaviour {
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private ConveyorItem itemPrefab;
    [SerializeField] private InventoryItemData testItemData;
    [SerializeField, Min(1)] private int testAmount = 1;

    [Header("Raycast")]
    [SerializeField] private float rayDistance = 8f;
    [SerializeField] private LayerMask hitMask = ~0;

    [Header("Input")]
    [SerializeField] private KeyCode spawnKey = KeyCode.T;

    private void Awake() {
        if (playerCamera == null) playerCamera = Camera.main;
    }

    private void Update() {
        if (Input.GetKeyDown(spawnKey)) {
            SpawnOnLookedAtBelt();
        }
    }

    private void SpawnOnLookedAtBelt() {
        if (itemPrefab == null) {
            Debug.LogError("ConveyorTestSpawner: nincs itemPrefab beállítva.");
            return;
        }

        if (testItemData == null) {
            Debug.LogError("ConveyorTestSpawner: nincs testItemData beállítva.");
        }
        
        if (playerCamera == null) {
            Debug.LogWarning("ConveyorTestSpawner: nincs beállítva kamera.");
            return;
        }
        
        if (itemPrefab == null) {
            Debug.LogWarning("ConveyorTestSpawner: nincs beállítva itemPrefab.");
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (!Physics.Raycast(ray, out RaycastHit hit, rayDistance, hitMask, QueryTriggerInteraction.Ignore)) {
            Debug.Log("Nem talált semmit a raycast.");
            return;
        }

        ConveyorBelt belt = hit.collider.GetComponentInParent<ConveyorBelt>();

        if (belt == null) {
            Debug.Log("Nem conveyor beltre nézel.");
            return;
        }

        Vector3 spawnPos = belt.transform.position;
        ConveyorItem item = Instantiate(itemPrefab, spawnPos, Quaternion.identity);
        item.Setup(testItemData, testAmount);

        bool inserted = belt.TryInsertItem(item, 0f, null);

        if (!inserted) {
            Debug.Log("A belt most nem tudta átvenni az itemet.");
            Destroy(item.gameObject);
        }
    }

    private void OnDrawGizmosSelected() {
        if (playerCamera == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * rayDistance);
    }
}
