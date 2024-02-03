using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Component that tracks an input and makes it available to the NetworkClient.
/// The client sends data to the server at a lower frequency than the Unity update loop.
/// </summary>
public abstract class InputTracker : MonoBehaviour
{
    /// <summary>
    /// Gather the input that occurred during the current client update cycle.
    /// </summary>
    /// <param name="state">Serializable object that contains the client state for the current client update cycle.</param>
    public abstract void UpdateClientState(ref ClientState state);

    /// <summary>
    /// Signal the end of an client update cycle.
    /// Use to reset the internal state.
    /// </summary>
    public abstract void OnEndFrame();
}
