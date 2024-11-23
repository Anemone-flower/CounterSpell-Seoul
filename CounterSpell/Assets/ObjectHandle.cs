using UnityEngine;

public class ObjectHandle : MonoBehaviour
{
    public float zoomSpeed = 2f;        // 줌 속도
    public float targetOrthographicSize = 3f; // 목표 카메라 확대 크기
    public Canvas uiCanvas;            // UI Canvas (활성화할 UI)
    private Camera mainCamera; // mainCamera
    private Vector3 originalPosition;  // 카메라의 초기 위치
    private float originalSize;        // 카메라의 초기 크기
    private bool isZooming = false;    // 줌 상태 플래그

    void Start()
    {
        mainCamera = Camera.main;
        originalPosition = mainCamera.transform.position;
        originalSize = mainCamera.orthographicSize;

        if (uiCanvas != null)
        {
            uiCanvas.gameObject.SetActive(false); // UI 비활성화
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 마우스 왼쪽 클릭
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

            if (hit.collider != null)
            {   
                if(hit.collider.gameObject.tag == "Mob")
                {
                    StartZoom(hit.collider.transform.position); // 클릭한 몬스터의 위치로 줌
                }
            }
        }
    }

    void StartZoom(Vector3 targetPosition)
    {
        if (isZooming) return;

        isZooming = true;

        // 줌 및 UI 활성화 코루틴 실행
        StartCoroutine(ZoomToTarget(targetPosition));
    }

    System.Collections.IEnumerator ZoomToTarget(Vector3 targetPosition)
    {
        Vector3 startPosition = mainCamera.transform.position;
        float startSize = mainCamera.orthographicSize;

        Vector3 endPosition = new Vector3(targetPosition.x, targetPosition.y, startPosition.z);
        float endSize = targetOrthographicSize;

        float elapsed = 0f;
        float duration = 1f; // 줌 애니메이션 시간

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // 위치와 크기 보간
            mainCamera.transform.position = Vector3.Lerp(startPosition, endPosition, elapsed / duration);
            mainCamera.orthographicSize = Mathf.Lerp(startSize, endSize, elapsed / duration);

            yield return null;
        }

        // 최종 값 설정
        mainCamera.transform.position = endPosition;
        mainCamera.orthographicSize = endSize;

        if (uiCanvas != null)
        {
            uiCanvas.gameObject.SetActive(true); // UI 활성화
        }

        isZooming = false;
    }
}
