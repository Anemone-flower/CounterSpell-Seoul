using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    private RectTransform dropZoneRect;
    private Canvas parentCanvas;

    private void Start()
    {
        dropZoneRect = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
    }

    // IsPointerInDropZone를 public으로 변경하여 외부에서 호출할 수 있도록 함
    public bool IsPointerInDropZone(Vector2 pointerPosition)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(dropZoneRect, pointerPosition, parentCanvas.worldCamera);
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject draggedBlock = eventData.pointerDrag;
        RectTransform draggedBlockRect = draggedBlock.GetComponent<RectTransform>();

        // DropZone 내에 놓인 경우 블록을 DropZone의 자식으로 설정
        if (IsPointerInDropZone(eventData.position))
        {
            draggedBlock.transform.SetParent(transform);
            draggedBlock.transform.position = eventData.position;
        }
        else
        {
            // DropZone 외부에 놓은 경우, 원래 위치로 복귀
            draggedBlock.transform.SetParent(parentCanvas.transform);
            draggedBlockRect.anchoredPosition = Vector2.zero;
        }
    }
}
