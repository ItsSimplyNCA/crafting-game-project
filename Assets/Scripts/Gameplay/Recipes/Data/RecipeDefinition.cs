using UnityEngine;

namespace Game.Gameplay.Recipes.Data {
    [CreateAssetMenu(fileName = "RecipeDefinition", menuName = "Game/Recipes/Recipe Definition")]
    public sealed class RecipeDefinition : ScriptableObject {
        [Header("Identity")]
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [TextArea]
        [SerializeField] private string description;
        [SerializeField] private Sprite icon;

        [Header("Recipe")]
        [SerializeField] private RecipeIngredient[] inputs;
        [SerializeField] private RecipeOutput[] outputs;
        [SerializeField, Min(0.01f)] private float craftDuration = 1f;

        public string Id => string.IsNullOrWhiteSpace(id) ? name : id;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public string Description => description;
        public Sprite Icon => icon;

        public RecipeIngredient[] Inputs => inputs ?? System.Array.Empty<RecipeIngredient>();
        public RecipeOutput[] Outputs => outputs ?? System.Array.Empty<RecipeOutput>();
        public float CraftDuration => Mathf.Max(0.01f, craftDuration);

        private void OnValidate() {
            craftDuration = Mathf.Max(0.01f, craftDuration);

            if (string.IsNullOrWhiteSpace(displayName)) {
                displayName = name;
            }

            if (inputs != null) {
                for (int i = 0; i < inputs.Length; i++) {
                    inputs[i]?.Clamp();
                }
            }

            if (outputs != null) {
                for (int i = 0; i < outputs.Length; i++) {
                    outputs[i]?.Clamp();
                }
            }
        } 
    }
}