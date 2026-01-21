using System;
using UnityEngine;

public class Lilypad : MonoBehaviour
{

    //------ Events ------//
        public static event Action<Lilypad> OnLilypadCollected;

    //------- Unity Methods -------//
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            OnLilypadCollected?.Invoke(this);
            
            //Destroy lilypad when player lands on it
            Destroy(gameObject);
        }
    }
}
