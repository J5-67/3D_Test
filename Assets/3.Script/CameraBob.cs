using UnityEngine;

public class CameraBob : MonoBehaviour
{
    public float bobSpeed = 14f;
    public float bobAmount = 0.05f;
    public PlayerController player;

    private float defaultPosY = 0;
    private float timer = 0;

    void Start()
    {
        defaultPosY = transform.localPosition.y;
    }

    void Update()
    {
        float speed = player.GetComponent<CharacterController>().velocity.magnitude;

        if (speed > 0.1f) // ∞»∞Ì ¿÷¥Ÿ∏È
        {
            timer += Time.deltaTime * bobSpeed;

            float newY = defaultPosY + Mathf.Sin(timer) * bobAmount;

            transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
        }
        else
        {
            timer = 0;
            float newY = Mathf.Lerp(transform.localPosition.y, defaultPosY, Time.deltaTime * bobSpeed);
            transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
        }
    }
}