using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    private TextMesh textMesh;
    private float lifetime = 0.65f;
    private float timer;
    private Vector3 moveVector;
    private Color textColor;

    public static void Create(Vector3 position, int damage, bool isCrit = false)
    {
        GameObject popupObj = new GameObject("DamagePopup");
        popupObj.transform.position = position + new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(0.2f, 0.5f), 0f);

        DamagePopup popup = popupObj.AddComponent<DamagePopup>();
        popup.Setup(damage, isCrit);
    }

    public void Setup(int damage, bool isCrit)
    {
        textMesh = gameObject.AddComponent<TextMesh>();
        MeshRenderer mr = GetComponent<MeshRenderer>();
        mr.sortingOrder = 200; // Әрқашан алдыңғы планда көрінеді

        textMesh.text = isCrit ? $"-{damage} CRIT!" : $"-{damage}";
        textMesh.fontSize = isCrit ? 36 : 28;
        textMesh.characterSize = 0.08f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.fontStyle = isCrit ? FontStyle.Bold : FontStyle.Normal;

        textColor = isCrit ? new Color(1f, 0.85f, 0.2f) : (damage > 40 ? new Color(1f, 0.35f, 0.2f) : Color.white);
        textMesh.color = textColor;

        moveVector = new Vector3(Random.Range(-0.8f, 0.8f), 2.2f, 0f);
        timer = lifetime;
    }

    private void Update()
    {
        transform.position += moveVector * Time.deltaTime;
        moveVector -= moveVector * 2.5f * Time.deltaTime;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        // Жоғарылаған сайын өшу (Fade out)
        if (textMesh != null)
        {
            float alpha = timer / lifetime;
            textMesh.color = new Color(textColor.r, textColor.g, textColor.b, alpha);
        }
    }
}
