using System.Collections.Generic;
using UnityEngine;

public class PlayerResetParticleEmitter : ParticleEmitter
{
    [SerializeField] private ParticleSystem explosionPrefab;

    private Vector3 lastPosition;
    private readonly List<GameObject> activeExplosions = new();

    void Start()
    {
        lastPosition = transform.position;
    }

    void OnEnable()
    {
        Player.OnPlayerReset += HandleOnPlayerReset;
    }

    void OnDisable()
    {
        Player.OnPlayerReset -= HandleOnPlayerReset;
        ClearExplosions();
    }

    void LateUpdate()
    {
        lastPosition = transform.position;
    }

    private void HandleOnPlayerReset()
    {
        ClearExplosions();

        SpawnExplosion(lastPosition);
        Play();
    }

    private void SpawnExplosion(Vector3 position)
    {
        ParticleSystem ps = Instantiate(explosionPrefab, position, Quaternion.identity);
        ps.Play();

        activeExplosions.Add(ps.gameObject);

        Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
    }

    private void ClearExplosions()
    {
        for (int i = 0; i < activeExplosions.Count; i++)
        {
            if (activeExplosions[i] != null)
                Destroy(activeExplosions[i]);
        }

        activeExplosions.Clear();
    }
}