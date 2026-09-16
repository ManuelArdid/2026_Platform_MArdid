using UnityEngine;


public class GeneralUtils : Singleton<GeneralUtils>
{

    //---------- COROUTINES ----------//

    /// <summary>
    /// Destroys the specified GameObject after a delay in real time.
    /// </summary>
    /// <param name="obj">The GameObject to be destroyed.</param>
    /// <param name="delay">The delay in seconds before destruction.</param>
    /// <returns>IEnumerator for coroutine.</returns>
    public System.Collections.IEnumerator DestroyAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        Destroy(obj);
    }

    /// <summary>
    /// Deactivates the specified GameObject after a delay in real time.
    /// </summary>
    /// <param name="obj">The GameObject to be deactivated.</param>
    /// <param name="delay">The delay in seconds before deactivation.</param>
    /// <returns>IEnumerator for coroutine.</returns>
    public System.Collections.IEnumerator DeactivateAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        obj.SetActive(false);
    }
}