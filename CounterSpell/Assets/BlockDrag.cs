using UnityEngine;
using UnityEngine.EventSystems;

public class DragAndDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 originalPosition; // 원래 위치를 저장
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 드래그 시작 시 원래 위치 저장
        originalPosition = rectTransform.anchoredPosition;

        // 드래그 시작 시 반투명하게 표시
        canvasGroup.alpha = 0.6f; 
        canvasGroup.blocksRaycasts = false; // Raycast 비활성화
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 드래그 중에 블록 이동
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 드래그 끝나면 원래 상태로 돌아옴
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true; // Raycast 활성화

        // 드래그가 끝난 후, 블록이 DropZone 내에 있는지 확인
        DropZone dropZone = eventData.pointerEnter?.GetComponent<DropZone>();
        if (dropZone != null && dropZone.IsPointerInDropZone(eventData.position))
        {
            // DropZone에 블록을 놓음
            rectTransform.anchoredPosition = eventData.position; // Drop된 위치로 이동
        }
        else
        {
            // DropZone 외부에 놓은 경우 원래 위치로 복귀
            rectTransform.anchoredPosition = originalPosition;
        }
    }
}
