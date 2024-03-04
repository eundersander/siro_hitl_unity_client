/// <summary>
/// Object that can be updated.
/// </summary>
public interface IUpdatable
{
    /// <summary>
    /// Update the component.
    /// Called every frame from a MonoBehaviour Update().
    /// </summary>
    public abstract void Update();
}
