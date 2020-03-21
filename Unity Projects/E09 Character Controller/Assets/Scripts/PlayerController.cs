using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 2;
    public float runSpeed = 6;
    public float glideSpeedMultMax = 2;
    public float glideDampMultMax = 10;
    public float gravity = -12;
    public float jumpHeight = 1;

    [Range(0, 1)]
    public float airControlPercent;

    public float turnSmoothTime = 0.2f;
    private float turnSmoothVelocity;

    public float speedSmoothTime = 0.1f;
    private float speedSmoothVelocity;
    private float currentSpeed;
    private float velocityY;

    private Animator animator;
    private Transform cameraT;
    private CharacterController character;
    private bool isGliding;
    private float glideDampening;
    private float glideSpeedMult;

    private void Start()
    {
        animator = GetComponent<Animator>();
        cameraT = Camera.main.transform;
        character = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // input
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector2 inputDir = input.normalized;

        bool running = Input.GetKey(KeyCode.LeftShift);

        UpdateCharacterMotion(inputDir, running);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
            isGliding = true;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            isGliding = false;
        }
        Glide();

        

        // animator
        float animationSpeedPercent = ((running) ? currentSpeed / runSpeed : currentSpeed / walkSpeed * .5f);
        animator.SetFloat("speedPercent", animationSpeedPercent, speedSmoothTime, Time.deltaTime);
    }

    private void UpdateCharacterMotion(Vector2 inputDir, bool running)
    {
        if (inputDir != Vector2.zero)
        {
            float targetRotation = Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg + cameraT.eulerAngles.y;
            transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, GetModifiedSmoothTime(turnSmoothTime));
        }

        float targetSpeed = ((running) ? runSpeed : walkSpeed) * inputDir.magnitude;
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, GetModifiedSmoothTime(speedSmoothTime));

        velocityY += Time.deltaTime * gravity * glideDampening;
        Vector3 velocity = transform.forward * currentSpeed * glideSpeedMult + Vector3.up * velocityY;
        character.Move(velocity * Time.deltaTime);

        if (character.isGrounded)
        {
            velocityY = 0;
        }
    }

    private void Jump()
    {
        if (character.isGrounded)
        {
            float jumpVelocity = Mathf.Sqrt(-10 * gravity * jumpHeight);
            velocityY = jumpVelocity;
        }
    }

    private void Glide()
    {
        if (!character.isGrounded && isGliding)
        {
            glideDampening =  1 / glideDampMultMax;
            glideSpeedMult = glideSpeedMultMax;
        } else
        {
            glideDampening = 1;
            glideSpeedMult = 1;
        }
    }

    private float GetModifiedSmoothTime(float smoothTime)
    {
        if (character.isGrounded)
        {
            return smoothTime;
        }

        if (airControlPercent == 0)
        {
            return float.MaxValue;
        }
        return smoothTime / airControlPercent;
    }
}