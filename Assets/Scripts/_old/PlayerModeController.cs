using UnityEngine;

public class PlayerModeController : MonoBehaviour {
    public enum PlayerMode {
        None,
        Build,
        Remove,
        Inventory,
        Machine
    }

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private SimpleFPSController fpsController;
    [SerializeField] private BuildingSystem buildingSystem;
    [SerializeField] private InventorySystem inventorySystem;

    [Header("Mode Keys")]
    [SerializeField] private KeyCode buildModeKey = KeyCode.Q;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private KeyCode removeModeKey = KeyCode.F;
    [SerializeField] private KeyCode inventoryKey = KeyCode.Tab;
    [SerializeField] private KeyCode cancelKey = KeyCode.Escape;

    [Header("Machine Interaction")]
    [SerializeField] private float interactionDistance = 4f;
    [SerializeField] private LayerMask interactionMask = ~0;

    public PlayerMode CurrentMode { get; private set; } = PlayerMode.None;

    private IMachineInteractable activeMachine;

    private void Awake() {
        if (playerCamera == null) playerCamera = Camera.main;
        if (fpsController == null) fpsController = GetComponent<SimpleFPSController>();
        if (buildingSystem == null) buildingSystem = FindObjectOfType<BuildingSystem>();
        if (inventorySystem == null) inventorySystem = InventorySystem.Instance;
    }

    private void Start() {
        if (playerCamera == null) playerCamera = Camera.main;
        if (buildingSystem == null) buildingSystem = FindObjectOfType<BuildingSystem>();
        if (inventorySystem == null) inventorySystem = InventorySystem.Instance;

        SetMode(PlayerMode.None);
    }

    private void Update() {
        if (CurrentMode == PlayerMode.Machine && activeMachine == null) {
            SetMode(PlayerMode.None);
            return;
        }

        if (Input.GetKeyDown(cancelKey)) {
            SetMode(PlayerMode.None);
            return;
        }

        if (Input.GetKeyDown(inventoryKey)) {
            ToggleInventoryMode();
            return;
        }

        if (Input.GetKeyDown(buildModeKey)) {
            ToggleBuildMode();
            return;
        }

        if (Input.GetKeyDown(removeModeKey)) {
            ToggleRemoveMode();
            return;
        }

        if (Input.GetKeyDown(interactKey)) {
            TryEnterMachineMode();
        }
    }

    public void ToggleBuildMode() {
        SetMode(CurrentMode == PlayerMode.Build ? PlayerMode.None : PlayerMode.Build);
    }

    public void ToggleRemoveMode() {
        SetMode(CurrentMode == PlayerMode.Remove ? PlayerMode.None : PlayerMode.Remove);
    }

    public void ToggleInventoryMode() {
        SetMode(CurrentMode == PlayerMode.Inventory ? PlayerMode.None : PlayerMode.Inventory);
    }

    public void ExitCurrentMode() {
        SetMode(PlayerMode.None);
    }

    private void TryEnterMachineMode() {
        if (CurrentMode == PlayerMode.Inventory || CurrentMode == PlayerMode.Machine) return;
        if (!TryFindMachineInteractable(out IMachineInteractable machine)) return;
        if (!machine.CanInteract) return;

        SetMode(PlayerMode.Machine, machine);
    }

    private void SetMode(PlayerMode newMode, IMachineInteractable machine = null) {
        if (CurrentMode == newMode && activeMachine == machine) return;

        ExitCurrentModeInternal();
        activeMachine = machine;

        switch (CurrentMode) {
            case PlayerMode.Build:
                buildingSystem?.EnterBuildMode();
                break;
            case PlayerMode.Remove:
                buildingSystem?.EnterRemoveMode();
                break;
            case PlayerMode.Inventory:
                inventorySystem?.Open();
                break;
            case PlayerMode.Machine:
                activeMachine?.BeginInteraction();
                break;
        }

        ApplyState();
    }

    private void ExitCurrentModeInternal() {
        switch (CurrentMode) {
            case PlayerMode.Build:
            case PlayerMode.Remove:
                buildingSystem?.CancelCurrentMode();
                break;
            case PlayerMode.Inventory:
                inventorySystem?.Close();
                break;
            case PlayerMode.Machine:
            activeMachine?.EndInteraction();
            activeMachine = null;
            break;
        }

        CurrentMode = PlayerMode.None;
    }

    private void ApplyState() {
        bool gameplayInputEnabled = CurrentMode != PlayerMode.Inventory && CurrentMode != PlayerMode.Machine;

        if (fpsController != null) {
            fpsController.SetInputEnabled(gameplayInputEnabled);
        }

        if (gameplayInputEnabled) {
            SimpleFPSController.LockCursor();
        } else {
            SimpleFPSController.UnlockCursor();
        }
    }

    private bool TryFindMachineInteractable(out IMachineInteractable machine) {
        machine = null;

        if (playerCamera == null) return false;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (!Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactionMask, QueryTriggerInteraction.Ignore)) return false;

        MonoBehaviour[] behaviours = hit.collider.GetComponentsInParent<MonoBehaviour>(true);

        foreach (MonoBehaviour behaviour in behaviours) {
            if (behaviour is IMachineInteractable interactable) {
                machine = interactable;
                return true;
            }
        }

        return false;
    }

}
