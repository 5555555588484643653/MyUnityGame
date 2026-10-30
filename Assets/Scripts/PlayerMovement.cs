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
    private Animator animator;
    private bool isDashing;
    private float nextDashTime;
    private Vector2 lastMoveDir = Vector2.right;
    private static readonly int SpeedParameter = Animator.StringToHash("Speed");

    public float Speed => speed;
    public bool IsInvincible => isDashing;
    public float SpeedMultiplier { get; set; } = 1f;

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
        if (isDashing)
        {
            return;
        }

        Vector2 input = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
        if (moveAction == null && Keyboard.current != null)
        {
            input.x = (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed ? 1f : 0f) -
                (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed ? 1f : 0f);
            input.y = (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed ? 1f : 0f) -
                (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed ? 1f : 0f);
        }

        if (input.sqrMagnitude > 0.01f)
        {
            lastMoveDir = input.normalized;
        }

        if (animator != null)
        {
            animator.SetFloat(SpeedParameter, input.sqrMagnitude);
        }

        Vector3 movement = new Vector3(input.x, input.y, 0f) * speed * SpeedMultiplier * Time.deltaTime;
        transform.Translate(movement);

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