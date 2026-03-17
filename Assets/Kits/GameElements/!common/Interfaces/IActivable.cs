using UnityEngine;

public interface IActivable
{
    public bool IsActivated { get; set; }

    public void Activate();
    
    public void Deactivate();
}
