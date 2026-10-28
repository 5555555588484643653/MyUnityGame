using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Қозғалыс параметрлері")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float dashSpeed = 16f;
    [SerializeField] private float dashDuration = 0.16f;
    [SerializeField] private float dashCooldown = 1.1f;

    private InputAction moveAction;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    private bool isDashing = false;
    private float nextDashTime = 0f;
    private Vector2 lastMoveDir = Vector2.right;

    public bool IsInvincible => isDashing;
    public float SpeedMultiplier { get; set; } = 1.0f;

    private void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (isDashing) return;

        Vector2 input = Vector2.zero;
        if (moveAction != null)
        {
            input = moveAction.ReadValue<Vector2>();
        }
        else
        {
            // Input System бапталмаған жағдайдағы резервтік пернелер
            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) input.y += 1;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) input.y -= 1;
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) input.x -= 1;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) input.x += 1;
            }
        }

        if (input.sqrMagnitude > 0.01f)
        {
            lastMoveDir = input.normalized;
        }

        float currentSpeed = speed * SpeedMultiplier;
        Vector3 movement = new Vector3(input.x, input.y, 0f) * currentSpeed * Time.deltaTime;
        transform.position += movement;

        if (input.x != 0f && spriteRenderer != null)
        {
            spriteRenderer.flipX = input.x < 0f;
        }

        // КӨЛЕҢКЕЛІ СЕКІРУ (DASH): Space немесе Shift басылғанда
        bool dashPressed = false;
        if (Keyboard.current != null)
        {
            dashPressed = Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.leftShiftKey.wasPressedThisFrame;
        }

        if (dashPressed && Time.time >= nextDashTime)
        {
            StartCoroutine(PerformDash());
        }
    }

    private IEnumerator PerformDash()
    {
        isDashing = true;
        nextDashTime = Time.time + dashCooldown;

        Vector2 dashDir = lastMoveDir;
        CameraFollow.Shake(0.1f, 0.12f);

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            transform.position += (Vector3)(dashDir * dashSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;

            // Көлеңкелі із қалдыру (Ghost trail)
            SpawnGhostTrail();
            yield return null;
        }

        isDashing = false;
    }

    private void SpawnGhostTrail()
    {
        if (spriteRenderer == null) return;

        GameObject ghost = new GameObject("GhostTrail");
        ghost.transform.position = transform.position;
        ghost.transform.rotation = transform.rotation;
        ghost.transform.localScale = transform.localScale;

        SpriteRenderer ghostSr = ghost.AddComponent<SpriteRenderer>();
        ghostSr.sprite = spriteRenderer.sprite;
        ghostSr.flipX = spriteRenderer.flipX;
        ghostSr.color = new Color(0.3f, 0.6f, 1f, 0.55f);
        ghostSr.sortingOrder = spriteRenderer.sortingOrder - 1;

        StartCoroutine(FadeAndDestroyGhost(ghostSr));
    }

    private IEnumerator FadeAndDestroyGhost(SpriteRenderer sr)
    {
        float dur = 0.25f;
        float elapsed = 0f;
        Color startCol = sr.color;

        while (elapsed < dur)
        {
            if (sr == null) yield break;
            elapsed += Time.deltaTime;
            sr.color = new Color(startCol.r, startCol.g, startCol.b, Mathf.Lerp(startCol.a, 0f, elapsed / dur));
            yield return null;
        }

        if (sr != null) Destroy(sr.gameObject);
    }
}