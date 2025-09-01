using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TutorialTextPopup : MonoBehaviour
{
    public TextMeshProUGUI text;

    private void Start()
    {
        if (text != null)
            text.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (text == null) return;
        if (other.CompareTag("Player"))
            text.gameObject.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (text == null) return;
        if (other.CompareTag("Player"))
            text.gameObject.SetActive(false);
    }
}
