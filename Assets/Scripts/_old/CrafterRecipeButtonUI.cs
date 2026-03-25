using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CrafterRecipeButtonUI : MonoBehaviour {
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text label;
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject selectedMarker;

    private int recipeIndex;
    private Action<int> onClicked;

    private void Awake() {
        if (button == null) {
            button = GetComponent<Button>();
        }
    }

    public void Setup(CrafterRecipeSO recipe, int index, Action<int> clickCallback, bool isSelected) {
        recipeIndex = index;
        onClicked = clickCallback;

        if (label != null) {
            label.text = recipe != null ? recipe.RecipeName : "Unknown Recipe";
        }

        if (iconImage != null) {
            iconImage.sprite = recipe != null ? recipe.Icon : null;
            iconImage.enabled = recipe != null && recipe.Icon != null;
        }

        SetSelected(isSelected);

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
