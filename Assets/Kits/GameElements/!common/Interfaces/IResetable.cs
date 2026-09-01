using UnityEngine;

public interface IResetable
{
    public Vector3 OriginalPosition { get; set; }

    public void Reset();
}