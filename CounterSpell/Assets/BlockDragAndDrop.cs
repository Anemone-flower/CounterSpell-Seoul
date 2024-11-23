using UnityEngine;
using UnityEngine.EventSystems;

public class BlockDragAndDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 originalPosition; // 원래 위치를 저장

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 드래그 시작 시 원래 위치 저장
        originalPosition = rectTransform.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 드래그 중에 블록 이동
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
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
