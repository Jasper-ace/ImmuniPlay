using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SceneEventSequence : MonoBehaviour
{
    [Header("Sound - Optional")]
    [SerializeField] private AudioClip sound;

    [Header("Transition")]
    [Tooltip("Extra delay in seconds after the sound finishes before starting the transition.")]
    [SerializeField] private float delayAfterSound = 0f;

    [SerializeField] private GameObject transitionTarget;
    [SerializeField] private TransitionBase transition;

    [Header("After Transition - Show")]
    [SerializeField] private List<GameObject> objectsToShow = new List<GameObject>();

    [Header("After Transition - Hide")]
    [SerializeField] private List<GameObject> objectsToHide = new List<GameObject>();

    private bool hasTriggered = false;

    private void OnEnable()
    {
        PlaySequence();
    }

    public void PlaySequence()
    {
        if (hasTriggered)
            return;

        hasTriggered = true;
        StartCoroutine(Sequence());
    }

    private IEnumerator Sequence()
    {
        // 1. Play sound through AudioManager (optional)
        // Wait for the sound to finish before starting the transition.
        if (sound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(sound);
            yield return new WaitForSeconds(sound.length);
        }

        // Extra delay after the sound has finished
        if (delayAfterSound > 0f)
        {
            yield return new WaitForSeconds(delayAfterSound);
        }

        // 2. Enable transition target
        if (transitionTarget != null)
        {
            transitionTarget.SetActive(true);
        }

        // 3. Play transition
        if (transition != null)
        {
            yield return StartCoroutine(transition.Play());
        }

        // 4. Show objects
        foreach (GameObject obj in objectsToShow)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        // 5. Hide objects
        foreach (GameObject obj in objectsToHide)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }
}