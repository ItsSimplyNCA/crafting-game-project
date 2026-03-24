using System.Collections.Generic;
using UnityEngine;

public class ConveyorBelt : PlacedObject
{
    private enum EntryLane
    {
        Rear = 0,
        Left = 1,
        Right = 2
    }

    [Header("Movement")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float itemSpacing = 0.35f;
    [SerializeField] private float endStopOffset = 0.02f;
    [SerializeField] private float itemLift = 0.01f;

    [Header("Path Points")]
    [SerializeField] private Transform rearEntryPoint;
    [SerializeField] private Transform leftEntryPoint;
    [SerializeField] private Transform rightEntryPoint;
    [SerializeField] private Transform centerPoint;
    [SerializeField] private Transform exitPoint;
    [SerializeField] private float fallbackSurfaceHeight = 0.15f;

    [Header("Neighbor Detection")]
    [SerializeField] private float nextBeltProbeForward = 0.12f;
    [SerializeField] private float nextBeltProbeRadius = 0.3f;
    [SerializeField] private LayerMask beltDetectionMask = ~0;
    [SerializeField] private float oppositeFacingDotThreshold = -0.75f;
    [SerializeField] private float transferValidationEpsilon = 0.01f;

    private readonly List<ConveyorItem> items = new();

    private void Start()
    {
        if (WorldGrid.Instance == null) return;

        originCell = WorldGrid.Instance.WorldToCell(transform.position);

        if (!WorldGrid.Instance.TryGetObjectAtCell(originCell, out PlacedObject existing) || existing == this)
        {
            WorldGrid.Instance.RegisterObject(this);
        }
    }

    private void Update()
    {
        CleanupNulls();
        if (items.Count == 0) return;

        ConveyorBelt nextBelt = GetNextBelt();

        items.Sort((a, b) =>
        {
            float aRemaining = GetRemainingDistance(a);
            float bRemaining = GetRemainingDistance(b);
            return aRemaining.CompareTo(bRemaining);
        });

        for (int i = 0; i < items.Count; i++)
        {
            ConveyorItem item = items[i];
            if (item == null) continue;

            int lane = item.CurrentEntryIndex;
            float pathLength = GetPathLength(lane);

            item.DistanceOnBelt = Mathf.Clamp(item.DistanceOnBelt, 0f, pathLength);

            float currentDistance = item.DistanceOnBelt;
            float desiredDistance = currentDistance + speed * Time.deltaTime;

            if (desiredDistance >= pathLength && nextBelt != null)
            {
                if (nextBelt.TryInsertItem(item, 0f, this))
                {
                    i--;
                    continue;
                }
            }

            float maxAllowedDistance = GetMaxAllowedDistance(item, nextBelt);
            maxAllowedDistance = Mathf.Max(currentDistance, maxAllowedDistance);

            item.DistanceOnBelt = Mathf.Clamp(desiredDistance, 0f, maxAllowedDistance);
            SnapItem(item);
        }
    }

    public bool TryInsertItem(ConveyorItem item, float startDistance = 0f, ConveyorBelt sourceBelt = null, Vector3? sourceWorldPosition = null) {
        if (item == null) return false;

        CleanupNulls();

        int entryIndex = ResolveEntryIndex(sourceBelt, sourceWorldPosition);
        startDistance = Mathf.Max(0f, startDistance);

        if (!CanAcceptAtDistance(startDistance, entryIndex))
            return false;

        if (item.CurrentBelt != null && item.CurrentBelt != this) {
            item.CurrentBelt.RemoveItem(item, false);
        }

        item.AttachToBelt(this, startDistance, entryIndex);
        items.Add(item);
        SnapItem(item);
        return true;
    }

    public void RemoveItem(ConveyorItem item, bool restorePhysics)
    {
        if (item == null) return;

        items.Remove(item);

        if (item.CurrentBelt == this)
        {
            item.DetachFromBelt(restorePhysics);
        }
    }

    public Vector3 GetExitPosition()
    {
        return GetExitWorld();
    }

    private bool CanAcceptAtDistance(float distance, int entryIndex)
    {
        foreach (ConveyorItem existing in items)
        {
            if (existing == null) continue;

            if (existing.CurrentEntryIndex != entryIndex)
                continue;

            if (Mathf.Abs(existing.DistanceOnBelt - distance) < itemSpacing)
                return false;
        }

        return true;
    }

    private float GetMaxAllowedDistance(ConveyorItem movingItem, ConveyorBelt nextBelt)
    {
        int lane = movingItem.CurrentEntryIndex;
        float pathLength = GetPathLength(lane);
        float currentDistance = movingItem.DistanceOnBelt;
        float myMergeDistance = GetDistanceToCenter(lane);

        float maxAllowed = (nextBelt != null) ? pathLength : pathLength - endStopOffset;

        foreach (ConveyorItem other in items)
        {
            if (other == null || other == movingItem) continue;

            int otherLane = other.CurrentEntryIndex;
            float otherDistance = other.DistanceOnBelt;

            if (otherLane == lane)
            {
                if (otherDistance > currentDistance)
                {
                    maxAllowed = Mathf.Min(maxAllowed, otherDistance - itemSpacing);
                }

                continue;
            }

            float otherMergeDistance = GetDistanceToCenter(otherLane);

            if (otherDistance < otherMergeDistance)
                continue;

            float otherSharedDistance = otherDistance - otherMergeDistance;
            float mappedDistanceOnMyPath = myMergeDistance + otherSharedDistance;

            if (mappedDistanceOnMyPath > currentDistance)
            {
                maxAllowed = Mathf.Min(maxAllowed, mappedDistanceOnMyPath - itemSpacing);
            }
        }

        return maxAllowed;
    }

    private int ResolveEntryIndex(ConveyorBelt sourceBelt, Vector3? sourceWorldPosition = null) {
        Vector3 sourcePos;

        if (sourceBelt != null) {
            sourcePos = sourceBelt.GetExitPosition();
        } else if (sourceWorldPosition.HasValue) {
            sourcePos = sourceWorldPosition.Value;
        } else {
            return (int)EntryLane.Rear;
        }

        float bestDistance = float.MaxValue;
        int bestIndex = (int)EntryLane.Rear;

        for (int i = 0; i < 3; i++) {
            float d = Vector3.Distance(sourcePos, GetEntryWorld(i));
            if (d < bestDistance) {
                bestDistance = d;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    private void SnapItem(ConveyorItem item)
    {
        Vector3 pathPos = EvaluatePathPosition(item.CurrentEntryIndex, item.DistanceOnBelt);
        float halfHeight = GetItemHalfHeight(item);
        item.transform.position = pathPos + Vector3.up * (halfHeight + itemLift);
    }

    private Vector3 EvaluatePathPosition(int entryIndex, float distanceOnPath)
    {
        GetPathPoints(entryIndex, out Vector3 entry, out Vector3 center, out Vector3 exit);

        float entryToCenter = Vector3.Distance(entry, center);
        float centerToExit = Vector3.Distance(center, exit);
        float totalLength = Mathf.Max(0.0001f, entryToCenter + centerToExit);

        float d = Mathf.Clamp(distanceOnPath, 0f, totalLength);

        if (d <= entryToCenter)
        {
            float t = entryToCenter <= 0.0001f ? 1f : d / entryToCenter;
            return Vector3.Lerp(entry, center, t);
        }

        float d2 = d - entryToCenter;
        float t2 = centerToExit <= 0.0001f ? 1f : d2 / centerToExit;
        return Vector3.Lerp(center, exit, t2);
    }

    private void GetPathPoints(int entryIndex, out Vector3 entry, out Vector3 center, out Vector3 exit)
    {
        entry = GetEntryWorld(entryIndex);
        center = GetCenterWorld();
        exit = GetExitWorld();
    }

    private float GetPathLength(int entryIndex)
    {
        GetPathPoints(entryIndex, out Vector3 entry, out Vector3 center, out Vector3 exit);
        return Mathf.Max(0.0001f, Vector3.Distance(entry, center) + Vector3.Distance(center, exit));
    }

    private float GetDistanceToCenter(int entryIndex)
    {
        return Vector3.Distance(GetEntryWorld(entryIndex), GetCenterWorld());
    }

    private float GetRemainingDistance(ConveyorItem item)
    {
        if (item == null) return float.MaxValue;

        float pathLength = GetPathLength(item.CurrentEntryIndex);
        return Mathf.Max(0f, pathLength - item.DistanceOnBelt);
    }

    private float GetClosestEntryDistanceTo(Vector3 worldPos)
    {
        float best = float.MaxValue;

        for (int i = 0; i < 3; i++)
        {
            best = Mathf.Min(best, Vector3.Distance(GetEntryWorld(i), worldPos));
        }

        return best;
    }

    private float GetItemHalfHeight(ConveyorItem item)
    {
        Collider[] colliders = item.GetComponentsInChildren<Collider>();

        if (colliders.Length > 0)
        {
            Bounds bounds = colliders[0].bounds;

            for (int i = 1; i < colliders.Length; i++)
            {
                bounds.Encapsulate(colliders[i].bounds);
            }

            return bounds.extents.y;
        }

        Renderer[] renderers = item.GetComponentsInChildren<Renderer>();

        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;

            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds.extents.y;
        }

        return 0.1f;
    }

    private ConveyorBelt GetNextBelt()
    {
        if (WorldGrid.Instance != null)
        {
            Vector3Int myCell = WorldGrid.Instance.WorldToCell(transform.position);
            Vector3Int nextCell = myCell + GetForwardCellOffset();

            if (WorldGrid.Instance.TryGetObjectAtCell(nextCell, out PlacedObject placed))
            {
                ConveyorBelt next = placed as ConveyorBelt;
                if (next != null && next != this)
                    return next;
            }
        }

        ConveyorBelt probed = GetNextBeltByProbe();
        if (probed != null && CanOutputTo(probed)) return probed;

        return null;
    }

    private ConveyorBelt GetNextBeltByProbe()
    {
        Vector3 myExit = GetExitPosition();
        Vector3 probeCenter = myExit + transform.forward * nextBeltProbeForward;

        Collider[] hits = Physics.OverlapSphere(
            probeCenter,
            nextBeltProbeRadius,
            beltDetectionMask,
            QueryTriggerInteraction.Ignore);

        ConveyorBelt best = null;
        float bestScore = float.MaxValue;

        foreach (Collider hit in hits)
        {
            ConveyorBelt candidate = hit.GetComponentInParent<ConveyorBelt>();

            if (candidate == null || candidate == this)
                continue;

            if (!CanOutputTo(candidate)) continue;
            
            Vector3 toCandidateCenter = candidate.transform.position - transform.position;
            float forwardDot = Vector3.Dot(transform.forward, toCandidateCenter.normalized);

            if (forwardDot < 0.25f)
                continue;

            float score = candidate.GetClosestEntryDistanceTo(myExit);

            if (score < bestScore)
            {
                bestScore = score;
                best = candidate;
            }
        }

        return best;
    }

    private Vector3Int GetForwardCellOffset()
    {
        Vector3 f = transform.forward;

        if (Mathf.Abs(f.x) > Mathf.Abs(f.z))
            return (f.x >= 0f) ? Vector3Int.right : Vector3Int.left;

        return (f.z >= 0f)
            ? new Vector3Int(0, 0, 1)
            : new Vector3Int(0, 0, -1);
    }

    private Vector3 GetEntryWorld(int entryIndex)
    {
        float halfCell = (WorldGrid.Instance != null) ? WorldGrid.Instance.cellSize * 0.5f : 0.5f;
        Vector3 upOffset = Vector3.up * fallbackSurfaceHeight;

        switch ((EntryLane)entryIndex)
        {
            case EntryLane.Left:
                if (leftEntryPoint != null) return leftEntryPoint.position;
                return transform.position - transform.right * halfCell + upOffset;

            case EntryLane.Right:
                if (rightEntryPoint != null) return rightEntryPoint.position;
                return transform.position + transform.right * halfCell + upOffset;

            default:
                if (rearEntryPoint != null) return rearEntryPoint.position;
                return transform.position - transform.forward * halfCell + upOffset;
        }
    }

    private Vector3 GetCenterWorld()
    {
        if (centerPoint != null) return centerPoint.position;
        return transform.position + Vector3.up * fallbackSurfaceHeight;
    }

    private Vector3 GetExitWorld()
    {
        if (exitPoint != null) return exitPoint.position;

        float halfCell = (WorldGrid.Instance != null) ? WorldGrid.Instance.cellSize * 0.5f : 0.5f;
        return transform.position + transform.forward * halfCell + Vector3.up * fallbackSurfaceHeight;
    }

    private void CleanupNulls()
    {
        items.RemoveAll(i => i == null);
    }

    private void OnDestroy() {
        if (WorldGrid.Instance != null) {
            WorldGrid.Instance.UnregisterObject(this);
        }

        CleanupNulls();
    }

    private bool CanOutputTo(ConveyorBelt other) {
        if (other == null || other == this) return false;

        Vector3 myForward = transform.forward.normalized;
        Vector3 otherForward = other.transform.forward.normalized;

        float facingDot = Vector3.Dot(myForward, otherForward);
        if (facingDot <= oppositeFacingDotThreshold) return false;

        Vector3 myExit = GetExitPosition();

        float rearDist = Vector3.Distance(myExit, other.GetEntryWorld((int)EntryLane.Rear));
        float leftDist = Vector3.Distance(myExit, other.GetEntryWorld((int)EntryLane.Left));
        float rightDist = Vector3.Distance(myExit, other.GetEntryWorld((int)EntryLane.Right));
        float bestEntryDist = Mathf.Min(rearDist, Mathf.Min(leftDist, rightDist));

        float otherExitDist = Vector3.Distance(myExit, other.GetExitPosition());

        if (otherExitDist <= bestEntryDist + transferValidationEpsilon) return false;

        return true;
    }

    public bool TryCollectItemsToInventory() {
        InventorySystem inventory = InventorySystem.Instance;
        if (inventory == null) {
            Debug.LogError("ConveyorBelt: nincs InventorySystem a scene-ben.");
            return false;
        }

        ConveyorItem[] childItems = GetComponentsInChildren<ConveyorItem>(true);
        //Debug.Log($"ConveyorBelt: a talált itemek száma: {childItems.Length}");

        if (childItems.Length == 0) return true;

        for (int i = 0; i < childItems.Length; i++) {
            ConveyorItem item = childItems[i];
            if (item == null) continue;

            if (item.ItemData == null) {
                Debug.LogError($"ConveyorBelt: {item.name} itemhez nincs ItemData rendelve.", item);
                return false;
            }

            if (item.Amount <= 0) {
                Debug.LogError($"ConveyorBelt: {item.name} amount <= 0.", item);
                return false;
            }
        }

        if (!CanFitAllItemsInInventory(inventory, childItems)) {
            Debug.LogWarning("ConveyorBelt: nincs elég hely az inventoryban.");
            return false;
        }

        for (int i = 0; i < childItems.Length; i++) {
            ConveyorItem item = childItems[i];
            if (item == null) continue;

            bool added = inventory.AddItem(item.ItemData, item.Amount);
            if (!added) {
                Debug.LogError($"ConveyorBelt: nem sikerült inventoryba tenni ezt: {item.ItemData.itemName}", item);
                return false;
            }
        }

        for (int i = 0; i < childItems.Length; i++) {
            ConveyorItem item = childItems[i];
            if (item == null) continue;

            items.Remove(item);
            Destroy(item.gameObject);
        }

        return true;
    }

    private bool CanFitAllItemsInInventory(InventorySystem inventory, ConveyorItem[] pickupItems) {
        List<InventorySlotData> simulatedSlots = new List<InventorySlotData>(inventory.Slots.Count);

        foreach (InventorySlotData slot in inventory.Slots) {
            InventorySlotData copy = new InventorySlotData();

            if (slot != null && !slot.IsEmpty) {
                copy.Set(slot.item, slot.amount);
            }

            simulatedSlots.Add(copy);
        }

        for (int i = 0; i < pickupItems.Length; i++) {
            ConveyorItem item = pickupItems[i];
            if (item == null || item.ItemData == null || item.Amount <= 0) continue;
            if (!TrySimulateAdd(simulatedSlots, item.ItemData, item.Amount)) return false;
        }

        return true;
    }

    private bool TrySimulateAdd(List<InventorySlotData> simulatedSlots, InventoryItemData item, int amount) {
        int remaining = amount;

        for (int i = 0; i < simulatedSlots.Count; i++) {
            InventorySlotData slot = simulatedSlots[i];

            if (slot.IsEmpty || slot.item != item || slot.amount >= item.maxStack) continue;

            int addAmount = Mathf.Min(remaining, item.maxStack - slot.amount);
            slot.amount += addAmount;
            remaining -= addAmount;

            if (remaining <= 0) return true;
        }

        for (int i = 0; i < simulatedSlots.Count; i++) {
            InventorySlotData slot = simulatedSlots[i];

            if (!slot.IsEmpty) continue;

            int addAmount = Mathf.Min(remaining, item.maxStack);
            slot.Set(item, addAmount);
            remaining -= addAmount;

            if (remaining <= 0) return true;
        }

        return false;
    }

    public bool TryPickupItemsAndDestroy() {
        InventorySystem inventory = InventorySystem.Instance;
        if (inventory == null) {
            Debug.LogError("ConveyorBelt: nincs InventorySystem a scene-ben.");
            return false;
        }

        ConveyorItem[] childItems = GetComponentsInChildren<ConveyorItem>(true);

        for (int i = 0; i < childItems.Length; i++) {
            ConveyorItem item = childItems[i];
            if (item == null) continue;

            if (item.ItemData == null) {
                Debug.LogError($"ConveyorBelt: {item.name} itemhez nincs ItemData rendelve.", item);
                return false;
            }

            if (item.Amount <= 0) {
                Debug.LogError($"ConveyorBelt: {item.name} amount <= 0.", item);
                return false;
            }
        }

        if (!CanFitAllItemsInInventory(inventory, childItems)) {
            Debug.LogWarning("ConveyorBelt: nincs elég hely az invetoryban.");
        }

        for (int i = 0; i < childItems.Length; i++) {
            ConveyorItem item = childItems[i];
            if (item == null) continue;

            bool added = inventory.AddItem(item.ItemData, item.Amount);
            if (!added) {
                Debug.LogError($"ConveyorBelt: nem sikerült inventoryba tenni ezt: {item.ItemData.itemName}", item);
                return false;
            }
        }

        for (int i = 0; i < childItems.Length; i++) {
            ConveyorItem item = childItems[i];

            if (item == null) continue;

            items.Remove(item);
            Destroy(item.gameObject);
        }

        if (WorldGrid.Instance != null) {
            WorldGrid.Instance.UnregisterObject(this);
        }

        Destroy(gameObject);
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 rear = GetEntryWorld((int)EntryLane.Rear);
        Vector3 left = GetEntryWorld((int)EntryLane.Left);
        Vector3 right = GetEntryWorld((int)EntryLane.Right);
        Vector3 center = GetCenterWorld();
        Vector3 exit = GetExitWorld();

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(rear, 0.04f);
        Gizmos.DrawSphere(left, 0.04f);
        Gizmos.DrawSphere(right, 0.04f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(center, 0.05f);
        Gizmos.DrawSphere(exit, 0.05f);

        Gizmos.DrawLine(rear, center);
        Gizmos.DrawLine(left, center);
        Gizmos.DrawLine(right, center);
        Gizmos.DrawLine(center, exit);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(exit + transform.forward * nextBeltProbeForward, nextBeltProbeRadius);
    }
}