using UnityEngine;

public class ConcreteActivable: MonoBehaviour, IActivable
{
    public bool IsActivated { get; set; }

    public virtual void Activate()
    {
        IsActivated = true;
    }

    public virtual void Deactivate()
    {
        IsActivated = false;
    }
}