using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Шабуыл баптаулары")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField, Min(0.01f)] private float fireRate = 0.25f;
    [SerializeField] private int baseDamage = 35;

    [Header("Жаңарту деңгейлері (Upgrades)")]
    public int bulletCount = 1; // Бір атқандағы оқ саны (Multishot)
    public bool isExplosive = false; // Жарылғыш оқтар
    public bool hasLightningOrbs = false; // Найзағайлы орбита
    public float spreadAngle = 14f; // Оқтар арасындағы бұрыш

    private float nextFireTime;
    private AudioSource audioSource;
    private AudioClip shootClip;
    private List<GameObject> activeOrbs = new List<GameObject>();
    private float orbRotationAngle = 0f;

    private void Start()
    {
        SetupAudio();
    }

    private void Update()
    {
        // 1. Негізгі ату (Тінтуірдің сол батырмасы немесе автоматты ату)
        bool shootPressed = false;
        if (Mouse.current != null)
        {
            shootPressed = Mouse.current.leftButton.isPressed;
        }

        if (shootPressed && Time.time >= nextFireTime)
        {
            ShootAtMouse();
            nextFireTime = Time.time + fireRate;
        }

        // 2. Найзағайлы орбиталарды айналдыру
        if (hasLightningOrbs)
        {
            UpdateLightningOrbs();
        }
    }

    private void ShootAtMouse()
    {
        if (bulletPrefab == null) return;

        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        Ray mouseRay = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane playerPlane = new Plane(Vector3.forward, transform.position);
        if (!playerPlane.Raycast(mouseRay, out float distance)) return;

        Vector3 targetPoint = mouseRay.GetPoint(distance);
        Vector3 baseDir = (targetPoint - transform.position).normalized;
        if (baseDir.sqrMagnitude == 0f) return;

        PlayShootSound();

        // Multishot: Бірнеше оқты желпуіш (cone) тәрізді ату
        if (bulletCount == 1)
        {
            SpawnSingleBullet(baseDir);
        }
        else
        {
            float startAngle = -((bulletCount - 1) * spreadAngle) / 2f;
            for (int i = 0; i < bulletCount; i++)
            {
                float currentAngle = startAngle + (i * spreadAngle);
                Vector3 spreadDir = Quaternion.Euler(0, 0, currentAngle) * baseDir;
                SpawnSingleBullet(spreadDir);
            }
        }
    }

    private void SpawnSingleBullet(Vector3 direction)
    {
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        Bullet bScript = bullet.GetComponent<Bullet>();
        if (bScript != null)
        {
            bScript.Setup(direction, baseDamage, isExplosive);
        }
    }

    public void EnableLightningOrbs()
    {
        hasLightningOrbs = true;
        if (activeOrbs.Count == 0)
        {
            int orbCount = 3;
            for (int i = 0; i < orbCount; i++)
            {
                GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                orb.name = "LightningOrb_" + i;
                orb.transform.localScale = Vector3.one * 0.45f;

                Renderer rend = orb.GetComponent<Renderer>();
                if (rend != null) rend.material.color = new Color(0.2f, 0.85f, 1f);

                SphereCollider col = orb.GetComponent<SphereCollider>();
                if (col != null) col.isTrigger = true;

                activeOrbs.Add(orb);
            }
        }
    }

    private void UpdateLightningOrbs()
    {
        orbRotationAngle += 180f * Time.deltaTime;
        float radius = 2.2f;

        for (int i = 0; i < activeOrbs.Count; i++)
        {
            if (activeOrbs[i] == null) continue;

            float angle = orbRotationAngle + (i * (360f / activeOrbs.Count));
            float rad = angle * Mathf.Deg2Rad;
            Vector3 orbPos = transform.position + new Vector3(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius, 0f);
            activeOrbs[i].transform.position = orbPos;

            // Жақын маңдағы жауларға зақым келтіру
            Collider2D[] hits = Physics2D.OverlapCircleAll(orbPos, 0.5f);
            foreach (Collider2D hit in hits)
            {
                if (hit.name == "Player" || hit.CompareTag("Player")) continue;
                EnemyMovement em = hit.GetComponentInParent<EnemyMovement>();
                if (em != null) em.TakeDamage(18);

                RangedEnemy re = hit.GetComponentInParent<RangedEnemy>();
                if (re != null) re.TakeDamage(18);
            }
        }
    }

    private void SetupAudio()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // Синтезделген атыс дыбысы (Laser pew)
        int sampleRate = 44100;
        int count = (int)(sampleRate * 0.08f);
        float[] samples = new float[count];

        for (int i = 0; i < count; i++)
        {
            float t = (float)i / sampleRate;
            float freq = Mathf.Lerp(880f, 220f, (float)i / count);
            samples[i] = Mathf.Sin(2 * Mathf.PI * freq * t) * (1f - ((float)i / count)) * 0.25f;
        }

        shootClip = AudioClip.Create("SFX_Shoot", count, 1, sampleRate, false);
        shootClip.SetData(samples, 0);
    }

    private void PlayShootSound()
    {
        if (audioSource != null && shootClip != null)
        {
            audioSource.PlayOneShot(shootClip, 0.4f);
        }
    }
}