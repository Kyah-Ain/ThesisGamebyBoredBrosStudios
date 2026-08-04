using TMPro;
using UnityEngine;

public class PuzzleUI : MonoBehaviour
{
    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private TextMeshProUGUI timerText;

    private void Awake()
    {
        if (rootCanvas == null)
            rootCanvas = GetComponent<Canvas>();

        Hide();
    }

    public void Show()
    {
        if (rootCanvas != null)
            rootCanvas.enabled = true;
    }

    public void Hide()
    {
        if (rootCanvas != null)
            rootCanvas.enabled = false;
    }

    public void UpdateTimer(float remainingTime)
    {
        if (timerText == null)
            return;

        timerText.text = Mathf.CeilToInt(remainingTime).ToString();
    }
}