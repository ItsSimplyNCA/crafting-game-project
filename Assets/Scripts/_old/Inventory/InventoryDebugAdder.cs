using UnityEngine;

public class InventoryDebugAdder : MonoBehaviour {
    [SerializeField] private InventoryItemData testItem;
    [SerializeField] private int amount = 1;
    [SerializeField] private KeyCode addKey = KeyCode.G;

    void Update() {
        if (Input.GetKeyDown(addKey) && InventorySystem.Instance != null) {
            InventorySystem.Instance.AddItem(testItem, amount);
        }
    }
}
