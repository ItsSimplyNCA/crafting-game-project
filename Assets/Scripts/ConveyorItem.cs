using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ConveyorItem : MonoBehaviour
{
    public ConveyorBelt CurrentBelt { get; private set; }
    public float DistanceOnBelt { get; set; }
    public int CurrentEntryIndex { get; private set; }

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void AttachToBelt(ConveyorBelt belt, float startDistance, int entryIndex = 0)
    {
        CurrentBelt = belt;
        DistanceOnBelt = Mathf.Max(0f, startDistance);
        CurrentEntryIndex = Mathf.Clamp(entryIndex, 0, 2);

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void DetachFromBelt(bool restorePhysics)
    {
        CurrentBelt = null;
        DistanceOnBelt = 0f;
        CurrentEntryIndex = 0;

        if (rb != null && restorePhysics)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }
}