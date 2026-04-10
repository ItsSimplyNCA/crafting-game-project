using System;
using UnityEngine;

namespace Game.Gameplay.Recipes.Data {
    [Serializable]
    public sealed class RecipeOutput {
        [SerializeField] private InventoryItemData item;
        [SerializeField, Min(1)] private int amount = 1;

        public InventoryItemData Item => item;
        public int Amount => Mathf.Max(1, amount);
        public bool IsValid => item != null && Amount > 0;

        public void Clamp() {
            amount = Mathf.Max(1, amount);
        } 
    }
}