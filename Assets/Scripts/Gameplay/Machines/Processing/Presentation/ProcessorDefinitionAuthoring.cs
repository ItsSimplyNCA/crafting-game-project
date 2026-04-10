using Game.Gameplay.Machines.Processing.Data;
using UnityEngine;

namespace Game.Gameplay.Machines.Processing.Presentation {
    [DisallowMultipleComponent]
    public sealed class ProcessorDefinitionAuthoring : MonoBehaviour {
        [SerializeField] private ProcessorDefinition processorDefinition;

        public ProcessorDefinition ProcessorDefinition => processorDefinition;
        public bool HasDefinition => processorDefinition != null;

        public void SetDefinition(ProcessorDefinition definition) {
            processorDefinition = definition;
        }

        [ContextMenu("Log Processor Definition")]
        public void LogProcessorDefinition() {
            Debug.Log(
                processorDefinition != null
                    ? $"ProcessorDefinitionAuthoring: {processorDefinition.DisplayName}"
                    : "ProcessorDefinitionAuthoring: nincs ProcessorDefinition beállítva.",
                this
            );
        }
    }
}