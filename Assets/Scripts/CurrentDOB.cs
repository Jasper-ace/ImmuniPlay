using System;
using TMPro;
using UnityEngine;

public class CurrentDOB : MonoBehaviour
{
    [Header("Date of Birth Text")]
    public TMP_Text dobText;

    void Start()
    {
        // Display today's date in numeric format (MM/DD/YYYY)
        dobText.text = DateTime.Now.ToString("MM/dd/yyyy");
    }
}