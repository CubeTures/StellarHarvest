using UnityEngine;

public abstract class TutorialStep : MonoBehaviour
{
    public abstract void Begin();
    public virtual void Check() { }
    public abstract void End();
}
