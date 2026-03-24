using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ConveyorItem : MonoBehaviour {
    [Header("Inventory")]
    [SerializeField] private InventoryItemData itemData;
    [SerializeField, Min(1)] private int amount = 1;

    [Header("Visual")]
    [SerializeField] private Transform modelAnchor;

    public ConveyorBelt CurrentBelt { get; private set; }
    public float DistanceOnBelt { get; set; }
    public int CurrentEntryIndex { get; private set; }

    public InventoryItemData ItemData => itemData;
    public int Amount => amount;

    private Rigidbody rb;
    private GameObject modelInstance;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        RefreshVisual();
    }

    private void OnValidate() {
        amount = Mathf.Max(1, amount);
    }

    public void Setup(InventoryItemData data, int stackAmount = 1) {
        itemData = data;
        amount = Mathf.Max(1, stackAmount);
        RefreshVisual();
    }

    public void AttachToBelt(ConveyorBelt belt, float startDistance, int entryIndex = 0) {
        CurrentBelt = belt;
        DistanceOnBelt = Mathf.Max(0f, startDistance);
        CurrentEntryIndex = Mathf.Clamp(entryIndex, 0, 2);

        if (belt != null) {
            transform.SetParent(belt.transform, true);
        }

        if (rb != null) {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void DetachFromBelt(bool restorePhysics) {
        ConveyorBelt previousBelt = CurrentBelt;

        if (previousBelt != null && transform.parent == previousBelt.transform) {
            transform.SetParent(null, true);
        }

        CurrentBelt = null;
        DistanceOnBelt = 0f;
        CurrentEntryIndex = 0;

        if (rb != null && restorePhysics)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }

    public bool RemoveFromStack(int removeAmount) {
        if (removeAmount <= 0) return false;
        if (amount < removeAmount) return false;

        amount -= removeAmount;
        return true;
    }

    public bool CanStackWith(InventoryItemData otherItem) {
        return itemData != null && itemData == otherItem;
    }

    [ContextMenu("Refresh Visual")]
    public void RefreshVisual() {
        ClearVisual();

        if (itemData == null || itemData.worldModelPrefab == null) return;

        if (itemData.worldModelPrefab.GetComponent<ConveyorItem>() != null) {
            Debug.LogError(
                $"A worldModelPrefab nem lehet ugyanaz, mint a ConveyorItem prefab. " +
                $"Csak vizuális modellt adj meg itt. Item: {itemData.itemName}",
                itemData.worldModelPrefab
            );
            return;
        }

        Transform parent = transform;

        if (modelAnchor != null && modelAnchor.IsChildOf(transform)) {
            parent = modelAnchor;
        }

        modelInstance = Instantiate(itemData.worldModelPrefab, parent);
        modelInstance.transform.localPosition = Vector3.zero;
        modelInstance.transform.localRotation = Quaternion.identity;
        modelInstance.transform.localScale = Vector3.one;
    }

    private void ClearVisual() {
        if (modelInstance == null) return;

        if (Application.isPlaying) {
            Destroy(modelInstance);
        } else {
            DestroyImmediate(modelInstance);
        }

        modelInstance = null;
    }

    private void OnDestroy() {
        ClearVisual();
    }
}