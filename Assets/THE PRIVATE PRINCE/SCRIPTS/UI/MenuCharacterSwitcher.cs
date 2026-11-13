using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuCharacterSwitcher : MonoBehaviour
{
    [Serializable]
    public class Entry
    {
        public Selectable button;
        public GameObject character;
    }

    [SerializeField] private List<Entry> entries = new List<Entry>();
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private Vector2 slideOffset = new Vector2(80f, 0f);
    [SerializeField] private bool showFirstOnEnable = true;
    [SerializeField] private bool hideWhenNothingSelected;

    private readonly Dictionary<GameObject, CanvasGroup> groups = new Dictionary<GameObject, CanvasGroup>();
    private readonly Dictionary<GameObject, Vector2> basePositions = new Dictionary<GameObject, Vector2>();
    private Entry current;

    private void Awake()
    {
        foreach (Entry entry in entries)
        {
            if (entry.button == null || entry.character == null)
            {
                continue;
            }

            GameObject character = entry.character;
            CanvasGroup group = character.GetComponent<CanvasGroup>();

            if (group == null)
            {
                group = character.AddComponent<CanvasGroup>();
            }

            groups[character] = group;

            RectTransform rect = character.transform as RectTransform;

            if (rect != null)
            {
                basePositions[character] = rect.anchoredPosition;
            }

            Entry captured = entry;
            AddTrigger(entry.button, EventTriggerType.Select, _ => Show(captured));
            AddTrigger(entry.button, EventTriggerType.PointerEnter, _ => Show(captured));
            AddTrigger(entry.button, EventTriggerType.Deselect, _ => OnButtonDeselected());
        }
    }

    private void OnEnable()
    {
        current = null;

        foreach (Entry entry in entries)
        {
            if (entry.character != null && groups.ContainsKey(entry.character))
            {
                SetState(entry.character, false);
            }
        }

        if (showFirstOnEnable && entries.Count > 0 && entries[0].character != null)
        {
            current = entries[0];
            SetState(current.character, true);
        }
    }

    private void OnDisable()
    {
        foreach (Entry entry in entries)
        {
            if (entry.character != null && groups.ContainsKey(entry.character))
            {
                Kill(entry.character);
            }
        }
    }

    private static void AddTrigger(Selectable selectable, EventTriggerType type, UnityAction<BaseEventData> action)
    {
        EventTrigger trigger = selectable.GetComponent<EventTrigger>();

        if (trigger == null)
        {
            trigger = selectable.gameObject.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry triggerEntry = new EventTrigger.Entry { eventID = type };
        triggerEntry.callback.AddListener(action);
        trigger.triggers.Add(triggerEntry);
    }

    private void Show(Entry entry)
    {
        if (entry == current || entry.character == null)
        {
            return;
        }

        if (current != null)
        {
            HideCharacter(current.character);
        }

        current = entry;
        ShowCharacter(entry.character);
    }

    private void ShowCharacter(GameObject character)
    {
        CanvasGroup group = groups[character];
        Kill(character);
        character.SetActive(true);
        group.alpha = 0f;

        RectTransform rect = character.transform as RectTransform;

        if (rect != null)
        {
            Vector2 basePosition = basePositions[character];
            rect.anchoredPosition = basePosition + slideOffset;
            rect.DOAnchorPos(basePosition, fadeDuration).SetEase(Ease.OutCubic).SetUpdate(true);
        }

        group.DOFade(1f, fadeDuration).SetUpdate(true);
    }

    private void HideCharacter(GameObject character)
    {
        CanvasGroup group = groups[character];
        Kill(character);

        RectTransform rect = character.transform as RectTransform;

        if (rect != null)
        {
            rect.DOAnchorPos(basePositions[character] + slideOffset, fadeDuration).SetEase(Ease.InCubic).SetUpdate(true);
        }

        group.DOFade(0f, fadeDuration).SetUpdate(true).OnComplete(() =>
        {
            if (current == null || current.character != character)
            {
                character.SetActive(false);
            }
        });
    }

    private void SetState(GameObject character, bool visible)
    {
        Kill(character);
        groups[character].alpha = visible ? 1f : 0f;

        RectTransform rect = character.transform as RectTransform;

        if (rect != null)
        {
            rect.anchoredPosition = basePositions[character];
        }

        character.SetActive(visible);
    }

    private void Kill(GameObject character)
    {
        groups[character].DOKill();

        RectTransform rect = character.transform as RectTransform;

        if (rect != null)
        {
            rect.DOKill();
        }
    }

    private void OnButtonDeselected()
    {
        if (hideWhenNothingSelected)
        {
            StartCoroutine(HideIfNothingSelected());
        }
    }

    private IEnumerator HideIfNothingSelected()
    {
        yield return null;

        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null && current != null)
        {
            HideCharacter(current.character);
            current = null;
        }
    }
}