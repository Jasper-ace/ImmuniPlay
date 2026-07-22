using UnityEngine;

public class ParentNavigator : MonoBehaviour
{
    [Header("All Parent Panels")]
    public GameObject[] parents;

    private int currentIndex = 0;

    void Start()
    {
        // Hide all panels
        foreach (GameObject panel in parents)
        {
            if (panel != null)
                panel.SetActive(false);
        }

        // Show the first panel
        if (parents.Length > 0 && parents[0] != null)
        {
            parents[0].SetActive(true);
            currentIndex = 0;
        }
    }

    // Go to the next parent
    public void NextParent()
    {
        if (currentIndex >= parents.Length - 1)
            return;

        parents[currentIndex].SetActive(false);

        currentIndex++;

        parents[currentIndex].SetActive(true);
    }

    // Go to the previous parent
    public void PreviousParent()
    {
        if (currentIndex <= 0)
            return;

        parents[currentIndex].SetActive(false);

        currentIndex--;

        parents[currentIndex].SetActive(true);
    }
public void RestartParents()
{
    foreach (GameObject panel in parents)
    {
        if (panel != null)
            panel.SetActive(false);
    }

    // Restart from the Great Work panel
    currentIndex = 2;

    if (parents[currentIndex] != null)
        parents[currentIndex].SetActive(true);
}

    // Show a specific parent by index
    public void ShowParent(int index)
    {
        if (index < 0 || index >= parents.Length)
            return;

        parents[currentIndex].SetActive(false);

        currentIndex = index;

        parents[currentIndex].SetActive(true);
    }
}