using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpringBoards : MonoBehaviour
{
    //-------------------UNITY EDITOR--------------------//
    
    [SerializeField] protected Sprite SpriteUsed;

    //-------------------CLASS VARIABLES--------------------//
    protected SpriteRenderer _spriteRenderer;
    //-------------------EVENTS--------------------//
    public static event System.Action OnSpringBoardUsed;

    //-------------------UNITY METHODS--------------------//
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnCollisionEnter(Collision collision)
    {
        _spriteRenderer.sprite = SpriteUsed;
        OnSpringBoardUsed?.Invoke();
    }
}
