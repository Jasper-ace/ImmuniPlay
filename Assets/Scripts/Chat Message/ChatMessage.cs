using System;
using UnityEngine;

[Serializable]
public class ChatMessage
{
    [TextArea(2, 5)]
    public string message;
}