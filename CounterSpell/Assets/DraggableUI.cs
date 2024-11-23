using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Transform canvas;
    private Transform previousParent;
    private RectTransform rect;
    private CanvasGroup canvasGroup;
    private RectTransform dropArea;

    private void Awake()
    {
        canvas = FindObjectOfType<Canvas>().transform; // 캔버스를 찾음
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        previousParent = transform.parent;

        transform.SetParent(canvas); // 드래그 시작 시 부모를 캔버스로 변경
        transform.SetAsLastSibling();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0.6f;
            canvasGroup.blocksRaycasts = false;
        }

        // 드래그 대상이 드롭 가능한 공간 내에 있을 때 그 영역을 설정
        if (previousParent != null)
        {
            dropArea = previousParent.GetComponent<RectTransform>(); // 드롭 영역을 설정
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 newPos = eventData.position;
        
        // 드롭 영역 경계를 넘지 않도록 제한
        if (dropArea != null)
        {
            Vector2 localPos = dropArea.InverseTransformPoint(newPos);
            localPos.x = Mathf.Clamp(localPos.x, 0, dropArea.rect.width);
            localPos.y = Mathf.Clamp(localPos.y, 0, dropArea.rect.height);
            newPos = dropArea.TransformPoint(localPos);
        }
        
        rect.position = newPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (transform.parent == canvas)
        {
            transform.SetParent(previousParent); // 원래 부모로 되돌림
            rect.position = previousParent.GetComponent<RectTransform>().position;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1.0f;
            canvasGroup.blocksRaycasts = true;
        }
    }
}
