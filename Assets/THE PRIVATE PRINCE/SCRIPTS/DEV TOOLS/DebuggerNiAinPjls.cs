using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebuggerNiAinPjls : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    // Toggle to enable or disable debug logs for this component (can be set in the Inspector)
    [SerializeField] private bool enableLogs = true;

    // ----------------------- DEBUG METHODS -------------------------

    // Debug method to log messages with the GameObject's name as a prefix
    public void Log(string message)
    {
        if (!enableLogs) return;

        Debug.Log($"[{gameObject.name}] {message}");
    }

    // Debug method to log warning messages with the GameObject's name as a prefix          
    public void Warn(string message)
    {
        if (!enableLogs) return;

        Debug.LogWarning($"[{gameObject.name}] {message}");
    }

    // Debug method to log error messages with the GameObject's name as a prefix
    public void Error(string message)
    {
        if (!enableLogs) return;

        Debug.LogError($"[{gameObject.name}] {message}");
    }
}