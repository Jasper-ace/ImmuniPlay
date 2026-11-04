using UnityEngine;
using System.Collections;

public abstract class TransitionBase : MonoBehaviour
{
    public abstract IEnumerator Play();
}