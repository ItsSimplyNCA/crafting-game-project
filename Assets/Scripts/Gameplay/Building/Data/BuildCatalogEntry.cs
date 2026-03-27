using System;
using Game.Gameplay.WorldEntities.Data;
using UnityEngine;

namespace Game.Gameplay.Building.Data {
    [Serializable]
    public sealed class BuildCatalogEntry {
        [SerializeField] private PlaceableDefinition definition;
        [SerializeField] private string displayNameOverride;
        [SerializeField] private KeyCode selectKey = KeyCode.None;
        [SerializeField] private bool enabled = true;

        public PlaceableDefinition Definition => definition;
        public string DisplayName =>
            !string.IsNullOrWhiteSpace(displayNameOverride)
                ? displayNameOverride
                : (definition != null ? definition.DisplayName : "Undefined");

        public KeyCode SelectKey => selectKey;
        public bool Enabled => enabled;
        public bool HasDefinition => definition != null;
        public bool HasExplicitHotkey => selectKey != KeyCode.None;
        public bool IsSelectable => enabled && definition != null;

        public BuildCatalogEntry(
            PlaceableDefinition definition,
            KeyCode selectKey = KeyCode.None,
            string displayNameOverride = null,
            bool enabled = true
        ) {
            this.definition = definition;
            this.selectKey = selectKey;
            this.displayNameOverride = displayNameOverride;
            this.enabled = enabled;
        }

        public static BuildCatalogEntry Create(
            PlaceableDefinition definition,
            KeyCode selectKey = KeyCode.None,
            string displayNameOverride = null,
            bool enabled = true
        ) {
            return new BuildCatalogEntry(definition, selectKey, displayNameOverride, enabled);
        }
    }
}