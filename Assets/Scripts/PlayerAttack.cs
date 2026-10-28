using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private GameObject bulletPrefab; // Оқтың префабы (Bullet)
    [SerializeField, Min(0f)] private float fireRate = 0.005f;
    private float nextFireTime;

    private void Update()
    {
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame &&
            Time.time >= nextFireTime)
        {
            ShootAtMousePosition();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void ShootAtMousePosition()
    {
        if (bulletPrefab == null)
        {
            Debug.LogError("PlayerAttack: Inspector-да Bullet Prefab салынбаған!");
            return;
        }

        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogError("Main Camera табылмады! Камераның Tag-ін 'MainCamera' деп қойыңыз.");
            return;
        }

        Ray mouseRay = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane playerPlane = new Plane(Vector3.forward, transform.position);
        if (!playerPlane.Raycast(mouseRay, out float distance))
        {
            return;
        }

        Vector3 direction = (mouseRay.GetPoint(distance) - transform.position).normalized;
        if (direction.sqrMagnitude == 0f)
        {
            return;
        }

        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.Setup(direction);
        }
        else
        {
            Debug.LogError("PlayerAttack: Bullet Prefab ішінде Bullet скрипті жоқ!");
            Destroy(bullet);
        }
    }
}