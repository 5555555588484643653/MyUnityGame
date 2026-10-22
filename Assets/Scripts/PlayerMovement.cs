using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    private InputAction moveAction;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        Vector3 movement = new Vector3(input.x, input.y, 0f) * speed * Time.deltaTime;
        transform.Translate(movement);

        if (input.x != 0f)
        {
            spriteRenderer.flipX = input.x < 0f;
        }
    }
}