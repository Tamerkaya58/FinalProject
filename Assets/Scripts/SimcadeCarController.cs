using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimcadeCarController : MonoBehaviour
{
    [Header("=== Motor & Hız ===")]
    [SerializeField] private float maxMotorTorque = 2500f;
    [SerializeField] private float maxSpeed = 200f;

    [SerializeField]
    private AnimationCurve torqueCurve = new AnimationCurve(
        new Keyframe(0f, 1f),
        new Keyframe(0.3f, 0.9f),
        new Keyframe(0.6f, 0.65f),
        new Keyframe(0.85f, 0.3f),
        new Keyframe(1f, 0f)
    );

    [Header("=== Fren ===")]
    [SerializeField] private float brakeForce = 3000f;
    [SerializeField] private float handbrakeForce = 4500f;

    [Header("=== Direksiyon ===")]
    [SerializeField] private float maxSteerAngle = 42f;
    [SerializeField, Range(0.2f, 1f)] private float highSpeedSteerFactor = 0.65f;
    [SerializeField, Range(2f, 20f)] private float steerSmoothing = 6f;

    [Header("=== Drift & Yol Tutuş ===")]
    [SerializeField] private float normalRearGrip = 1.5f;
    [SerializeField, Range(0.1f, 0.9f)] private float driftGripMultiplier = 0.4f;
    [SerializeField, Range(1f, 15f)] private float gripRecoverySpeed = 5f;
    [SerializeField] private float minDriftSpeed = 25f;

    [Header("=== Stabilite Desteği ===")]
    [SerializeField, Range(0f, 1f)] private float driftStabilityAssist = 0.35f;
    [SerializeField, Range(0f, 2f)] private float counterSteerStrength = 1.0f;
    [SerializeField] private float maxDriftAngle = 50f;

    [Header("=== Fizik & Kütle ===")]
    [SerializeField] private float vehicleMass = 1400f;
    [SerializeField] private Vector3 centerOfMassOffset = new Vector3(0f, -0.5f, -0.2f);
    [SerializeField] private float downforceCoefficient = 2.0f;
    [SerializeField] private float linearDrag = 0.15f;
    [SerializeField] private bool overrideRigidbodyDrag = true;

    [Header("=== Tekerlek Collider'ları ===")]
    [SerializeField] private WheelCollider frontLeftWheelCollider;
    [SerializeField] private WheelCollider frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider;
    [SerializeField] private WheelCollider rearRightWheelCollider;

    [Header("=== Tekerlek Mesh'leri ===")]
    [SerializeField] private Transform frontLeftWheelTransform;
    [SerializeField] private Transform frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform;
    [SerializeField] private Transform rearRightWheelTransform;

    [Header("=== Ters Dönme ===")]
    [SerializeField] private bool autoRecoverFromFlip = true;
    [SerializeField] private float flipRecoveryDelay = 1.0f;
    [SerializeField] private float flipRecoveryHeight = 1.5f;

    [Header("=== Geri Vites ===")]
    [SerializeField, Range(0.5f, 2.5f)] private float reverseSteerMultiplier = 1.3f;
    [SerializeField, Range(2f, 15f)] private float reverseTransitionSpeed = 6f;

    [Header("=== Debug ===")]
    [SerializeField] private bool showDebugInfo = false;

    private Rigidbody rb;
    private float horizontalInput;
    private float verticalInput;
    private bool isHandbrake;

    private float currentSteerAngle;
    private float currentRearGrip;
    private float currentSpeedKmh;
    private float slipAngle;

    private bool isDrifting;
    private bool isFlipped;
    private float flipTimer;

    private WheelFrictionCurve origRearFriction;
    private WheelFrictionCurve origFrontFriction;

    private bool isReversing;
    private float reverseBlend;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = vehicleMass;
        rb.centerOfMass = centerOfMassOffset;

        if (overrideRigidbodyDrag)
            rb.drag = linearDrag;

        origRearFriction = rearLeftWheelCollider.sidewaysFriction;
        origFrontFriction = frontLeftWheelCollider.sidewaysFriction;

        currentRearGrip = normalRearGrip;
        ApplyRearGrip(normalRearGrip);
    }

    private void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        isHandbrake = Input.GetKey(KeyCode.Space);
    }

    private void FixedUpdate()
    {
        currentSpeedKmh = rb.velocity.magnitude * 3.6f;

        if (CheckAndHandleFlip())
            return;

        float forwardDot = Vector3.Dot(rb.velocity, transform.forward);

        // W BASILIYSA GERİ VİTESTEN KESİN ÇIK
        if (verticalInput > 0.1f)
        {
            isReversing = false;
            reverseBlend = 0f;
            ClearBrakes();
        }
        // S BASILIYSA VE ARAÇ İLERİ GİTMİYORSA GERİ VİTES MODUNA GEÇ
        else if (verticalInput < -0.1f && forwardDot < 1f)
        {
            isReversing = true;
        }
        // TUŞ YOKSA, ARAÇ GERİ KAYIYORSA GERİ MODDA KALABİLİR
        else if (Mathf.Abs(verticalInput) < 0.1f)
        {
            isReversing = forwardDot < -0.5f;
        }

        float targetBlend = isReversing ? 1f : 0f;
        reverseBlend = Mathf.MoveTowards(
            reverseBlend,
            targetBlend,
            Time.fixedDeltaTime * reverseTransitionSpeed
        );

        if (isReversing)
        {
            HandleReverseMode();
        }
        else
        {
            CalculateSlipAngle();
            HandleMotor();
            HandleSteering();
            HandleBraking();
            HandleDriftGrip();
            HandleStabilityAssist();
            ApplyDownforce();
        }

        UpdateWheels();

        if (showDebugInfo)
            DebugLog();
    }

    private void HandleReverseMode()
    {
        frontLeftWheelCollider.motorTorque = 0f;
        frontRightWheelCollider.motorTorque = 0f;
        rearLeftWheelCollider.motorTorque = 0f;
        rearRightWheelCollider.motorTorque = 0f;

        ClearBrakes();

        float reverseSteerAngle = horizontalInput * maxSteerAngle * reverseSteerMultiplier;
        frontLeftWheelCollider.steerAngle = reverseSteerAngle;
        frontRightWheelCollider.steerAngle = reverseSteerAngle;

        if (verticalInput < -0.05f)
        {
            float reverseForce = Mathf.Abs(verticalInput) * maxMotorTorque * 0.7f;
            float maxReverseSpeed = 60f;
            float speedLimiter = 1f - Mathf.Clamp01(currentSpeedKmh / maxReverseSpeed);

            rb.AddForce(
                -transform.forward * reverseForce * speedLimiter * reverseBlend,
                ForceMode.Force
            );
        }
        else
        {
            rb.velocity = Vector3.Lerp(
                rb.velocity,
                Vector3.zero,
                Time.fixedDeltaTime * 2f * reverseBlend
            );
        }

        Vector3 av = rb.angularVelocity;
        av.x *= 0.85f;
        av.z *= 0.85f;

        if (Mathf.Abs(horizontalInput) > 0.15f)
        {
            float speedFactor = Mathf.Clamp01(currentSpeedKmh / 40f);
            float targetYaw = -horizontalInput * reverseSteerMultiplier * speedFactor * 5f;
            av.y = Mathf.Lerp(av.y, targetYaw, Time.fixedDeltaTime * 12f);
        }
        else
        {
            av.y *= 0.7f;
        }

        rb.angularVelocity = av;

        if (isHandbrake)
            rb.velocity *= 0.95f;

        float wheelSpinSpeed = -currentSpeedKmh * 6f;
        SpinWheelMesh(rearLeftWheelTransform, wheelSpinSpeed);
        SpinWheelMesh(rearRightWheelTransform, wheelSpinSpeed);
    }

    private void HandleMotor()
    {
        float speedRatio = Mathf.Clamp01(currentSpeedKmh / maxSpeed);
        float torqueMult = torqueCurve.Evaluate(speedRatio);

        if (isHandbrake)
        {
            rearLeftWheelCollider.motorTorque = 0f;
            rearRightWheelCollider.motorTorque = 0f;
            frontLeftWheelCollider.motorTorque = 0f;
            frontRightWheelCollider.motorTorque = 0f;
            return;
        }

        float torque = verticalInput * maxMotorTorque * torqueMult;

        rearLeftWheelCollider.motorTorque = torque;
        rearRightWheelCollider.motorTorque = torque;

        frontLeftWheelCollider.motorTorque = 0f;
        frontRightWheelCollider.motorTorque = 0f;
    }

    private void HandleSteering()
    {
        float speedRatio = Mathf.Clamp01(currentSpeedKmh / maxSpeed);
        float steerLimit = Mathf.Lerp(1f, highSpeedSteerFactor, speedRatio);
        float targetSteerAngle = horizontalInput * maxSteerAngle * steerLimit;

        currentSteerAngle = Mathf.Lerp(
            currentSteerAngle,
            targetSteerAngle,
            Time.fixedDeltaTime * steerSmoothing
        );

        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    private void HandleBraking()
    {
        if (isHandbrake)
        {
            frontLeftWheelCollider.brakeTorque = 0f;
            frontRightWheelCollider.brakeTorque = 0f;
            rearLeftWheelCollider.brakeTorque = handbrakeForce;
            rearRightWheelCollider.brakeTorque = handbrakeForce;

            if (currentSpeedKmh > minDriftSpeed)
                isDrifting = true;
        }
        else if (verticalInput < -0.1f)
        {
            float forwardDot = Vector3.Dot(rb.velocity, transform.forward);

            if (forwardDot > 1f)
            {
                float brake = brakeForce * Mathf.Abs(verticalInput);

                frontLeftWheelCollider.brakeTorque = brake;
                frontRightWheelCollider.brakeTorque = brake;
                rearLeftWheelCollider.brakeTorque = brake;
                rearRightWheelCollider.brakeTorque = brake;
            }
            else
            {
                ClearBrakes();
            }
        }
        else if (Mathf.Abs(verticalInput) < 0.1f)
        {
            float idleBrake = 300f;

            frontLeftWheelCollider.brakeTorque = idleBrake;
            frontRightWheelCollider.brakeTorque = idleBrake;
            rearLeftWheelCollider.brakeTorque = idleBrake;
            rearRightWheelCollider.brakeTorque = idleBrake;
        }
        else
        {
            ClearBrakes();
        }
    }

    private void HandleDriftGrip()
    {
        float targetGrip = isDrifting || isHandbrake
            ? normalRearGrip * driftGripMultiplier
            : normalRearGrip;

        if (!isHandbrake && isDrifting)
        {
            if (Mathf.Abs(slipAngle) > 8f && currentSpeedKmh > minDriftSpeed)
            {
                targetGrip = normalRearGrip * driftGripMultiplier * 1.3f;
            }
            else
            {
                isDrifting = false;
                targetGrip = normalRearGrip;
            }
        }

        currentRearGrip = Mathf.Lerp(
            currentRearGrip,
            targetGrip,
            Time.fixedDeltaTime * gripRecoverySpeed
        );

        ApplyRearGrip(currentRearGrip);
    }

    private void HandleStabilityAssist()
    {
        if (Mathf.Abs(slipAngle) < 3f || currentSpeedKmh < 10f)
            return;

        bool counterSteer =
            (slipAngle > 0f && horizontalInput < -0.1f) ||
            (slipAngle < 0f && horizontalInput > 0.1f);

        float correction = -slipAngle * driftStabilityAssist * rb.mass * 0.5f;

        if (counterSteer)
            correction *= counterSteerStrength;

        if (Mathf.Abs(slipAngle) > maxDriftAngle)
        {
            float emergency = Mathf.InverseLerp(
                maxDriftAngle,
                maxDriftAngle + 25f,
                Mathf.Abs(slipAngle)
            );

            correction *= 1f + emergency * 2.5f;
        }

        rb.AddTorque(0f, correction * Time.fixedDeltaTime, 0f, ForceMode.Impulse);
    }

    private void ApplyDownforce()
    {
        if (currentSpeedKmh < 20f) return;

        float speed = rb.velocity.magnitude;
        rb.AddForce(-Vector3.up * downforceCoefficient * speed * speed);
    }

    private void CalculateSlipAngle()
    {
        if (currentSpeedKmh < 3f)
        {
            slipAngle = 0f;
            return;
        }

        Vector3 vel = rb.velocity;
        vel.y = 0f;

        if (vel.sqrMagnitude < 0.1f)
        {
            slipAngle = 0f;
            return;
        }

        slipAngle = Vector3.SignedAngle(transform.forward, vel.normalized, Vector3.up);
    }

    private bool CheckAndHandleFlip()
    {
        bool wasFlipped = isFlipped;
        isFlipped = transform.up.y < 0.4f;

        if (isFlipped && !wasFlipped)
        {
            rb.isKinematic = true;
            flipTimer = 0f;
        }

        if (!isFlipped)
        {
            if (wasFlipped)
            {
                rb.isKinematic = false;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            flipTimer = 0f;
            return false;
        }

        if (autoRecoverFromFlip)
        {
            flipTimer += Time.fixedDeltaTime;

            if (flipTimer >= flipRecoveryDelay)
            {
                Vector3 euler = transform.eulerAngles;

                transform.position = rb.position + Vector3.up * flipRecoveryHeight;
                transform.rotation = Quaternion.Euler(0f, euler.y, 0f);

                isDrifting = false;
                slipAngle = 0f;
                currentRearGrip = normalRearGrip;
                ApplyRearGrip(normalRearGrip);

                rb.isKinematic = false;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                flipTimer = 0f;
            }
        }

        return true;
    }

    private void ApplyRearGrip(float stiffness)
    {
        WheelFrictionCurve f = origRearFriction;
        f.stiffness = stiffness;

        rearLeftWheelCollider.sidewaysFriction = f;
        rearRightWheelCollider.sidewaysFriction = f;
    }

    private void ClearBrakes()
    {
        frontLeftWheelCollider.brakeTorque = 0f;
        frontRightWheelCollider.brakeTorque = 0f;
        rearLeftWheelCollider.brakeTorque = 0f;
        rearRightWheelCollider.brakeTorque = 0f;
    }

    private void UpdateWheels()
    {
        UpdateWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateWheel(frontRightWheelCollider, frontRightWheelTransform);
        UpdateWheel(rearLeftWheelCollider, rearLeftWheelTransform);
        UpdateWheel(rearRightWheelCollider, rearRightWheelTransform);
    }

    private void UpdateWheel(WheelCollider col, Transform tr)
    {
        if (col == null || tr == null) return;
        if (!col.enabled) return;

        col.GetWorldPose(out Vector3 pos, out Quaternion rot);

        tr.position = pos;
        tr.rotation = rot;
    }

    private void SpinWheelMesh(Transform wheelTr, float degreesPerSecond)
    {
        if (wheelTr == null) return;
        wheelTr.Rotate(Vector3.right, degreesPerSecond * Time.fixedDeltaTime, Space.Self);
    }

    private void DebugLog()
    {
        Debug.Log($"[Simcade] {currentSpeedKmh:F0}km/h | Kayma:{slipAngle:F1}° | Drift:{isDrifting} | Grip:{currentRearGrip:F2}");
    }

    public float SpeedKmh => currentSpeedKmh;
    public bool IsDrifting => isDrifting;
    public float SlipAngle => slipAngle;
    public float CurrentRearGrip => currentRearGrip;
    public float SpeedRatio => Mathf.Clamp01(currentSpeedKmh / maxSpeed);
}