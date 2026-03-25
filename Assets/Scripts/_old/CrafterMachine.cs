using System;
using UnityEngine;

public class CrafterMachine : MachineBase {
    public const int InputSlotIndex = 0;
    public const int OutputSlotIndex = 1;

    public event Action<CrafterMachine> StateChanged;

    [Header("Recipes")]
    [SerializeField] private CrafterRecipeSO[] availableRecipes;
    [SerializeField] private int selectedRecipeIndex = 0;

    [Header("Transfer Points")]
    [SerializeField] private Transform inputPoint;
    [SerializeField] private Transform outputPoint;

    [Header("Timing")]
    [SerializeField, Min(0.01f)] private float inputPullInterval = 0.15f;
    [SerializeField, Min(0.01f)] private float outputPushInterval = 0.15f;

    [Header("Belt Detection")]
    [SerializeField, Min(0.05f)] private float pointForwardProbeDistance = 0.55f;
    [SerializeField, Min(0.05f)] private float physicsProbeRadius = 0.25f;
    [SerializeField] private LayerMask beltDetectionMask = ~0;

    [Header("Runtime Debug")]
    [SerializeField, Min(0f)] private float craftProgress = 0f;

    private float inputPullTimer;
    private float outputPushTimer;

    public CrafterRecipeSO SelectedRecipe =>
        IsValidRecipeIndex(selectedRecipeIndex) ? availableRecipes[selectedRecipeIndex] : null;

    public int SelectedRecipeIndex => selectedRecipeIndex;
    public int RecipeCount => availableRecipes == null ? 0 : availableRecipes.Length;
    public float CraftProgress => craftProgress;

    public float CraftProgressNormalized {
        get {
            CrafterRecipeSO recipe = SelectedRecipe;
            if (recipe == null || recipe.CraftDuration <= 0f) return 0f;
            return Mathf.Clamp01(craftProgress / recipe.CraftDuration);
        }
    }

    public InventorySlotData InputSlot => GetSlot(InputSlotIndex);
    public InventorySlotData OutputSlot => GetSlot(OutputSlotIndex);

    protected override void Awake() {
        slotCount = 2;
        base.Awake();
    }

    protected override void OnValidate() {
        slotCount = 2;
        base.OnValidate();

        if (availableRecipes == null || availableRecipes.Length == 0) {
            selectedRecipeIndex = -1;
            return;
        }

        selectedRecipeIndex = Mathf.Clamp(selectedRecipeIndex, 0, availableRecipes.Length - 1);
    }

    private void Start() {
        RegisterOnGrid();

        if (availableRecipes == null || availableRecipes.Length == 0) {
            selectedRecipeIndex = -1;
        } else if (!IsValidRecipeIndex(selectedRecipeIndex)) {
            selectedRecipeIndex = 0;
        }

        NotifyStateChanged();
    }

    private void Update() {
        inputPullTimer -= Time.deltaTime;
        outputPushTimer -= Time.deltaTime;

        if (inputPullTimer <= 0f) {
            inputPullTimer = inputPullInterval;
            TryPullFromInputBelt();
        }

        AdvanceCraft(Time.deltaTime);

        if (outputPushTimer <= 0f) {
            outputPushTimer = outputPushInterval;
            TryPushToOutputBelt();
        }
    }

    private void OnDestroy() {
        if (WorldGrid.Instance != null) {
            WorldGrid.Instance.UnregisterObject(this);
        }
    }

    public CrafterRecipeSO GetRecipeAt(int index) {
        return IsValidRecipeIndex(index) ? availableRecipes[index] : null;
    }

    public void SelectRecipe(int index) {
        if (!IsValidRecipeIndex(index)) return;
        if (selectedRecipeIndex == index) return;

        selectedRecipeIndex = index;
        craftProgress = 0f;
        NotifyStateChanged();
    }

    private void RegisterOnGrid() {
        if (WorldGrid.Instance == null) return;

        originCell = WorldGrid.Instance.WorldToCell(transform.position);
        
        if (!WorldGrid.Instance.TryGetObjectAtCell(originCell, out PlacedObject existing) || existing == this) {
            WorldGrid.Instance.RegisterObject(this);
        }
    }

    private void TryPullFromInputBelt() {
        CrafterRecipeSO recipe = SelectedRecipe;
        if (recipe == null || recipe.InputItem == null) return;

        if (!CanAddToSlot(InputSlotIndex, recipe.InputItem, 1)) return;

        ConveyorBelt inputBelt = GetConnectedBelt(inputPoint);
        if (inputBelt == null) return;

        if (!inputBelt.TryExtractReadyItem(recipe.InputItem, out InventoryItemData extractedItem)) return;
        if (extractedItem == null) return;

        if (TryAddToSlot(InputSlotIndex, extractedItem, 1)) {
            NotifyStateChanged();
        }
    }

