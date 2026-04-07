using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(SpriteRenderer))]
public class BallController : MonoBehaviour
{
    public float baseFallSpeed = 3f;
    public float moveToCornerSpeed = 10f;

    [HideInInspector] public BallColor ballColor;
    private float fallSpeed;

    private SpriteRenderer sr;
    private bool isMovingToCorner = false;
    private Vector3 targetPosition;
    private Corner targetCorner;

    public GameObject popEffect;

    private bool isPopping = false;   // ✅ safety flag

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void InitRandom(int colorCount)
    {
        int r = Random.Range(0, colorCount);
        ballColor = (BallColor)r;
        sr.sprite = GameManager.Instance.GetSprite(ballColor);
        UpdateSpeed();
    }

    public void Init(BallColor color)
    {
        ballColor = color;
        sr.sprite = GameManager.Instance.GetSprite(ballColor);
        UpdateSpeed();
    }

    private void UpdateSpeed()
    {
        if (GameManager.Instance.score < 40)
            fallSpeed = 3f;
        else if (GameManager.Instance.score < 100)
            fallSpeed = 6f;
        else
            fallSpeed = 10f;
    }

    private void Update()
    {
        if (isPopping) return; // ✅ stop movement during pop

        if (!isMovingToCorner)
        {
            transform.Translate(Vector2.down * fallSpeed * Time.deltaTime, Space.Self);

            float bottomLimit = Camera.main.transform.position.y - Camera.main.orthographicSize - 1f;
            if (transform.position.y < bottomLimit)
            {
                GameManager.Instance.OnBallMissed(this);
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveToCornerSpeed * Time.deltaTime);
            if (Vector3.SqrMagnitude(transform.position - targetPosition) < 0.0004f)
            {
                GameManager.Instance.OnBallArrived(this, targetCorner);
                // ✅ DO NOT destroy here anymore (pop handles it)
            }
        }
    }

    public void MoveToCorner(Transform cornerTransform, Corner corner, float speed = 10f)
    {
        if (cornerTransform == null) return;

        targetPosition = cornerTransform.position;
        targetCorner = corner;
        moveToCornerSpeed = speed;
        isMovingToCorner = true;
    }

    // ✅ ✅ ✅ CORRECT MATCH EFFECT (Soft Success Pop)
    public void SuccessPopAndDestroy()
    {
        if (isPopping) return;
        isPopping = true;

        //transform
        //    .DOScale(transform.localScale * 0.85f, 0.08f)
        //    .SetEase(Ease.InQuad)
        //    .OnComplete(() =>
        //    {
        //        transform
        //            .DOScale(transform.localScale * 1.2f, 0.12f)
        //            .SetEase(Ease.OutBack)
        //            .OnComplete(() =>
        //            {
        //                transform
        //                    .DOScale(Vector3.zero, 0.12f)
        //                    .SetEase(Ease.InBack)
        //                    .OnComplete(() =>
        //                    {
        //                        Destroy(gameObject);
        //                    });
        //            });
        //    });


        Destroy(gameObject);

        //actvate particle effect 
        //GameObject popEffect = GameManager.Instance.GetPopEffect();
    }

    



    // ❌ ❌ ❌ WRONG MATCH / MISS EFFECT (Balloon Pop)
    public void WrongPopAndDestroy()
    {
        if (isPopping) return;
        isPopping = true;

        transform
            .DOScale(transform.localScale * 1.5f, 0.12f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                transform
                    .DOScale(Vector3.zero, 0.08f)
                    .SetEase(Ease.InBack)
                    .OnComplete(() =>
                    {
                        Destroy(gameObject);
                    });
            });
    }
}
