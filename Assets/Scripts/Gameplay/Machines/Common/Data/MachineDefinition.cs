using Game.Gameplay.WorldEntities.Data;
using UnityEngine;

namespace Game.Gameplay.Machines.Common.Data {
    [CreateAssetMenu(fileName = "MachineDefinition", menuName = "Game/Machines/Machine Definition")]
    public sealed class MachineDefinition : ScriptableObject {
        [Header("Identity")]
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [TextArea]
        [SerializeField] private string description;
        [SerializeField] private Sprite icon;

        [Header("Placement")]
        [SerializeField] private PlaceableDefinition placeableDefinition;
        
        [Header("Buffers")]
        [SerializeField, Min(0)] private int inputSlotCount = 1;
        [SerializeField, Min(0)] private int outputSlotCount = 1;
        [SerializeField, Min(0)] private int internalSlotCount = 0;

        [Header("Work")]
        [SerializeField, Min(0.01f)] private float workDuration = 1f;
        [SerializeField] private bool startsEnabled = true;

        public string Id => string.IsNullOrWhiteSpace(id) ? name : id;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public string Description => description;
        public Sprite Icon => icon;

        public PlaceableDefinition PlaceableDefinition => placeableDefinition;

        public int InputSlotCount => inputSlotCount;
        public int OutputSlotCount => outputSlotCount;
        public int InternalSlotCount => internalSlotCount;

        public float WorkDuration => Mathf.Max(0.01f, workDuration);
        public bool StartsEnabled => startsEnabled;

        private void OnValidate() {
            inputSlotCount = Mathf.Max(0, inputSlotCount);
            outputSlotCount = Mathf.Max(0, outputSlotCount);
            internalSlotCount = Mathf.Max(0, internalSlotCount);
            workDuration = Mathf.Max(0.01f, workDuration);

            if (string.IsNullOrWhiteSpace(displayName)) {
                displayName = name;
            }
        }
    }
}