using UnityEngine;

public class AnswerButton : MonoBehaviour
{
    public bool isCorrect;

    public void OnButtonClicked()
    {
        Debug.Log($"{gameObject.name} was clicked!");

        
    }
}