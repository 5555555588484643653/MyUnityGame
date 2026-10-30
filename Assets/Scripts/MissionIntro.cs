using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public sealed class MissionIntro : MonoBehaviour
{
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private CanvasGroup dialogueGroup;
    [SerializeField] private string nextScene = "nurzhaaaaaaaaaaaaan 17.10.206";
    [SerializeField, Min(0f)] private float fadeDuration = 0.45f;
    [SerializeField, Min(1f)] private float charactersPerSecond = 38f;

    private static readonly string[] DialogueLines =
    {
        "Зомбилер қаланы басып алды! WASD-пен қозғал да, тінтуірдің сол жақ батырмасымен шабуылда.",
        "Зомбилерді жеңсең, тәжірибе жинайсың. Жаңа деңгейге көтерілгенде, ұсынылған қабілеттердің бірін таңда.",
        "Бес толқыннан өт те, соңғы толқындағы босты жең. Q — күшті шабуыл. Боссты жеңсең, ойын аяқталады!"
    };

    private int currentLine;
    private bool canAdvance;
    private bool loading;
    private bool isTyping;
    private float fadeElapsed;
    private Coroutine typingCoroutine;

    private void Start()
    {
        if (dialogueText == null || dialogueGroup == null)
        {
            Debug.LogError("MissionIntro: диалог мәтіні немесе панель сілтемесі берілмеген.");
            enabled = false;
            return;
        }

        Time.timeScale = 1f;
        dialogueGroup.alpha = 0f;
        ShowCurrentLine();
    }

    private void Update()
    {
        if (loading)
        {
            return;
        }

        if (!canAdvance)
        {
            if (SkipInputsAreReleased())
            {
                canAdvance = true;
            }

            return;
        }

        if (dialogueGroup.alpha < 1f)
        {
            float duration = Mathf.Max(0f, fadeDuration);
            fadeElapsed += Time.unscaledDeltaTime;
            dialogueGroup.alpha = duration > 0f
                ? Mathf.Clamp01(fadeElapsed / duration)
                : 1f;
            if (dialogueGroup.alpha < 1f)
            {
                return;
            }
        }

        if (AdvanceInputPressed())
        {
            if (isTyping)
            {
                FinishTyping();
                return;
            }

            AdvanceDialogue();
        }
    }

    private void AdvanceDialogue()
    {
        currentLine++;
        if (currentLine >= DialogueLines.Length)
        {
            LoadNextScene();
            return;
        }

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        dialogueText.text = DialogueLines[currentLine];
        dialogueText.ForceMeshUpdate();
        dialogueText.maxVisibleCharacters = 0;
        isTyping = true;
        typingCoroutine = StartCoroutine(TypeCurrentLine());
    }

    private IEnumerator TypeCurrentLine()
    {
        float characterDelay = 1f / Mathf.Max(1f, charactersPerSecond);
        int characterCount = dialogueText.textInfo.characterCount;

        for (int visibleCharacters = 1; visibleCharacters <= characterCount; visibleCharacters++)
        {
            dialogueText.maxVisibleCharacters = visibleCharacters;
            yield return new WaitForSecondsRealtime(characterDelay);
        }

        isTyping = false;
        typingCoroutine = null;
    }

    private void FinishTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        dialogueText.maxVisibleCharacters = int.MaxValue;
        isTyping = false;
    }

    private static bool AdvanceInputPressed()
    {
        bool keyboardPressed = Keyboard.current != null &&
            Keyboard.current.anyKey.wasPressedThisFrame;
        bool mousePressed = Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame;
        return keyboardPressed || mousePressed;
    }

    private static bool SkipInputsAreReleased()
    {
        bool keyboardReleased = Keyboard.current == null || !Keyboard.current.anyKey.isPressed;
        bool mouseReleased = Mouse.current == null || !Mouse.current.leftButton.isPressed;
        return keyboardReleased && mouseReleased;
    }

    private void LoadNextScene()
    {
        if (loading)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(nextScene) ||
            !Application.CanStreamedLevelBeLoaded(nextScene))
        {
            Debug.LogError($"MissionIntro: '{nextScene}' сахнасы жүктеуге қолжетімсіз.");
            return;
        }

        loading = true;
        SceneManager.LoadScene(nextScene);
    }
}
