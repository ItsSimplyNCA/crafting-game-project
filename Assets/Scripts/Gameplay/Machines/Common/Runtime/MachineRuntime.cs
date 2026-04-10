using System;
using Game.Gameplay.Machines.Common.Data;
using Game.Gameplay.WorldEntities.Runtime;
using UnityEngine;

namespace Game.Gameplay.Machines.Common.Runtime {
    [Serializable]
    public class MachineRuntime {
        private readonly string runtimeId;
        private readonly MachineDefinition definition;
        private readonly PlaceableRuntime placeableRuntime;
        private readonly MachineBufferSet buffers;

        private MachineState state;
        private float progressSeconds;
        private  bool isEnabled;

        public event Action<MachineRuntime> Changed;

        public string RuntimeId => runtimeId;
        public MachineDefinition Definition => definition;
        public PlaceableRuntime PlaceableRuntime => placeableRuntime;
        public MachineBufferSet Buffers => buffers;

        public MachineState State => state;
        public float ProgressSeconds => progressSeconds;
        public float WorkDuration => definition != null ? definition.WorkDuration : 0.01f;
        public float ProgressNormalized => WorkDuration <= 0f ? 0f : Mathf.Clamp01(progressSeconds / WorkDuration);
        public bool IsEnabled => isEnabled;
        public bool IsProcessing => state == MachineState.Processing;
        public bool IsWorkComplete => progressSeconds >= WorkDuration;

        public MachineRuntime(MachineDefinition definition, PlaceableRuntime placeableRuntime) {
            if (definition == null) {
                throw new ArgumentNullException(nameof(definition));
            }

            if (placeableRuntime == null) {
                throw new ArgumentNullException(nameof(placeableRuntime));
            }

            runtimeId = Guid.NewGuid().ToString("N");
            this.definition = definition;
            this.placeableRuntime = placeableRuntime;

            buffers = new MachineBufferSet(
                definition.InputSlotCount,
                definition.OutputSlotCount,
                definition.InternalSlotCount
            );

            buffers.Changed += HandleBuffersChanged;

            isEnabled = definition.StartsEnabled;
            state = isEnabled ? MachineState.Idle : MachineState.Disabled;
            progressSeconds = 0f;
        }

        public void SetEnabled(bool enabled) {
            if (isEnabled == enabled) return;

            isEnabled = enabled;

            if (!isEnabled) {
                state = MachineState.Disabled;
                progressSeconds = 0f;
            } else if (state == MachineState.Disabled) {
                state = MachineState.Idle;
            }

            NotifyChanged();
        }

        public void SetState(MachineState newState) {
            if (!isEnabled && newState != MachineState.Disabled) {
                newState = MachineState.Disabled;
            }

            if (state == newState) return;

            state = newState;
            NotifyChanged();
        }

        public void ResetProgress(bool notify = true) {
            if (Mathf.Approximately(progressSeconds, 0f)) return;

            progressSeconds = 0f;

            if (notify) {
                NotifyChanged();
            }
        }

        public void BeginProcessing(bool resetProgress = true) {
            if (!isEnabled) return;

            state = MachineState.Processing;

            if (resetProgress) {
                progressSeconds = 0f;
            }

            NotifyChanged();
        }

        public void Advance(float deltaTime) {
            if (!isEnabled || state != MachineState.Processing) return;
            if (deltaTime <= 0f) return;

            progressSeconds = Mathf.Min(progressSeconds + deltaTime, WorkDuration);
            NotifyChanged();
        }

        public void MarkWaitingForInput(bool resetProgress = true) {
            state = isEnabled ? MachineState.WaitingForInput : MachineState.Disabled;

            if (resetProgress) progressSeconds = 0f;

            NotifyChanged();
        }

        public void MarkBlockedOutput() {
            state = isEnabled ? MachineState.BlockedInput : MachineState.Disabled;
            NotifyChanged();
        }

        public void MarkIdle(bool resetProgress = false) {
            state = isEnabled ? MachineState.Idle : MachineState.Disabled;
            
            if (resetProgress) {
                progressSeconds = 0f;
            }

            NotifyChanged();
        }

        public void MarkError() {
            state = MachineState.Error;
            NotifyChanged();
        }

        protected virtual void HandleBuffersChanged() {
            NotifyChanged();
        }

        protected void NotifyChanged() {
            Changed?.Invoke(this);
        }

        public override string ToString() {
            return $"{definition.DisplayName} [{runtimeId}] State={state} Progress={progressSeconds:0.00}/{WorkDuration:0.00}";
        }
    }
}