using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target; // Кімнің артынан ереміз
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f); // Камераның қашықтығы
    [SerializeField] private float smoothTime = 0.2f; // Жылжу бірқалыптылығы

    private Vector3 velocity;

    private void LateUpdate()
    {
        if (target == null) return; // Егер нысан жоқ болса, ештеңе істемейміз

        Vector3 targetPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(
            transform.position, targetPosition, ref velocity, smoothTime);
    }
}