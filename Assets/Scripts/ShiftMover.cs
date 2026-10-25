using UnityEngine;

public class ShiftMover : MonoBehaviour, ITriggerable
{
    [Header("Player Tracking Settings")]
    public LayerMask playerLayer = ~0;
    public Vector3 checkDetectionSize = new Vector3(3f, 1.5f, 3f);
    public Vector3 checkOffset = new Vector3(0f, 1.2f, 0f);

    private Vector3 previousPosition;
    private bool wasTriggeredThisFrame;

    void Start()
    {
        previousPosition = transform.position;
    }

    public void TriggerMove(Vector3 direction, float speed)
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
        wasTriggeredThisFrame = true;
    }

    void LateUpdate()
    {
        Vector3 platformDelta = transform.position - previousPosition;

        if (wasTriggeredThisFrame && platformDelta.sqrMagnitude > 0.0001f)
        {
            Collider[] hitColliders = Physics.OverlapBox(transform.position + checkOffset, checkDetectionSize / 2f, transform.rotation, playerLayer);

            foreach (var hit in hitColliders)
            {
                CharacterController cc = hit.GetComponent<CharacterController>();
                if (cc != null)
                {
                    cc.Move(platformDelta);
                }
            }
        }

        previousPosition = transform.position;
        wasTriggeredThisFrame = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + checkOffset, checkDetectionSize);
    }
}
