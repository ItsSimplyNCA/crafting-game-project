using System;
using Game.Gameplay.Recipes.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Machines {
    [DisallowMultipleComponent]
    public sealed class ProcessorRecipeButtonView : MonoBehaviour {
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private Image iconImage;
        [SerializeField] private GameObject selectedMarker;

        private int recipeIndex;
        private Action<int> onClicked;

        private void Awake() {
            if (button == null) {
                button = GetComponent<Button>();
            }
        }

        public void Setup(RecipeDefinition recipe, int index, Action<int> clickCallback, bool selected) {
            recipeIndex = index;
            onClicked = clickCallback;

            if (labelText != null) {
                labelText.text = recipe != null ? recipe.DisplayName : "Unknown Recipe";
            }

            if (iconImage != null) {
                iconImage.sprite = recipe != null ? recipe.Icon : null;
                iconImage.enabled = recipe != null && recipe.Icon != null;
            }

            SetSelected(selected);

            if (button != null) {
                button.onClick.RemoveListener(HandleClicked);
                button.onClick.AddListener(HandleClicked);
            }
        }

        public void SetSelected(bool selected) {
            if (selectedMarker != null) {
                selectedMarker.SetActive(selected);
            }
        }

        private void HandleClicked() {
            onClicked?.Invoke(recipeIndex);
        }

        private void OnDestroy() {
            if (button != null) {
                button.onClick.RemoveListener(HandleClicked);
            }
        }
    }
}