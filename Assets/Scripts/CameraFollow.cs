using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance { get; private set; }

    [SerializeField] private Transform target; // Кімнің артынан ереміз
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f); // Камераның қашықтығы
    [SerializeField] private float smoothTime = 0.15f; // Жылжу бірқалыптылығы

    private Vector3 velocity;
    private float shakeTimeRemaining;
    private float shakeMagnitude;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.Find("Player");
            if (player != null) target = player.transform;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position + offset;
        Vector3 newPos = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

        // Экран сілкінісі (Screen Shake)
        if (shakeTimeRemaining > 0)
        {
            shakeTimeRemaining -= Time.deltaTime;
            Vector2 randomOffset = Random.insideUnitCircle * shakeMagnitude;
            newPos += new Vector3(randomOffset.x, randomOffset.y, 0f);
        }

        transform.position = newPos;
    }

    /// <summary>
    /// Соққы мен жарылыстар кезінде камераны сілку
    /// </summary>
    public static void Shake(float duration = 0.12f, float magnitude = 0.18f)
    {
        if (Instance != null)
        {
            Instance.shakeTimeRemaining = duration;
            Instance.shakeMagnitude = magnitude;
        }
    }
}