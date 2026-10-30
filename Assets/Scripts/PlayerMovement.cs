using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    private InputAction moveAction;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private static readonly int SpeedParameter = Animator.StringToHash("Speed");

    public float Speed => speed;

    public void UpgradeSpeed(float amount)
    {
        speed += Mathf.Max(0f, amount);
    }

    public void RestoreSpeed(float savedSpeed)
    {
        speed = Mathf.Max(0.1f, savedSpeed);
    }

    private void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (moveAction == null)
        {
            return;
        }

        Vector2 input = moveAction.ReadValue<Vector2>();
        if (animator != null)
        {
            animator.SetFloat(SpeedParameter, input.sqrMagnitude);
        }

        Vector3 movement = new Vector3(input.x, input.y, 0f) * speed * Time.deltaTime;
        transform.Translate(movement);

        if (input.x != 0f && spriteRenderer != null)
        {
            spriteRenderer.flipX = input.x < 0f;
        }
    }
}