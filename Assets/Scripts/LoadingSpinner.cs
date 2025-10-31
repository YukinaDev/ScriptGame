using UnityEngine;

public class LoadingSpinner : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 200f;

    void Update()
    {
        transform.Rotate(0f, 0f, -rotationSpeed * Time.deltaTime);
    }
}
