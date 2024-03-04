using System.Collections;
using UnityEngine;

/// <summary>
/// Component that acts as a container for coroutines for usage by non-MonoBehavior objects.
/// See: https://docs.unity3d.com/Manual/Coroutines.html
/// 
/// Note that the coroutine functions using a string parameter cannot be used with this object.
/// They require the emitter to be a MonoBehaviour because it uses reflection tricks under the hood.
/// Use the newer IEnumerator-based functions instead.
/// </summary>
public class CoroutineContainer : MonoBehaviour
{
    public static CoroutineContainer Create(string name)
    {
        return new GameObject(name).AddComponent<CoroutineContainer>();
    }

    // The string 'methodName' argument requires the target method to be within a MonoBehavior object.
    // Use the newer StartCoroutine(IEnumerator) instead.
    new public void CancelInvoke()
    {
        throw new UnityException("Cannot use CancelInvoke(). Use coroutines instead.");
    }
    new public void CancelInvoke(string methodName)
    {
        throw new UnityException("Cannot use CancelInvoke(). Use coroutines instead.");
    }
    new public void Invoke(string methodName, float time)
    {
        throw new UnityException("Cannot use Invoke(). Use StartCoroutine() instead.");
    }
    new public void InvokeRepeating(string methodName, float time, float repeatRate)
    {
        throw new UnityException("Cannot use InvokeRepeating(). Use StartCoroutine() instead.");
    }
    new public bool IsInvoking(string methodName)
    {
        throw new UnityException("Cannot use IsInvoking(). Use coroutines instead.");
    }
    new public Coroutine StartCoroutine(string methodName)
    {
        throw new UnityException("Cannot use StartCoroutine(string). Use StartCoroutine(IEnumerator) instead.");
    }
    new public Coroutine StartCoroutine(string methodName, [UnityEngine.Internal.DefaultValue("null")] object value)
    {
        throw new UnityException("Cannot use StartCoroutine(string). Use StartCoroutine(IEnumerator) instead.");
    }
    new public Coroutine StopCoroutine(string methodName)
    {
        throw new UnityException("Cannot use StopCoroutine(string). Use StopCoroutine(IEnumerator) instead.");
    }
}
