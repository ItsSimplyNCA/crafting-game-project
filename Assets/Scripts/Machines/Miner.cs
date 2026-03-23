using UnityEngine;

public class Miner : MachineBase {
    [Header("Mining")]
    [SerializeField] private InventoryItemData oreItem;
    [SerializeField, Min(0.1f)] private float productionInterval = 2f;
    [SerializeField, Min(1)] private int amountPerCycle = 1;
    [SerializeField] private bool debugBlockedProduction = false;

    [Header("Output")]
    //[SerializeField] private ConveyorItem conveyorItemPrefab;
    [SerializeField] private Transform outputPoint;
    [SerializeField] private float fallbackOutputForwardOffset = 0.55f;
    [SerializeField] private float fallbackOutputHeight = 0.2f;

    private float productionTimer;

    public InventorySlotData OutputSlot => GetSlot(0);

    protected override void Awake() {
        slotCount = 1;
        base.Awake();
        productionTimer = productionInterval;
    }

    protected override void OnValidate() {
        slotCount = 1;
        base.OnValidate();

        productionInterval = Mathf.Max(0.1f, productionInterval);
        amountPerCycle = Mathf.Max(1, amountPerCycle);
        fallbackOutputForwardOffset = Mathf.Max(0.01f, fallbackOutputForwardOffset);
        fallbackOutputHeight = Mathf.Max(0f, fallbackOutputHeight);
    }

    private void Update() {
        Produce();
        TryOutputToFrontBelt();
    }

    private void Produce() {
        if (oreItem == null) return;

        productionTimer -= Time.deltaTime;

        while (productionTimer <= 0f) {
            if (!TryAddToSlot(0, oreItem, amountPerCycle)) {
                if (debugBlockedProduction) {
                    Debug.Log($"{name}: Miner belső slotja tele van.");
                }
                
                productionTimer = 0f;
                return;
            }

            productionTimer += productionInterval;
        }
    }

    private void TryOutputToFrontBelt() {
        InventorySlotData slot = GetSlot(0);

        if (slot == null || slot.IsEmpty || slot.item == null) return;

        ConveyorBelt frontBelt = GetFrontBelt();
        if (frontBelt == null) return;

        ConveyorItem itemPrefab = slot.item.conveyorItemPrefab;
        if (itemPrefab == null) return;

        Vector3 spawnPos = GetOutputSpawnPosition();
        ConveyorItem spawnedItem = Instantiate(itemPrefab, spawnPos, Quaternion.identity);
        spawnedItem.Setup(slot.item, 1);

        bool inserted = frontBelt.TryInsertItem(spawnedItem, 0f, null);
        Debug.Log(inserted);

        if (!inserted) {
            Destroy(spawnedItem.gameObject);
            return;
        }

        slot.amount -= 1;
        if (slot.amount <= 0) slot.Clear();
    }

    private ConveyorBelt GetFrontBelt() {
        if (WorldGrid.Instance == null) return null;

        Vector3Int myCell = WorldGrid.Instance.WorldToCell(transform.position);
        Vector3Int frontCell = myCell + GetForwardCellOffset();

        if (!WorldGrid.Instance.TryGetObjectAtCell(frontCell, out PlacedObject placed)) return null;

        return placed as ConveyorBelt;
    }

    private Vector3Int GetForwardCellOffset() {
        Vector3 f = transform.forward;

        if (Mathf.Abs(f.x) > Mathf.Abs(f.z)) {
            return (f.x >= 0f) ? Vector3Int.right : Vector3Int.left;
        }

        return (f.z >= 0f) ? new Vector3Int(0, 0, 1) : new Vector3Int(0, 0, -1);
    }

    private Vector3 GetOutputSpawnPosition() {
        if (outputPoint != null) return outputPoint.position;

        return transform.position
            + transform.forward * fallbackOutputForwardOffset
            + Vector3.up * fallbackOutputHeight;
    }

    public bool TryTakeOne(out InventoryItemData itemData) {
        return TryTakeFromSlot(0, 1, out itemData);
    }
}
