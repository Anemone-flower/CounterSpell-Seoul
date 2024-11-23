using UnityEngine;
using UnityEngine.UI;

public class ObjectHandle : MonoBehaviour
{
    public float zoomSpeed = 1.5f;        // 줌 속도
    public float targetOrthographicSize = 3f; // 목표 카메라 확대 크기
    public Canvas uiCanvas;            // UI Canvas (활성화할 UI)
    public RawImage uiRawImage;        // RawImage UI 컴포넌트
    public Texture playerTexture;      // 플레이어 이미지 (Texture)
    public Texture mobTexture;         // 몬스터 이미지 (Texture)
    private Camera mainCamera;         // mainCamera
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
                // 클릭한 객체가 "Mob" 또는 "Player" 태그일 때
                if(hit.collider.gameObject.CompareTag("Mob") || hit.collider.gameObject.CompareTag("Player"))
                {
                    StartZoom(hit.collider.transform.position, hit.collider.gameObject); // 클릭한 객체로 줌 및 이미지 변경
                }
            }
        }

        // ESC 키 감지
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StartZoomOut();
        }
    }

    void StartZoom(Vector3 targetPosition, GameObject clickedObject)
    {
        if (isZooming) return;

        isZooming = true;

        // 줌 및 UI 활성화 코루틴 실행
        StartCoroutine(ZoomToTarget(targetPosition, clickedObject));
    }

    void StartZoomOut()
    {
        if (!isZooming)
        {
            StartCoroutine(ZoomOutToOriginal());
        }
    }

    System.Collections.IEnumerator ZoomToTarget(Vector3 targetPosition, GameObject clickedObject)
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

        // 클릭한 객체에 따라 UI RawImage의 Texture 변경
        if (clickedObject.CompareTag("Player"))
        {
            uiRawImage.texture = playerTexture; // 플레이어 Texture로 변경
        }
        else if (clickedObject.CompareTag("Mob"))
        {
            uiRawImage.texture = mobTexture; // 몬스터 Texture로 변경
        }

        if (uiCanvas != null)
        {
            uiCanvas.gameObject.SetActive(true); // UI 활성화
        }

        isZooming = false;
    }

    System.Collections.IEnumerator ZoomOutToOriginal()
    {
        Vector3 startPosition = mainCamera.transform.position;
        float startSize = mainCamera.orthographicSize;

        Vector3 endPosition = originalPosition;
        float endSize = originalSize;

        float elapsed = 0f;
        float duration = 1f; // 줌 애니메이션 시간

        isZooming = true;

        if (uiCanvas != null)
        {
            uiCanvas.gameObject.SetActive(false); // UI 비활성화
        }

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



        isZooming = false;
    }
}
