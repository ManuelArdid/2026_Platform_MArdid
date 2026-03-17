using UnityEngine;

public class Activator : MonoBehaviour, IActivator
{
    //--------- UNITY EDITOR ---------//

    [SerializeField] Activable[] Activables;

    //--------- CLASS VARIABLES ---------//
    int _currentIndex = 0;


    /// <summary>
    /// Activates all activables in the array.
    /// </summary>
    public void ActivateAll()
    {
        foreach (var activable in Activables)
        {
            activable.Activate();
        }
    }

    /// <summary>
    /// Activates the next activable in the array, looping back to the start if necessary
    /// </summary>
    public void ActivateNext()
    {
        if (Activables.Length == 0) return;

        Activables[_currentIndex].Activate();
        _currentIndex = (_currentIndex + 1) % Activables.Length;
    }

    /// <summary> Activates a random activable from the array.
    /// </summary> <remarks> If the array is empty, the method does nothing.</remarks>
    public void ActivateRandom()
    {
        if (Activables.Length == 0) return;

        int randomIndex = Random.Range(0, Activables.Length);
        Activables[randomIndex].Activate();
    }

    /// <summary>
    /// Deactivates all activables in the array.
    /// </summary>  
    public void DeactivateAll()
    {
        foreach (var activable in Activables)
        {
            activable.Deactivate();
        }
    }

    /// <summary>
    /// Deactivates the previous activable in the array, looping back to the end if necessary.
    /// </summary> <remarks> If the array is empty, the method does nothing.</remarks>
    public void DeactivatePrevious()
    {
        if (Activables.Length == 0) return;

        _currentIndex = (_currentIndex - 1 + Activables.Length) % Activables.Length;
        Activables[_currentIndex].Deactivate();
    }

    /// <summary>
    /// Deactivates a random activable from the array.
    /// </summary> <remarks> If the array is empty, the method does nothing.</remarks>
    public void DeactivateRandom()
    {
        if (Activables.Length == 0) return;

        int randomIndex = Random.Range(0, Activables.Length);
        Activables[randomIndex].Deactivate();
    }
}