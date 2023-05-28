using System.Collections;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private int _x;

    [SerializeField] private int _y;

    [SerializeField] private SpriteRenderer _spriteRenderer;

    [SerializeField] private ParticleSystem _hitEffect;

    [SerializeField] private AudioSource _hitSource;

    public int type;
    
    public int X
    {
        get => _x;

        set
        {
            _x = value;

            var pos = transform.localPosition;
            pos.x = _x * _spriteRenderer.bounds.size.x;
            transform.localPosition = pos;
        }
    }
    public int Y
    {
        get => _y;

        set
        {
            _y = value;
            var pos = transform.localPosition;
            pos.y = _y * _spriteRenderer.bounds.size.y;
            transform.localPosition = pos;
        }
    }

    public void Hit()
    {
        _hitSource.Play();
        _hitEffect.Play();
        StartCoroutine(Shake());
    }

    private IEnumerator Shake()
    {
        var beginTime = Time.realtimeSinceStartup;
        var beginY = _spriteRenderer.transform.localPosition.y;
        var endTime = beginTime + 0.3f;

        var spriteTransform = _spriteRenderer.transform;

        while (Time.realtimeSinceStartup < endTime)
        {
            var diffTime = endTime - Time.realtimeSinceStartup;
            var range = 0.05f * diffTime;
            var pos = spriteTransform.localPosition;
            pos.y = beginY + Random.Range(-range, range);
            spriteTransform.localPosition = pos;
            yield return null;
        }

        {
            var pos = spriteTransform.localPosition;
            pos.y = beginY;
            spriteTransform.localPosition = pos;
        }

    }
}
