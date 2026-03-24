using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CrafterUI : MonoBehaviour {
    public static CrafterUI Instance { get; private set; }

    [Header("Root")]
    [SerializeField] private GameObject root;
    [SerializeField] private bool manageCursor = true;

    [Header("Header")]
    [SerializeField] private TMP_Text machineTitleText;
    [SerializeField] private TMP_Text selectedRecipeText;
    [SerializeField] private Image selectedRecipeIcon;
    [SerializeField] private Button closeButton;

    [Header("Status")]
    [SerializeField] private TMP_Text inputText;
    [SerializeField] private TMP_Text outputText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private Image progressFill;

    [Header("Recipe List")]
    [SerializeField] private Transform recipeButtonParent;
    [SerializeField] private CrafterRecipeButtonUI recipeButtonPrefab;

    private readonly List<CrafterRecipeButtonUI> spawnedButtons = new();
    private CrafterMachine currentMachine;

    public bool IsOpen => root != null && root.activeSelf;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (closeButton != null) {
            closeButton.onClick.AddListener(Hide);
        }

        if (root != null) {
            root.SetActive(false);
        }
    }

    private void Update() {
        if (IsOpen && Input.GetKeyDown(KeyCode.Escape)) {
            Hide();
        }
    }

    private void OnDestroy() {
        if (closeButton != null) {
            closeButton.onClick.RemoveListener(Hide);
        }

        UnbindMachine();
    }

    public void Show(CrafterMachine machine) {
        if (machine == null) return;

        if (currentMachine != machine) {
            UnbindMachine();
            currentMachine = machine;
            currentMachine.StateChanged += HandleMachineStateChanged;
            RebuildRecipeButtons();
        }

        if (root != null) {
            root.SetActive(true);
        }

        Refresh();

        if (manageCursor) {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void Hide() {
        if (root != null) root.SetActive(false);

        if (manageCursor) {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void UnbindMachine() {
        if (currentMachine != null) {
            currentMachine.StateChanged -= HandleMachineStateChanged;
            currentMachine = null;
        }
    }

    private void HandleMachineStateChanged(CrafterMachine machine) {
        if (machine == currentMachine) Refresh();
    }

    private void RebuildRecipeButtons() {
        for (int i = 0; i < spawnedButtons.Count; i++) {
            if (spawnedButtons[i] != null) {
                Destroy(spawnedButtons[i].gameObject);
            }
        }

        spawnedButtons.Clear();

        if (currentMachine == null || recipeButtonParent == null | recipeButtonPrefab == null) return;

        for (int i = 0; i < currentMachine.RecipeCount; i++) {
            CrafterRecipeSO recipe = currentMachine.GetRecipeAt(i);
            if (recipe == null) continue;

            CrafterRecipeButtonUI button = Instantiate(recipeButtonPrefab, recipeButtonParent);
            button.Setup(recipe, i, HandleRecipeSelected, i == currentMachine.SelectedRecipeIndex);
            spawnedButtons.Add(button);
        }
    }

    private void HandleRecipeSelected(int recipeIndex) {
        if (currentMachine == null) return;

        currentMachine.SelectRecipe(recipeIndex);

        for (int i = 0; i < spawnedButtons.Count; i++) {
            if (spawnedButtons[i] != null) {
                spawnedButtons[i].SetSelected(i == currentMachine.SelectedRecipeIndex);
            }
        }

        Refresh();
    }

    private void Refresh() {
        if (currentMachine == null) return;

        CrafterRecipeSO recipe = currentMachine.SelectedRecipe;
        InventorySlotData inputSlot = currentMachine.InputSlot;
        InventorySlotData outputSlot = currentMachine.OutputSlot;

        if (machineTitleText != null) {
            machineTitleText.text = currentMachine.name;
        }

        if (selectedRecipeText != null) {
            selectedRecipeText.text = recipe != null ? recipe.RecipeName : "Nincs recept kiválasztva";
        }

        if (selectedRecipeIcon != null) {
            selectedRecipeIcon.sprite = recipe != null ? recipe.Icon : null;
            selectedRecipeIcon.enabled = recipe != null && recipe.Icon != null;
        }

        if (inputText != null) {
            inputText.text = BuildInputText(inputSlot, recipe);
        }

        if (outputText != null) {
            outputText.text = BuildOutputText(outputSlot, recipe);
        }

        if (progressText != null) {
            progressText.text = recipe == null ? "Progress: 0%" : $"Progress: {Mathf.RoundToInt(currentMachine.CraftProgressNormalized * 100f)}%";
        }

        if (progressFill != null) {
            progressFill.fillAmount = currentMachine.CraftProgressNormalized;
        }

        for (int i = 0; i < spawnedButtons.Count; i++) {
            if (spawnedButtons[i] != null) {
                spawnedButtons[i].SetSelected(i == currentMachine.SelectedRecipeIndex);
            }
        }
    }

    private string BuildInputText(InventorySlotData slot, CrafterRecipeSO recipe) {
        string required = recipe == null || recipe.InputItem == null ? "-" : $"{recipe.InputItem.itemName} x{recipe.InputAmount}";
        if (slot == null || slot.IsEmpty) return $"Input: Empty\nRequired: {required}";
        return $"Input: {slot.item.itemName} x{slot.amount}\nRequired: {required}";
    }

    private string BuildOutputText(InventorySlotData slot, CrafterRecipeSO recipe) {
        string produces = recipe == null || recipe.OutputItem == null ? "-" : $"{recipe.OutputItem.itemName} x{recipe.OutputAmount}";
        if (slot == null || slot.IsEmpty) return $"Output: Empty\nProduces: {produces}";
        return $"Output: {slot.item.itemName} x{slot.amount}\nProduces: {produces}";
    }
}
