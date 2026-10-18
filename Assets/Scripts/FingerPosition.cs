using UnityEngine;
using UnityEngine.EventSystems;

public class FingerPosition : MonoBehaviour
{
    [Tooltip("Optional: canvas this UI element belongs to. If null, the script will search in parents.")]
    [SerializeField] private Canvas uiCanvas;
    private RectTransform rectTransform;
    private RectTransform parentRect;

    // Allow using mouse in the Editor / standalone for testing
    [SerializeField] private bool allowMouseInEditor = true;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
            Debug.LogError("FingerPosition requires a RectTransform (must be a UI element).", this);

        if (uiCanvas == null)
            uiCanvas = GetComponentInParent<Canvas>();

        parentRect = rectTransform != null && rectTransform.parent != null
            ? rectTransform.parent as RectTransform
            : null;
    }

    void Update()
    {
        if (rectTransform == null) return;

        // Handle all touches (multitouch enabled)
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);

            if (t.phase == TouchPhase.Began)
            {
                // If the touch began over another UI element and you want to ignore it, uncomment the check below:
                // if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(t.fingerId)) continue;

                SetUIPositionToScreenPoint(t.position);
            }
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        // Optional: allow mouse click to simulate touch for testing
        if (allowMouseInEditor && Input.GetMouseButtonDown(0))
        {
            // ignore clicks over UI if desired:
            // if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            SetUIPositionToScreenPoint(Input.mousePosition);
        }
#endif
    }

    private void SetUIPositionToScreenPoint(Vector2 screenPoint)
    {
        Camera cam = (uiCanvas != null && uiCanvas.renderMode != RenderMode.ScreenSpaceOverlay) ? uiCanvas.worldCamera : null;
        RectTransform targetRect = parentRect != null ? parentRect : rectTransform;

        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(targetRect, screenPoint, cam, out localPoint))
        {
            // If anchored to parent, this sets anchoredPosition correctly.
            rectTransform.anchoredPosition = localPoint;
        }
        else
        {
            // Fallback: convert using camera
            Vector3 worldPos = cam != null ? cam.ScreenToWorldPoint(screenPoint) : (Vector3)screenPoint;
            rectTransform.position = new Vector3(worldPos.x, worldPos.y, rectTransform.position.z);
        }
    }
}