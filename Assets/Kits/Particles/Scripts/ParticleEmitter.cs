using UnityEngine;

public class ParticleEmitter : MonoBehaviour
{
    //----------- UNITY EDITOR -----------//
    [SerializeField] private ParticleSystem particleSystemReference;

    //----------- PUBLIC METHODS -----------//
    public void Play()
    {
        particleSystemReference.Play();
    }

}