    private void TryPushToOutputBelt() {
        InventorySlotData outputSlot = OutputSlot;
        if (outputSlot == null || outputSlot.IsEmpty || outputSlot.item == null) return;
        if (outputPoint == null) return;

        ConveyorBelt outputBelt = GetConnectedBelt(outputPoint);
        if (outputBelt == null) return;

        ConveyorItem itemPrefab = outputSlot.item.conveyorItemPrefab;
        if (itemPrefab == null) {
            Debug.LogWarning($"CrafterMachine '{name}': a(z) '{outputSlot.item.itemName}' itemhez nincs conveyorItemPrefab beállítva.", this);
            return;
        }

        ConveyorItem spawnedItem = Instantiate(itemPrefab, outputPoint.position, outputPoint.rotation);
        spawnedItem.Setup(outputSlot.item, 1);

        bool inserted = outputBelt.TryInsertItem(spawnedItem, 0f, null, outputPoint.position);

        if (!inserted) {
            Destroy(spawnedItem.gameObject);
            return;
        }

        outputSlot.amount -= 1;
        if (outputSlot.amount <= 0) {
            outputSlot.Clear();
        }

        NotifyStateChanged();
    }

    private void AdvanceCraft(float deltaTime) {
        CrafterRecipeSO recipe = SelectedRecipe;
        if (recipe == null) return;

        bool canCraft = CanCraft(recipe);

        if (!canCraft) {
            if (craftProgress > 0f) {
                craftProgress = 0f;
                NotifyStateChanged();
            }
            return;
        }

        craftProgress += deltaTime;

        if (craftProgress < recipe.CraftDuration) {
            NotifyStateChanged();
            return;
        }

        craftProgress = 0f;
        CompleteCraft(recipe);
    }

    private bool CanCraft(CrafterRecipeSO recipe) {
        if (recipe == null) return false;
        if (recipe.InputItem == null || recipe.OutputItem == null) return false;

        InventorySlotData inputSlot = InputSlot;
        InventorySlotData outputSlot = OutputSlot;

        if (inputSlot == null || outputSlot == null) return false;
        if (inputSlot.IsEmpty) return false;
        if (inputSlot.item != recipe.InputItem) return false;
        if (inputSlot.amount < recipe.InputAmount) return false;

        if (outputSlot.IsEmpty) {
            return recipe.OutputAmount <= recipe.OutputItem.maxStack;
        }

        if (outputSlot.item != recipe.OutputItem) return false;

        return outputSlot.amount + recipe.OutputAmount <= outputSlot.item.maxStack;
    }

    private void CompleteCraft(CrafterRecipeSO recipe) {
        InventorySlotData inputSlot = InputSlot;
        InventorySlotData outputSlot = OutputSlot;

        if (inputSlot == null || OutputSlot == null) return;

        inputSlot.amount -= recipe.InputAmount;
        if (inputSlot.amount <= 0) {
            inputSlot.Clear();
        }

        if (outputSlot.IsEmpty) {
            outputSlot.Set(recipe.OutputItem, recipe.OutputAmount);
        } else {
            outputSlot.amount += recipe.OutputAmount;
        }

        NotifyStateChanged();
    }

    private ConveyorBelt GetConnectedBelt(Transform point) {
        if (point == null) return null;

        ConveyorBelt beltByGrid = GetConnectedBeltByGrid(point);
        if (beltByGrid != null) return beltByGrid;

        return GetConnectedBeltByPhysics(point);
    }

    private ConveyorBelt GetConnectedBeltByGrid(Transform point) {
        if (WorldGrid.Instance == null) return null;

        float probeDistance = Mathf.Max(pointForwardProbeDistance, WorldGrid.Instance.cellSize * 0.55f);
        Vector3 probeWorldPos = point.position + point.forward * probeDistance;
        Vector3Int targetCell = WorldGrid.Instance.WorldToCell(probeWorldPos);

        if (!WorldGrid.Instance.TryGetObjectAtCell(targetCell, out PlacedObject placedObject)) {
            return null;
        }

        return placedObject as ConveyorBelt;
    }

    private ConveyorBelt GetConnectedBeltByPhysics(Transform point) {
        Vector3 probeCenter = point.position + point.forward * pointForwardProbeDistance;

        Collider[] hits = Physics.OverlapSphere(
            probeCenter,
            physicsProbeRadius,
            beltDetectionMask,
            QueryTriggerInteraction.Ignore
        );

        ConveyorBelt best = null;
        float bestDistance = float.MaxValue;

        foreach (Collider hit in hits) {
            ConveyorBelt candidate = hit.GetComponentInParent<ConveyorBelt>();
            if (candidate == null) continue;

            float distance = Vector3.Distance(candidate.transform.position, probeCenter);
            if (distance < bestDistance) {
                bestDistance = distance;
                best = candidate;
            }
        }

        return best;
    }

    private bool IsValidRecipeIndex(int index) {
        return availableRecipes != null &&
            index >= 0 &&
            index < availableRecipes.Length &&
            availableRecipes[index] != null;
    }

    private void NotifyStateChanged() {
        StateChanged?.Invoke(this);
    }
}
