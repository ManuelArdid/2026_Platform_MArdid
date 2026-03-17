using UnityEngine;

public interface IActivator
{
    public void ActivateAll();
    public void DeactivateAll();
    public void ActivateNext();
    public void DeactivatePrevious();
    public void ActivateRandom();
    public void DeactivateRandom();
}

