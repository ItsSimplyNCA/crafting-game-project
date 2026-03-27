using Game.Gameplay.WorldEntities.Data;
using UnityEngine;

namespace Game.Gameplay.WorldEntities.Presentation {
    [DisallowMultipleComponent]
    public sealed class PlaceableDefinitionAuthoring : MonoBehaviour {
        [SerializeField] private PlaceableDefinition definition;

        public PlaceableDefinition Definition => definition;
        public bool HasDefinition => definition != null;

        public void SetDefinition(PlaceableDefinition newDefinition) {
            definition = newDefinition;
        }

        [ContextMenu("Log Definition")]
        public void LogDefinition() {
            Debug.Log(
                definition != null
                    ? $"PlaceableDefinitionAuthoring: {definition.DisplayName}"
                    : "PlaceableDefinitionAuthoring: nincs definition beállítva.",
                this
            );
        }
    }
}