public interface IMachineInteractable {
    bool CanInteract { get; }
    void BeginInteraction();
    void EndInteraction();
}