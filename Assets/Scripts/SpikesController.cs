using UnityEngine;
using DG.Tweening;

public class SpikesController : MonoBehaviour
{
    [SerializeField] private GameObject StartPoint;
    [SerializeField] private GameObject EndPoint;
    [SerializeField] private float duration = 2f;
    [SerializeField] private Ease ease = Ease.Linear;
    [SerializeField] private bool useLocalSpace = false;
    [SerializeField] private bool startAtStartPoint = true;

    private Tweener tweener;
    private Vector3 lastPosition;

    private void Awake()
    {
        if (StartPoint == null || EndPoint == null)
        {
            Debug.LogError("SpikesController: Assign StartPoint and EndPoint in the Inspector.", this);
            enabled = false;
            return;
        }

        if (startAtStartPoint)
        {
            if (useLocalSpace)
                transform.localPosition = StartPoint.transform.localPosition;
            else
                transform.position = StartPoint.transform.position;
        }

        lastPosition = useLocalSpace ? transform.localPosition : transform.position;
    }

    private void Start()
    {
        Vector3 target = useLocalSpace
            ? EndPoint.transform.localPosition
            : EndPoint.transform.position;

        if (useLocalSpace)
        {
            tweener = transform
                .DOLocalMove(target, duration)
                .SetEase(ease)
                .SetLoops(-1, LoopType.Yoyo)
                .OnUpdate(UpdateYAxisScale);
        }
        else
        {
            tweener = transform
                .DOMove(target, duration)
                .SetEase(ease)
                .SetLoops(-1, LoopType.Yoyo)
                .OnUpdate(UpdateYAxisScale);
        }
    }

    private void UpdateYAxisScale()
    {
        Vector3 currentPosition = useLocalSpace ? transform.localPosition : transform.position;
        Vector3 worldDelta = currentPosition - lastPosition;

        // Convert movement into LOCAL space (this fixes parent rotation issues)
        Vector3 localDelta = transform.InverseTransformDirection(worldDelta);

        // Start -> End
        if (localDelta.y > 0.001f)
            SetYScale(1f);
        // End -> Start
        else if (localDelta.y < -0.001f)
            SetYScale(-1f);

        lastPosition = currentPosition;
    }

    private void SetYScale(float yValue)
    {
        Vector3 scale = transform.localScale;
        scale.y = Mathf.Abs(scale.y) * Mathf.Sign(yValue);
        transform.localScale = scale;
    }

    private void OnDisable()
    {
        if (tweener != null && tweener.IsActive())
            tweener.Kill();
    }
}
