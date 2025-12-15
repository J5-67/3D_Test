using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("설정 값")]
    public float moveSpeed = 5.0f;
    public float mouseSensitivity = 25.0f;

    [Header("점프 & 중력 설정")]
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("상태")]
    public bool isGround;
    public bool isJumping = false;
    public Vector3 currentVelocity;

    [Header("전투 설정")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 50f;

    [Header("시간 조종 설정")]
    public bool isTimeSlow = false;
    public float slowFactor = 0.1f;
    public float abilityGauge = 100f;
    [SerializeField] private Slider abilitySlider;

    [Header("카메라 연결")]
    public Transform cameraTransform;

    private CharacterController characterController;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float xRotation = 0f;
    private Vector3 velocity;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        TryGetComponent(out characterController);
    }

    void Update()
    {
        isGround = characterController.isGrounded;

        currentVelocity = velocity;

        if (Keyboard.current.tKey.wasPressedThisFrame && abilityGauge >= 0)
        {
            ToggleTime();
        }
        else if (abilityGauge <= 0)
        {
            ToggleTime();
        }

        if (isTimeSlow && abilityGauge >= 0)
        {
            abilityGauge -= 100 * Time.deltaTime;
            AbilityGaugeSlider();
        }
        else if (!isTimeSlow && abilityGauge <= 100)
        {
            abilityGauge += 10 * Time.deltaTime;
            AbilityGaugeSlider();
        }

        if (isGround && velocity.y < 0)
        {
            isJumping = false;
            velocity.y = -2f;
        }

        Vector3 finalMove = CalculateMove();
        CalculateGravity();

        Vector3 finalVelocity = finalMove + velocity;

        characterController.Move(finalVelocity * Time.unscaledDeltaTime);

        Look();
    }

    private void AbilityGaugeSlider()
    {
        abilitySlider.value = abilityGauge;
    }

    private void ToggleTime()
    {
        isTimeSlow = !isTimeSlow;
        if (isTimeSlow) { Time.timeScale = slowFactor; Time.fixedDeltaTime = 0.02f * Time.timeScale; }
        else { Time.timeScale = 1.0f; Time.fixedDeltaTime = 0.02f; }
    }

    private void CalculateGravity() { velocity.y += gravity * Time.unscaledDeltaTime; }

    private Vector3 CalculateMove()
    {
        if (characterController == null) return Vector3.zero;
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        return move * moveSpeed;
    }

    public void OnFire(InputValue value) { if (value.isPressed) Fire(); }

    private void Fire()
    {
        if (bulletPrefab == null || firePoint == null || cameraTransform == null) return;

        Vector3 targetPoint = GetAimTargetPoint();
        Vector3 fireDirection = (targetPoint - firePoint.position).normalized;

        Debug.DrawLine(firePoint.position, targetPoint, Color.red, 2.0f);

        GameObject newBullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        newBullet.transform.up = fireDirection;

        if (newBullet.TryGetComponent(out Rigidbody bulletRb))
        {
            bulletRb.linearVelocity = fireDirection * bulletSpeed;
        }
        Destroy(newBullet, 3.0f);
    }

    private Vector3 GetAimTargetPoint()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f)) return hit.point;
        else return ray.GetPoint(100f);
    }

    public void OnJump(InputValue value)
    {
        if (isGround && !isJumping)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            isJumping = true;
        }
    }

    public void OnMove(InputValue value) { moveInput = value.Get<Vector2>(); }
    public void OnLook(InputValue value) { lookInput = value.Get<Vector2>(); }

    private void Look()
    {
        if (cameraTransform == null) return;
        float mouseX = lookInput.x * mouseSensitivity * Time.unscaledDeltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.unscaledDeltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}