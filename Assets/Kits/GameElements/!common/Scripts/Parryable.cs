using System;
using UnityEngine;

public class Parryable : MonoBehaviour
{

    //------- EVENTS -------//
    public static event Action<Parryable> OnSuccessfulParry;

    void OnTriggerStay2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();
        Parry(player);
    }

    protected virtual void Parry(Player player)
    {
        if (player.PlayerIsParrying)
        {
            Debug.Log("Parry Successful!");
            OnSuccessfulParry?.Invoke(this);
        }
    }

}