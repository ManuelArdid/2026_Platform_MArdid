using UnityEngine;

public class ParticleEmitter : MonoBehaviour
{
    //----------- UNITY EDITOR -----------//
    [SerializeField] protected ParticleSystem particleSystemReference;

    //----------- PUBLIC METHODS -----------//
    public void Play()
    {
        particleSystemReference.Play();
    }

}
