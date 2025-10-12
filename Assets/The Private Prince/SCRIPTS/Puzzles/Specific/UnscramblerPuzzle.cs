using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class UnscramblerPuzzle : PuzzleBase
{
    [Header("Unscrambler Puzzle Settings")]
    [TextArea] public string sentence; // target sentence to unscramble
    public int guessLimit = 0; // 0 means no limit
    public int glitchCount = 0;

    [Header("Unscrambler UI References")]
    public Transform blanksParent;
    public Transform wordBankParent;
    public GameObject wordButtonPrefab;
    public GameObject blankPrefab;
    public Button submitButton;
    public TextMeshProUGUI guessText;

    private string[] targetWords;
    private List<string> availableWords = new List<string>();
    private List<string> currentBlanks = new List<string>();
    private List<GameObject> blankObjects = new List<GameObject>();
    private List<GameObject> wordButtonObjects = new List<GameObject>();
    private List<GameObject> blankToButton = new List<GameObject>();
    private int guessCounter = 0;

    public override void StartPuzzle()
    {
        base.StartPuzzle();

        targetWords = sentence.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);

        ClearChildren(wordBankParent);
        ClearChildren(blanksParent);

        SetupWords();
        SetupBlanks();
        UpdateSubmitButton();
        UpdateGuessUI();

        if (submitButton != null)
        {
            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(SubmitGuess);
        }
    }

    private void SetupWords()
    {
        availableWords.Clear();
        wordButtonObjects.Clear();
        availableWords.AddRange(targetWords);

        if (glitchCount > 0)
            AddGlitchWords();

        availableWords = ShuffleList(availableWords);

        foreach (var w in availableWords)
        {
            GameObject buttonObj = Instantiate(wordButtonPrefab, wordBankParent);
            TextMeshProUGUI label = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = w;

            Button btnComp = buttonObj.GetComponent<Button>();
            string word = w; // Capture for listener
            btnComp.onClick.AddListener(() => SelectWord(word, buttonObj));

            wordButtonObjects.Add(buttonObj);
        }
    }

    private void SetupBlanks()
    {
        blankObjects.Clear();
        blankToButton.Clear();
        currentBlanks.Clear();

        for (int i = 0; i < targetWords.Length; i++)
        {
            GameObject blankObj = Instantiate(blankPrefab, blanksParent);
            blankObjects.Add(blankObj);

            blankToButton.Add(null);
            currentBlanks.Add(null);

            Transform removeBtnT = blankObj.transform.Find("RemoveButton");
            if (removeBtnT != null)
            {
                Button removeBtn = removeBtnT.GetComponent<Button>();
                int index = i; // Capture for listener
                removeBtn.onClick.RemoveAllListeners();
                removeBtn.onClick.AddListener(() => DeselectWord(index));
                removeBtn.gameObject.SetActive(false);
            }
        }

        UpdateBlanksUI();
    }

    public void SelectWord(string word, GameObject button)
    {
        int index = currentBlanks.FindIndex(b => b == null);
        if (index == -1) return; // No empty blanks

        currentBlanks[index] = word;
        
        if (button != null)
            button.SetActive(false);

        blankToButton[index] = button;

        UpdateBlanksUI();
        UpdateSubmitButton();
    }

    public void DeselectWord(int index)
    {
        if (index < 0 || index >= currentBlanks.Count) return;
        if (currentBlanks[index] == null) return;

        // Reactivate original word button
        GameObject sourceButton = blankToButton[index];
        if (sourceButton != null)
        {
            sourceButton.SetActive(true);
            blankToButton[index] = null;
        }
        else
        {
            // Safety check in case the button reference was lost
            string word = currentBlanks[index];
            foreach (Transform child in wordBankParent)
            {
                var lab = child.GetComponentInChildren<TextMeshProUGUI>();
                if (lab != null && lab.text == word && !child.gameObject.activeSelf)
                {
                    child.gameObject.SetActive(true);
                    break;
                }
            }
        }

        currentBlanks[index] = null;
        UpdateBlanksUI();
        UpdateSubmitButton();
    }

    public void SubmitGuess()
    {
        if (currentBlanks.Contains(null)) return;

        bool correct = CheckSentenceCorrect();

        if (correct)
        {
            PuzzleManager.Instance.EndPuzzle(PuzzleResult.Solved);
        }
        else
        {
            guessCounter++;
            if (guessLimit > 0 && guessCounter >= guessLimit)
                PuzzleManager.Instance.EndPuzzle(PuzzleResult.Failed);
            else
                RetryPuzzle();
        }
    }

    private bool CheckSentenceCorrect()
    {
        if (currentBlanks.Count != targetWords.Length) return false;

        for (int i = 0; i < targetWords.Length; i++)
        {
            if (!string.Equals(currentBlanks[i], targetWords[i], System.StringComparison.Ordinal))
                return false;
        }
        return true;
    }

    private void RetryPuzzle()
    {
        for (int i = 0; i < currentBlanks.Count; i++)
        {
            currentBlanks[i] = null;
            if (i < blankToButton.Count)
                blankToButton[i] = null;
        }

        foreach (var btn in wordButtonObjects)
        {
            if (btn != null)
                btn.SetActive(true);
        }

        UpdateBlanksUI();
        UpdateSubmitButton();
        UpdateGuessUI();
    }

    private void UpdateBlanksUI()
    {
        for (int i = 0; i < blankObjects.Count; i++)
        {
            var blankObj = blankObjects[i];
            if (blankObj == null) continue;

            var label = blankObj.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                label.text = currentBlanks[i] ?? "";
            }

            Transform removeBtnT = blankObj.transform.Find("RemoveButton");
            if (removeBtnT != null)
            {
                removeBtnT.gameObject.SetActive(currentBlanks[i] != null);
            }
        }
    }

    private void UpdateSubmitButton()
    {
        if (submitButton == null) return;
        submitButton.gameObject.SetActive(!currentBlanks.Contains(null));
    }

    private void UpdateGuessUI()
    {
        if (guessText == null) return;
        if (guessLimit > 0)
        {
            guessText.gameObject.SetActive(true);
            guessText.text = $"Guesses: {guessCounter}/{guessLimit}";
        }
        else
        {
            guessText.gameObject.SetActive(false);
        }
    }

    private List<string> ShuffleList(List<string> list)
    {
        List<string> shuffled = new List<string>(list);
        for (int i = 0; i < shuffled.Count; i++)
        {
            int rand = Random.Range(i, shuffled.Count);
            (shuffled[i], shuffled[rand]) = (shuffled[rand], shuffled[i]);
        }
        return shuffled;
    }

    private void AddGlitchWords()
    {
        int minLen = int.MaxValue, maxLen = 0;
        foreach (var word in targetWords)
        {
            minLen = Mathf.Min(minLen, word.Length);
            maxLen = Mathf.Max(maxLen, word.Length);
        }

        HashSet<string> unique = new HashSet<string>(availableWords);
        int added = 0;
        while (added < glitchCount)
        {
            int len = Random.Range(minLen, maxLen + 1);
            string glitch = GenerateRandomString(len);

            if (!unique.Contains(glitch))
            {
                unique.Add(glitch);
                availableWords.Add(glitch);
                added++;
            }
        }
    }

    private string GenerateRandomString(int length)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz";
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < length; i++)
            sb.Append(chars[Random.Range(0, chars.Length)]);
        return sb.ToString();
    }

    private void ClearChildren(Transform parent)
    {
        if (parent == null) return;
        for (int i = parent.childCount - 1; i >= 0; i--)
            Destroy(parent.GetChild(i).gameObject);
    }

    public override void HandleInput()
    {
        
    }
}
