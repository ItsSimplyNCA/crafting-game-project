using Game.Gameplay.Machines.Common.Data;
using UnityEngine;

namespace Game.Gameplay.Machines.Storage.Data {
    [CreateAssetMenu(fileName = "StorageDefinition", menuName = "Game/Machines/Storage Definition")]
    public sealed class StorageDefinition : ScriptableObject {
        [Header("Identity")]
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [TextArea]
        [SerializeField] private string description;
        [SerializeField] private Sprite icon;

        [Header("References")]
        [SerializeField] private MachineDefinition machineDefinition;

        [Header("Storage")]
        [SerializeField, Min(1)] private int slotCount = 16;

        public string Id => string.IsNullOrWhiteSpace(id) ? name : id;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public string Description => description;
        public Sprite Icon => icon;

        public MachineDefinition MachineDefinition => machineDefinition;
        public int SlotCount => Mathf.Max(1, slotCount);

        private void OnValidate() {
            slotCount = Mathf.Max(1, slotCount);

            if (string.IsNullOrWhiteSpace(displayName)) {
                displayName = name;
            }
        } 
    }
}