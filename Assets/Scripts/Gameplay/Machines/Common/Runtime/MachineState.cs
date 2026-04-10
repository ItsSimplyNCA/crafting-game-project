namespace Game.Gameplay.Machines.Common.Runtime {
    public enum MachineState {
        Disabled = 0,
        Idle = 1,
        WaitingForInput = 2,
        Processing = 3,
        BlockedInput = 4,
        Error = 5
    }
}