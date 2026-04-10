using Game.Gameplay.Machines.Common.Data;
using Game.Gameplay.Recipes.Data;
using UnityEngine;

namespace Game.Gameplay.Machines.Processing.Data {
    [CreateAssetMenu(fileName = "ProcessorDefinition", menuName = "Game/Machines/Processor Definition")]
    public sealed class ProcessorDefinition : ScriptableObject {
        [Header("Identity")]
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [TextArea]
        [SerializeField] private string description;
        [SerializeField] private Sprite icon;

        [Header("References")]
        [SerializeField] private MachineDefinition machineDefinition;

        [Header("Recipes")]
        [SerializeField] private RecipeDefinition[] availableRecipes;
        [SerializeField] private int defaultRecipeIndex = 0;

        public string Id => string.IsNullOrWhiteSpace(id) ? name : id;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public string Description => description;
        public Sprite Icon => icon;

        public MachineDefinition MachineDefinition => machineDefinition;
        public RecipeDefinition[] AvailableRecipes => availableRecipes ?? System.Array.Empty<RecipeDefinition>();
        public int DefaultRecipeIndex => Mathf.Max(0, defaultRecipeIndex);

        public bool HasMachineDefinition => machineDefinition != null;
        public bool HasRecipes => availableRecipes != null && availableRecipes.Length > 0;

        public RecipeDefinition GetDefaultRecipe() {
            if (!HasRecipes) return null;

            int index = Mathf.Clamp(defaultRecipeIndex, 0, availableRecipes.Length - 1);
            return availableRecipes[index];
        }

        private void OnValidate() {
            defaultRecipeIndex = Mathf.Max(0, defaultRecipeIndex);

            if (string.IsNullOrWhiteSpace(displayName)) {
                displayName = name;
            }
        }
    }
}