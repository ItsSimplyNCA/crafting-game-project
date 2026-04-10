using System.Reflection;
using Game.Gameplay.Machines.Common.Presentation;
using Game.Gameplay.WorldEntities.Presentation;
using Unity.VisualScripting;
using UnityEngine;

namespace Game.Gameplay.Machines.Processing.Presentation {
    [DisallowMultipleComponent]
    public sealed class ProcessorSceneInstaller : MonoBehaviour {
        [Header("References")]
        [SerializeField] private ProcessorDefinitionAuthoring definitionAuthoring;
        [SerializeField] private PlaceableView placeableView;
        [SerializeField] private MachineView machineView;
        [SerializeField] private ProcessorView processorView;
        [SerializeField] private ProcessorRuntimeBinder processorRuntimeBinder;

        [Header("Options")]
        [SerializeField] private bool autoResolveReferences = true;
        [SerializeField] private bool addMissingComponents = true;
        [SerializeField] private bool installOnAwake = true;
        [SerializeField] private bool createRuntimeAfterInstall = true;
        [SerializeField] private bool verboseLogging = false;

        private void Awake() {
            if (autoResolveReferences) {
                ResolveMissingReferences();
            }

            if (installOnAwake) {
                Install();
            }
        }

        private void OnValidate() {
            if (!Application.isPlaying && autoResolveReferences) {
                ResolveMissingReferences();
            }
        }

        [ContextMenu("Resolve Missing References")]
        public void ResolveMissingReferences() {
            if (definitionAuthoring == null) {
                definitionAuthoring = GetComponent<ProcessorDefinitionAuthoring>();
            }

            if (placeableView == null) {
                placeableView = GetComponent<PlaceableView>();
            }

            if (machineView == null) {
                machineView = GetComponent<MachineView>();
            }

            if (processorView == null) {
                processorView = GetComponent<ProcessorView>();
            }

            if (processorRuntimeBinder == null) {
                processorRuntimeBinder = GetComponent<ProcessorRuntimeBinder>();
            }
        }

        [ContextMenu("Install")]
        public void Install() {
            ResolveMissingReferences();

            if (addMissingComponents) {
                EnsureComponents();
            }

            ResolveMissingReferences();

            if (machineView == null || processorView == null || processorRuntimeBinder == null) {
                Debug.LogError("ProcessorSceneInstaller: hiányzik a MachineView, ProcessorView vagy ProcessorRuntimeBinder.", this);
                return;
            }

            if (definitionAuthoring == null || !definitionAuthoring.HasDefinition) {
                Debug.LogError("ProcessorSceneInstaller: nincs ProcessorDefinitionAuthoring vagy nincs benne ProcessorDefinition.", this);
                return;
            }

            machineView.SetMachineDefinition(definitionAuthoring.ProcessorDefinition.MachineDefinition);

            SetField(machineView, "placeableView", placeableView);
            SetField(processorView, "machineView", machineView);

            SetField(processorRuntimeBinder, "processorView", processorView);
            SetField(processorRuntimeBinder, "machineView", machineView);
            SetField(processorRuntimeBinder, "placeableView", placeableView);
            SetField(processorRuntimeBinder, "processorDefinition", definitionAuthoring.ProcessorDefinition);

            if (createRuntimeAfterInstall) {
                processorRuntimeBinder.ResolveMissingReferences();
                processorRuntimeBinder.TryCreateAndBindRuntime();
            }

            if (verboseLogging) {
                Debug.Log("ProcessorSceneInstaller: install kész.", this);
            }
        }

        private void EnsureComponents() {
            if (definitionAuthoring == null) {
                definitionAuthoring = gameObject.GetComponent<ProcessorDefinitionAuthoring>() ?? gameObject.AddComponent<ProcessorDefinitionAuthoring>();
            }

            if (machineView == null) {
                machineView = gameObject.GetComponent<MachineView>() ?? gameObject.AddComponent<MachineView>();
            }

            if (processorView == null) {
                processorView = gameObject.GetComponent<ProcessorView>() ?? gameObject.AddComponent<ProcessorView>();
            }

            if (processorRuntimeBinder == null) {
                processorRuntimeBinder = gameObject.GetComponent<ProcessorRuntimeBinder>() ?? gameObject.AddComponent<ProcessorRuntimeBinder>();
            }
        }

        private static void SetField(object target, string fieldName, object value) {
            if (target == null) return;

            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
            );

            if (field == null) {
                Debug.LogWarning($"ProcessorSceneInstaller: nem található mező: {fieldName}");
                return;
            }

            field.SetValue(target, value);
        }
    }
}