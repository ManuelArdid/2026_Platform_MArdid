using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ShadowEffect : MonoBehaviour
{
    [SerializeField] Vector3 Offset = new(-0.1f, -0.1f);
    [SerializeField] Material ShadowMaterial;
    [SerializeField] bool InFront = true;

    GameObject _shadowObject;
    SpriteRenderer _sr;
    SpriteRenderer _shadowSR;

    void Start()
    {
        _sr = GetComponent<SpriteRenderer>();

        _shadowObject = new GameObject("Shadow");
        _shadowObject.transform.parent = transform;
        _shadowObject.transform.localScale = Vector3.one;

        _shadowSR = _shadowObject.AddComponent<SpriteRenderer>();

        _shadowSR.sprite = _sr.sprite;
        _shadowSR.material = ShadowMaterial;

        if (InFront)
        {
            _shadowSR.sortingLayerID = _sr.sortingLayerID;
            _shadowSR.sortingOrder = _sr.sortingOrder - 1;
        }

        else
        {
            _shadowSR.sortingLayerName = "Background";
            _shadowSR.sortingOrder = 0;
        }
    }

    void LateUpdate()
    {
        _shadowObject.transform.localPosition = Offset;

        _shadowSR.sprite = _sr.sprite;
        _shadowSR.flipX = _sr.flipX;
        _shadowSR.flipY = _sr.flipY;

        _shadowSR.sprite = _sr.sprite;
    }
}