/// <summary>
/// Component that produces ClientState data.
/// </summary>
public interface IClientStateProducer
{
    /// <summary>
    /// Gather all inputs that occurred since the last OnEndFrame() call.
    /// </summary>
    /// <param name="state">Serializable object to update.</param>
    public abstract void UpdateClientState(ref ClientState state);

    /// <summary>
    /// Signals the end of a client update cycle.
    /// </summary>
    public abstract void OnEndFrame();
}
