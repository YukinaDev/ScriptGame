using UnityEngine;

public class TopDownCamera2D : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform target;
    public float height = 15f;
    public float smoothSpeed = 10f;
    public Vector2 offset = Vector2.zero;

    [Header("Bounds")]
    public bool useBounds = true;
    public Vector2 minBounds = new Vector2(-50, -50);
    public Vector2 maxBounds = new Vector2(50, 50);

    void Start()
    {
        transform.rotation = Quaternion.Euler(90, 0, 0);
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = target.position;
        Vector3 desiredPosition = new Vector3(
            targetPos.x + offset.x,
            height,
            targetPos.z + offset.y
        );

        if (useBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
            desiredPosition.z = Mathf.Clamp(desiredPosition.z, minBounds.y, maxBounds.y);
        }

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}
