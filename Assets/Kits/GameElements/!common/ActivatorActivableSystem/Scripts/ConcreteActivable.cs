using UnityEngine;

public class Activable: MonoBehaviour, IActivable
{
    public bool IsActivated { get; set; }

    public void Activate()
    {
        IsActivated = true;
    }

    public void Deactivate()
    {
        IsActivated = false;
    }
}