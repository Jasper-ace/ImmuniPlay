using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ItemChecklist : MonoBehaviour
{
    [SerializeField] private Transform listContainer;  // the "Content" object
    [SerializeField] private TMP_Text counterText;     // the "Counter" text

    private readonly Dictionary<string, GameObject> checkMarks = new Dictionary<string, GameObject>();
    private int packedCount;

    void Awake()
    {
        foreach (Transform row in listContainer)
        {
            Transform checkedObj = row.Find("Checkbox/Checked");
            TMP_Text label = row.GetComponentInChildren<TMP_Text>();
            if (checkedObj == null || label == null) continue; // skips "Counter"

            checkMarks[Normalize(label.text)] = checkedObj.gameObject;
            checkedObj.gameObject.SetActive(false); // start unchecked
        }
        UpdateCounter();
    }

    public void Check(string itemName)
    {
        if (checkMarks.TryGetValue(Normalize(itemName), out GameObject mark) && !mark.activeSelf)
        {
            mark.SetActive(true);
            packedCount++;
            UpdateCounter();
        }
        else if (!checkMarks.ContainsKey(Normalize(itemName)))
        {
            Debug.LogWarning($"ItemChecklist: no row named '{itemName}'. Check the spelling.");
        }
    }

    private void UpdateCounter()
    {
        if (counterText != null)
            counterText.text = $"{packedCount} / {checkMarks.Count} packed";
    }

    private static string Normalize(string s) => s.Trim().ToLowerInvariant();
}