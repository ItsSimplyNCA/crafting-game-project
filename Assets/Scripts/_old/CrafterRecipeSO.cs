using UnityEngine;

[CreateAssetMenu(fileName = "CrafterRecipe", menuName = "Factory/Crafter Recipe")]
public class CrafterRecipeSO : ScriptableObject {
    [Header("Display")]
    [SerializeField] private string recipeName;
    [SerializeField] private Sprite icon;

    [Header("Recipe")]
    [SerializeField] private InventoryItemData inputItem;
    [SerializeField, Min(1)] private int inputAmount = 1;
    [SerializeField] private InventoryItemData outputItem;
    [SerializeField, Min(1)] private int outputAmount = 1;
    [SerializeField, Min(0.01f)] private float craftDuration = 2f;

    public string RecipeName => string.IsNullOrWhiteSpace(recipeName) ? name : recipeName;
    public Sprite Icon => icon;
    public InventoryItemData InputItem => inputItem;
    public int InputAmount => inputAmount;
    public InventoryItemData OutputItem => outputItem;
    public int OutputAmount => outputAmount;
    public float CraftDuration => craftDuration;

    private void OnValidate() {
        inputAmount = Mathf.Max(1, inputAmount);
        outputAmount = Mathf.Max(1, outputAmount);
        craftDuration = Mathf.Max(0.01f, craftDuration);
    } 
}
