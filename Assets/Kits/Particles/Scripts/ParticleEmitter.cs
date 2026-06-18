using UnityEngine;

public class ParticleEmitter : MonoBehaviour
{
    //----------- UNITY EDITOR -----------//
    [SerializeField] private ParticleSystem particleSystemReference;
    [SerializeField] private Sprite[] sprites;


    //----------- UNITY METHODS -----------//

    void Start()
    {
        ConfigureSprites();
    }

    //----------- PUBLIC METHODS -----------//
    public void Play()
    {
        particleSystemReference.Play();
    }

    //----------- PRIVATE METHODS -----------//
    private void ConfigureSprites()
    {
        var textureSheet = particleSystemReference.textureSheetAnimation;

        textureSheet.enabled = true;
        textureSheet.mode = ParticleSystemAnimationMode.Sprites;

        // Clean previous sprites
        textureSheet.RemoveSprite(0);

        // Add new sprites
        foreach (Sprite sprite in sprites)
        {
            textureSheet.AddSprite(sprite);
        }
    }
}
