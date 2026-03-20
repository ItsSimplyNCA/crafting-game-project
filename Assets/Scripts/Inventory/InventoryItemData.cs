using UnityEngine;

[CreateAssetMenu(fileName = "InventoryItem", menuName = "Crafting Game/Inventory Item")]
public class InventoryItemData : ScriptableObject {
    [Header("Basic")]
    public string itemName;

    [Header("UI")]
    public Sprite icon;

    [Header("Stacking")]
    [Min(1)] public int maxStack = 99;

    [Header("World")]
    public GameObject worldModelPrefab;
}