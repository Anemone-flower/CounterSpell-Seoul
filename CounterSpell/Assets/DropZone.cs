using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    private RectTransform dropZoneRect;

    private void Start()
    {
        // DropZone의 RectTransform을 가져옵니다.
        dropZoneRect = GetComponent<RectTransform>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject draggedBlock = eventData.pointerDrag; // 드래그한 블록
        RectTransform draggedBlockRect = draggedBlock.GetComponent<RectTransform>();

        // 드래그된 블록이 DropZone에 놓였는지 확인
        if (RectTransformUtility.RectangleContainsScreenPoint(dropZoneRect, eventData.position))
        {
            // DropZone에 드래그된 블록을 추가
            draggedBlock.transform.SetParent(transform);
            draggedBlock.transform.position = eventData.position; // 블록 위치를 드롭된 위치로 설정
        }
        else
        {
            // DropZone 외부에 드래그된 경우 원래 위치로 돌아가게 설정
            draggedBlock.transform.SetParent(draggedBlock.GetComponentInParent<Canvas>().transform);
            draggedBlock.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
    }
}
