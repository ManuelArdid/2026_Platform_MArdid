using UnityEngine;

public class PlayerJumpParticleEmitter : ParticleEmitter
{
    //----------- UNITY EDITOR -----------//
    [SerializeField] GameObject playerReference;
    [SerializeField] float XOffset = 0.3f;
    [SerializeField] float YOffset = -0.3f;

    //----------- UNITY METHODS -----------//
    void Update()
    {
        var direction = playerReference.GetComponent<Player>().PlayerGetLastDirection();

        float sign = direction.x >= 0 ? -1f : 1f;

        transform.position = playerReference.transform.position + new Vector3(XOffset * sign, YOffset, 0f);
    }

    void OnEnable()
    {
        Player.OnPlayerJump += HandleOnPlayerJump;
    }

    void OnDisable()
    {
        Player.OnPlayerJump -= HandleOnPlayerJump;
    }

    //----------- HANDLER METHODS -----------//
    private void HandleOnPlayerJump()
    {
        Play();
    }
}