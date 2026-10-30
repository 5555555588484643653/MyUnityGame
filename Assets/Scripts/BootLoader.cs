using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class BootLoader : MonoBehaviour
{
    [SerializeField] private Slider progressBar;
    [SerializeField] private Image loadingZombie;
    [SerializeField] private string nextScene = "MainMenu";
    [SerializeField, Min(0f)] private float minimumDisplayTime = 2f;

    private IEnumerator Start()
    {
        RectTransform zombieRect = loadingZombie != null ? loadingZombie.rectTransform : null;
        if (zombieRect != null)
        {
            zombieRect.localScale = new Vector3(-1f, 1f, 1f);
        }

        if (string.IsNullOrWhiteSpace(nextScene) ||
            !Application.CanStreamedLevelBeLoaded(nextScene))
        {
            Debug.LogError($"BootLoader: '{nextScene}' сахнасы жүктеуге қолжетімсіз.");
            yield break;
        }

        AsyncOperation operation = SceneManager.LoadSceneAsync(nextScene);
        if (operation == null)
        {
            Debug.LogError($"BootLoader: '{nextScene}' сахнасын жүктеу басталмады.");
            yield break;
        }

        operation.allowSceneActivation = false;
        float elapsed = 0f;
        float displayDuration = Mathf.Max(0f, minimumDisplayTime);

        while (elapsed < displayDuration || operation.progress < 0.9f)
        {
            elapsed += Time.unscaledDeltaTime;
            float timeProgress = displayDuration > 0f
                ? Mathf.Clamp01(elapsed / displayDuration)
                : 1f;
            float loadProgress = Mathf.Clamp01(operation.progress / 0.9f);
            float progress = Mathf.Min(loadProgress, timeProgress);

            if (progressBar != null)
            {
                progressBar.value = Mathf.Lerp(progressBar.minValue, progressBar.maxValue, progress);
            }

            float displayedProgress = progressBar != null
                ? progressBar.normalizedValue
                : progress;
            UpdateLoadingZombie(zombieRect, displayedProgress, elapsed);
            yield return null;
        }

        if (progressBar != null)
        {
            progressBar.value = progressBar.maxValue;
        }

        UpdateLoadingZombie(zombieRect, progressBar != null ? progressBar.normalizedValue : 1f, elapsed);
        operation.allowSceneActivation = true;
    }

    private void UpdateLoadingZombie(RectTransform zombieRect, float progress, float elapsed)
    {
        if (progressBar == null || zombieRect == null)
        {
            return;
        }

        RectTransform barRect = progressBar.GetComponent<RectTransform>();
        float zombieWidth = loadingZombie.sprite != null && loadingZombie.sprite.rect.height > 0f
            ? Mathf.Min(
                zombieRect.rect.width,
                zombieRect.rect.height * loadingZombie.sprite.rect.width / loadingZombie.sprite.rect.height)
            : zombieRect.rect.width;
        float fillEndX = barRect.rect.xMin + barRect.rect.width * progress;
        float x = fillEndX - zombieWidth * 0.5f;
        float bob = Mathf.Abs(Mathf.Sin(elapsed * 18f)) * 5f;
        float y = barRect.rect.height * 0.5f - 2f + bob;
        float tilt = Mathf.Sin(elapsed * 18f) * 6f;

        zombieRect.anchoredPosition = new Vector2(x, y);
        zombieRect.localRotation = Quaternion.Euler(0f, 0f, tilt);
    }
}
