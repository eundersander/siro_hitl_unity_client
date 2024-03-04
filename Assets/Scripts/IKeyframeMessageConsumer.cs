/// <summary>
/// Component that consumes keyframe messages to alter the client state.
/// </summary>
public interface IKeyframeMessageConsumer : IUpdatable
{
    /// <summary>
    /// Process a message.
    /// </summary>
    /// <param name="message">'Message' portion of a keyframe.</param>
    public void ProcessMessage(Message message);

    /// <summary>
    /// Process a message after the keyframe has been processed.
    /// Use for actions that must happen after the assets are loaded.
    /// </summary>
    /// <param name="message">'Message' portion of a keyframe.</param>
    public void PostProcessMessage(Message message) {}
}