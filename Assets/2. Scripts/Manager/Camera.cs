using System.Linq;
using UnityEngine;

public class Camera : MonoBehaviour
{
    public Transform target;
    public float rotationSpeed = 3.0f;
    public float distance = 3.0f;
    public float scrollSpeed = 3.0f;
    public float targetEye =5f;
    private float minDistance = 3;
    private float maxDistance = 25;
    private float currentRotationX = 0.0f;
    private float currentRotationY = 0.0f;

    public GameObject menuSet; 
    RaycastHit hit;

    private bool isCursorLocked = true;
    private bool isFirstPerson = false; // 1인칭 시점 여부를 나타내는 변수

    public GameObject[] targetObjects;

    static public Camera instance;

    private void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        FindAndSetPlayerTarget();
    }

private void Update()
{
    if (target != null)
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            isFirstPerson = !isFirstPerson; // R 키를 누르면 시점을 전환
        }

        if (isFirstPerson)
        {
            // 1인칭 시점
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

            currentRotationX -= mouseY;
            currentRotationX = Mathf.Clamp(currentRotationX, -90, 30); // 위아래 시야각 제한
            currentRotationY += mouseX;
            currentRotationY = Mathf.Clamp(currentRotationY, -90, 90); // 좌우 시야각 제한

            Quaternion rotation = Quaternion.Euler(currentRotationX, currentRotationY + target.eulerAngles.y, 0);
            Vector3 forwardOffset = target.forward * 0.7f; // 타겟의 앞으로 0.7 유닛 당김
            transform.position = target.position + new Vector3(0, targetEye, 0) + forwardOffset; // 카메라를 플레이어의 y축 targetEye 위치로 이동하고 약간 앞으로 당김
            transform.rotation = rotation;
        }
        else
        {
            // 3인칭 시점
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            distance = Mathf.Clamp(distance - scrollInput * scrollSpeed, minDistance, maxDistance);

            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

            currentRotationX -= mouseY;
            currentRotationX = Mathf.Clamp(currentRotationX, -80, 80);
            currentRotationY += mouseX;

            Quaternion rotation = Quaternion.Euler(currentRotationX, currentRotationY, 0);
            Vector3 offset = new Vector3(0, 0, -distance);
            Vector3 targetPosition = target.position + rotation * offset;

            Vector3 rayDirection = (targetPosition - target.position).normalized;
            Vector3 rayOrigin = target.position + Vector3.up * 0.5f;

            // "Ground" 레이어만 감지하도록 레이어 마스크 설정
            int layerMask = LayerMask.GetMask("Ground");

            if (Physics.Raycast(rayOrigin, rayDirection, out hit, distance, layerMask))
            {
                targetPosition = hit.point;
            }

            transform.position = targetPosition;
            transform.LookAt(target.position);
        }
    }

    MouseCursor();
}

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    void MouseCursor()
    {
        foreach (GameObject targetObject in targetObjects)
        {
            if (targetObject.activeInHierarchy && targetObject.CompareTag("OnUI"))
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                return;
            }
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void FindAndSetPlayerTarget()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            SetTarget(player.transform);
        }
    }

    public Vector3 GetMoveDirection(float horizontalInput, float verticalInput)
    {
        Vector3 cameraForward = transform.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();

        return cameraForward * verticalInput + transform.right * horizontalInput;
    }
}