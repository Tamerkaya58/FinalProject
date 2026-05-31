using UnityEngine;

/// <summary>
/// Simcade araç kontrolcüsü — WheelCollider tabanlı, temiz fizik.
/// Orijinal CarController'ın çalışan yapısı üzerine simcade özellikler eklendi.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class SimcadeCarController : MonoBehaviour
{
    // =====================================================================
    // MOTOR & HIZ
    // =====================================================================
    [Header("=== Motor & Hız ===")]
    [Tooltip("Maksimum motor torku (Nm).")]
    [SerializeField] private float maxMotorTorque = 2500f;

    [Tooltip("Maksimum hız (km/h).")]
    [SerializeField] private float maxSpeed = 200f;

    [Tooltip("İvmelenme eğrisi. X=normalizeHız(0-1), Y=torkÇarpanı(0-1).")]
    [SerializeField] private AnimationCurve torqueCurve = new AnimationCurve(
        new Keyframe(0f, 1f),
        new Keyframe(0.3f, 0.9f),
        new Keyframe(0.6f, 0.65f),
        new Keyframe(0.85f, 0.3f),
        new Keyframe(1f, 0f)
    );

    // =====================================================================
    // FREN
    // =====================================================================
    [Header("=== Fren ===")]
    [SerializeField] private float brakeForce = 3000f;
    [SerializeField] private float handbrakeForce = 4500f;

    // =====================================================================
    // DİREKSİYON
    // =====================================================================
    [Header("=== Direksiyon ===")]
    [SerializeField] private float maxSteerAngle = 42f;

    [Tooltip("Yüksek hızda direksiyon açısı çarpanı (0-1). Daha yüksek = yüksek hızda daha çevik.")]
    [SerializeField, Range(0.2f, 1f)] private float highSpeedSteerFactor = 0.65f;

    [Tooltip("Direksiyon yumuşatma hızı (derece/saniye cinsinden değil, lerp katsayısı). Yüksek = anlık tepki, düşük = yumuşak.")]
    [SerializeField, Range(2f, 20f)] private float steerSmoothing = 6f;

    // =====================================================================
    // DRİFT & YOL TUTUŞ
    // =====================================================================
    [Header("=== Drift & Yol Tutuş ===")]
    [Tooltip("Arka tekerleklerin normal yanal sürtünme sertliği.")]
    [SerializeField] private float normalRearGrip = 1.5f;

    [Tooltip("Drift sırasında arka grip çarpanı (0-1). Düşük=daha fazla kayma.")]
    [SerializeField, Range(0.1f, 0.9f)] private float driftGripMultiplier = 0.4f;

    [Tooltip("Grip geçiş hızı.")]
    [SerializeField, Range(1f, 15f)] private float gripRecoverySpeed = 5f;

    [Tooltip("Drift için minimum hız (km/h).")]
    [SerializeField] private float minDriftSpeed = 25f;

    // =====================================================================
    // STABİLİTE DESTEĞİ
    // =====================================================================
    [Header("=== Stabilite Desteği ===")]
    [Tooltip("ESP destek gücü (0=yok, 1=tam).")]
    [SerializeField, Range(0f, 1f)] private float driftStabilityAssist = 0.35f;

    [Tooltip("Counter-steer bonus çarpanı.")]
    [SerializeField, Range(0f, 2f)] private float counterSteerStrength = 1.0f;

    [Tooltip("Spin koruması devreye giren açı (derece).")]
    [SerializeField] private float maxDriftAngle = 50f;

    // =====================================================================
    // FİZİK & KÜTLE
    // =====================================================================
    [Header("=== Fizik & Kütle ===")]
    [SerializeField] private float vehicleMass = 1400f;

    [Tooltip("Ağırlık merkezi ofseti. Y↓=stabilite, Z↓=arka çekiş hissi.")]
    [SerializeField] private Vector3 centerOfMassOffset = new Vector3(0f, -0.5f, -0.2f);

    [Tooltip("Yüksek hızda yere yapışma kuvveti.")]
    [SerializeField] private float downforceCoefficient = 2.0f;

    [Tooltip("Rigidbody linear drag (hava/yol sürtünmesi — off-throttle yavaşlama). 0'dan küçük bırakılırsa Inspector'daki Rigidbody.drag korunur.")]
    [SerializeField] private float linearDrag = 0.15f;

    [Tooltip("Inspector'daki Rigidbody.drag değerinin script tarafından override edilip edilmeyeceği.")]
    [SerializeField] private bool overrideRigidbodyDrag = true;

    // =====================================================================
    // TEKERLEK REFERANSLARI
    // =====================================================================
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

    // =====================================================================
    // TERS DÖNME
    // =====================================================================
    [Header("=== Ters Dönme ===")]
    [SerializeField] private bool autoRecoverFromFlip = true;
    [SerializeField] private float flipRecoveryDelay = 1.0f;
    [SerializeField] private float flipRecoveryHeight = 1.5f;

    // =====================================================================
    // GERİ VİTES AYARLARI
    // =====================================================================
    [Header("=== Geri Vites ===")]
    [Tooltip("Geri giderken direksiyon hassasiyeti çarpanı. 1=normal, 1.5=daha duyarlı.")]
    [SerializeField, Range(0.5f, 2.5f)] private float reverseSteerMultiplier = 1.3f;

    [Tooltip("Geri giderken yanal sürtünme çarpanı (0-1). Düşük=kolay dönüş.")]
    [SerializeField, Range(0.1f, 1f)] private float reverseFrictionDamping = 0.4f;

    [Tooltip("Geri giderken direksiyon kırıldığında uygulanan ek dönüş kuvveti.")]
    [SerializeField] private float reverseTorqueCompensation = 800f;

    [Header("=== Debug ===")]
    [SerializeField] private bool showDebugInfo = false;

    // =====================================================================
    // PRİVATE
    // =====================================================================
    private Rigidbody rb;
    private float horizontalInput, verticalInput;
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

    // Geri↔ileri vites geçişini yumuşatmak için 0..1 blend değeri.
    // 0 = tam ileri sürüş modu, 1 = tam geri vites modu.
    private float reverseBlend;
    [Tooltip("Geri↔ileri vites moduna geçiş yumuşatma hızı (0-1 saniye başına). Yüksek = anlık geçiş.")]
    [SerializeField, Range(2f, 15f)] private float reverseTransitionSpeed = 6f;

    // =====================================================================
    // LIFECYCLE
    // =====================================================================

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = vehicleMass;
        rb.centerOfMass = centerOfMassOffset;
        if (overrideRigidbodyDrag)
        {
            rb.drag = linearDrag; // Hava/Yol sürtünmesi (off-throttle yavaşlama için)
        }

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

        Debug.Log("Vertical Input = " + verticalInput);
    }

    private void FixedUpdate()
    {
        currentSpeedKmh = rb.velocity.magnitude * 3.6f;

        // --- Ters dönme ---
        if (CheckAndHandleFlip())
            return;

        // --- Geri gitme tespiti ---
        float forwardDot = Vector3.Dot(rb.velocity, transform.forward);
        isReversing = forwardDot < -0.5f || (isReversing && forwardDot < 0.5f); // Hysteresis

        // Mod geçişi yumuşatma — 0=ileri, 1=geri. Anlık atlamayı engeller.
        float targetBlend = isReversing ? 1f : 0f;
        reverseBlend = Mathf.MoveTowards(reverseBlend, targetBlend,
                                         Time.fixedDeltaTime * reverseTransitionSpeed);

        if (isReversing)
        {
            // ============================================
            // GERİ VİTES — WheelCollider açık (süspansiyon için)
            // ama motorTorque = 0, hareket rb.AddForce ile
            // ============================================
            HandleReverseMode();
        }
        else
        {
            // ============================================
            // İLERİ SÜRÜŞ — Normal WheelCollider fiziği
            // ============================================
            CalculateSlipAngle();
            HandleMotor();
            HandleSteering();
            HandleBraking();
            HandleDriftGrip();
            HandleStabilityAssist();
            ApplyDownforce();
        }

        UpdateWheels();
        if (showDebugInfo) DebugLog();
    }

    // =====================================================================
    // GERİ VİTES MODU — WheelCollider açık ama motor sıfır
    // =====================================================================

    private void HandleReverseMode()
    {
        // WheelCollider'ların motor ve fren torkunu tamamen sıfırla
        // Bu, geri giderken tekerlek dönüşünün friction oscillation üretmesini engeller
        frontLeftWheelCollider.motorTorque = 0f;
        frontRightWheelCollider.motorTorque = 0f;
        rearLeftWheelCollider.motorTorque = 0f;
        rearRightWheelCollider.motorTorque = 0f;
        frontLeftWheelCollider.brakeTorque = 0f;
        frontRightWheelCollider.brakeTorque = 0f;
        rearLeftWheelCollider.brakeTorque = 0f;
        rearRightWheelCollider.brakeTorque = 0f;

        // Direksiyonu WheelCollider'a ver (görsel için — titreme motorTorque'tan geliyordu, steerAngle'dan değil)
        float reverseSteerAngle = horizontalInput * maxSteerAngle * reverseSteerMultiplier;
        frontLeftWheelCollider.steerAngle = reverseSteerAngle;
        frontRightWheelCollider.steerAngle = reverseSteerAngle;

        // --- İtme Kuvveti (Rigidbody ile) ---
        // reverseBlend (0→1) ile çarpılarak ileri↔geri vites geçişinde sert kuvvet sıçraması engellenir.
        if (verticalInput < -0.05f)
        {
            float reverseForce = Mathf.Abs(verticalInput) * maxMotorTorque * 0.7f;
            float maxReverseSpeed = 60f;
            float speedLimiter = 1f - Mathf.Clamp01(currentSpeedKmh / maxReverseSpeed);
            rb.AddForce(-transform.forward * reverseForce * speedLimiter * reverseBlend, ForceMode.Force);
        }
        else if (verticalInput > 0.1f)
        {
            // İleri gaz → fren etkisi
            rb.AddForce(transform.forward * brakeForce * verticalInput * 0.5f * reverseBlend, ForceMode.Force);
        }
        else
        {
            // Gaz yok → doğal yavaşlama (yol sürtünmesi + motor freni simülasyonu)
            // Lerp hızı reverseBlend ile yumuşatılır.
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, Time.fixedDeltaTime * 2f * reverseBlend);
        }

        // --- Direksiyon (Angular Velocity ile — güçlendirilmiş) ---
        Vector3 av = rb.angularVelocity;
        av.x *= 0.85f;
        av.z *= 0.85f;

        bool hasSteerInput = Mathf.Abs(horizontalInput) > 0.15f;
        if (hasSteerInput)
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

        // --- El freni ---
        if (isHandbrake)
        {
            rb.velocity *= 0.95f;
        }

        // --- Görsel: Arka tekerlekleri manuel döndür (motor torque=0 olduğu için) ---
        float wheelSpinSpeed = -currentSpeedKmh * 6f;
        SpinWheelMesh(rearLeftWheelTransform, wheelSpinSpeed);
        SpinWheelMesh(rearRightWheelTransform, wheelSpinSpeed);
    }

    // =====================================================================
    // MOTOR — RWD + NON-LİNEER TORK
    // =====================================================================

    private void HandleMotor()
    {
        Debug.Log("Rear Torque: " + rearLeftWheelCollider.motorTorque +
          " | Grounded: " + rearLeftWheelCollider.isGrounded +
          " | Brake: " + rearLeftWheelCollider.brakeTorque);
        float speedRatio = Mathf.Clamp01(currentSpeedKmh / maxSpeed);
        float torqueMult = torqueCurve.Evaluate(speedRatio);

        if (isHandbrake)
        {
            // El freni çekiliyken motor torku yok
            rearLeftWheelCollider.motorTorque = 0f;
            rearRightWheelCollider.motorTorque = 0f;
            frontLeftWheelCollider.motorTorque = 0f;
            frontRightWheelCollider.motorTorque = 0f;
            return;
        }

        float torque = verticalInput * maxMotorTorque * torqueMult;

        // RWD — arka tekerleklere tork
        rearLeftWheelCollider.motorTorque = torque;
        rearRightWheelCollider.motorTorque = torque;
        frontLeftWheelCollider.motorTorque = 0f;
        frontRightWheelCollider.motorTorque = 0f;
    }

    // =====================================================================
    // DİREKSİYON — HIZA DUYARLI
    // =====================================================================

    private void HandleSteering()
    {
        float speedRatio = Mathf.Clamp01(currentSpeedKmh / maxSpeed);
        float steerLimit = Mathf.Lerp(1f, highSpeedSteerFactor, speedRatio);
        float targetSteerAngle = horizontalInput * maxSteerAngle * steerLimit;

        // Smooth steering — anlık değişim yerine target'a doğru lerp ile yumuşatma.
        currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetSteerAngle,
                                       Time.fixedDeltaTime * steerSmoothing);

        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    // =====================================================================
    // FREN & EL FRENİ
    // =====================================================================

    private void HandleBraking()
    {
        if (isHandbrake)
        {
            // El freni = sadece arka tekerleklere (drift tetikleyici)
            frontLeftWheelCollider.brakeTorque = 0f;
            frontRightWheelCollider.brakeTorque = 0f;
            rearLeftWheelCollider.brakeTorque = handbrakeForce;
            rearRightWheelCollider.brakeTorque = handbrakeForce;

            if (currentSpeedKmh > minDriftSpeed)
                isDrifting = true;
        }
        else if (verticalInput < -0.1f)
        {
            // Geri input = ileri giderken fren, duruyorsa geri tork (HandleMotor halleder)
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
            // Gaz/Fren pedalı bırakılmış → Motor freni (Engine Braking)
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

    // =====================================================================
    // DRİFT YOL TUTUŞ
    // =====================================================================

    private void HandleDriftGrip()
    {
        float targetGrip;

        if (isDrifting || isHandbrake)
        {
            targetGrip = normalRearGrip * driftGripMultiplier;
        }
        else
        {
            targetGrip = normalRearGrip;
        }

        // El freni bırakıldığında
        if (!isHandbrake && isDrifting)
        {
            if (Mathf.Abs(slipAngle) > 8f && currentSpeedKmh > minDriftSpeed)
            {
                // Kayma devam ediyorsa hafif artan grip ile drift sürsün
                targetGrip = normalRearGrip * driftGripMultiplier * 1.3f;
            }
            else
            {
                isDrifting = false;
                targetGrip = normalRearGrip;
            }
        }

        currentRearGrip = Mathf.Lerp(currentRearGrip, targetGrip,
                                     Time.fixedDeltaTime * gripRecoverySpeed);
        ApplyRearGrip(currentRearGrip);
    }

    /// <summary>
    /// Geri vitesten ileri vitese geçişte tüm sürtünmeleri orijinal haline döndürür.
    /// </summary>
    private void RestoreAllFriction()
    {
        frontLeftWheelCollider.sidewaysFriction = origFrontFriction;
        frontRightWheelCollider.sidewaysFriction = origFrontFriction;
        ApplyRearGrip(normalRearGrip);

        WheelCollider[] all = {
            frontLeftWheelCollider, frontRightWheelCollider,
            rearLeftWheelCollider, rearRightWheelCollider
        };
        foreach (var w in all)
        {
            WheelFrictionCurve ff = w.forwardFriction;
            ff.stiffness = 1f;
            w.forwardFriction = ff;
        }
    }

    // =====================================================================
    // WHEEL COLLIDER TOGGLE
    // =====================================================================

    private void SetWheelCollidersEnabled(bool enabled)
    {
        frontLeftWheelCollider.enabled = enabled;
        frontRightWheelCollider.enabled = enabled;
        rearLeftWheelCollider.enabled = enabled;
        rearRightWheelCollider.enabled = enabled;
    }

    // =====================================================================
    // STABİLİTE DESTEĞİ
    // =====================================================================

    private void HandleStabilityAssist()
    {
        if (Mathf.Abs(slipAngle) < 3f || currentSpeedKmh < 10f)
            return;

        bool counterSteer = (slipAngle > 0f && horizontalInput < -0.1f) ||
                            (slipAngle < 0f && horizontalInput > 0.1f);

        float correction = -slipAngle * driftStabilityAssist * rb.mass * 0.5f;

        if (counterSteer)
            correction *= counterSteerStrength;

        // Spin koruması
        if (Mathf.Abs(slipAngle) > maxDriftAngle)
        {
            float emergency = Mathf.InverseLerp(maxDriftAngle, maxDriftAngle + 25f,
                                                 Mathf.Abs(slipAngle));
            correction *= (1f + emergency * 2.5f);
        }

        rb.AddTorque(0f, correction * Time.fixedDeltaTime, 0f, ForceMode.Impulse);
    }

    // =====================================================================
    // DOWNFORCE
    // =====================================================================

    private void ApplyDownforce()
    {
        if (currentSpeedKmh < 20f) return;
        float speed = rb.velocity.magnitude;
        rb.AddForce(-Vector3.up * downforceCoefficient * speed * speed);
    }

    // =====================================================================
    // KAYMA AÇISI
    // =====================================================================

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

    // =====================================================================
    // TERS DÖNME
    // =====================================================================

    /// <returns>true ise FixedUpdate'in geri kalanını atla.</returns>
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
                // Az önce düzelmiş — fiziği geri aç
                rb.isKinematic = false;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            flipTimer = 0f;
            return false;
        }

        // Ters durumdayız
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

    // =====================================================================
    // YARDIMCILAR
    // =====================================================================

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

    // =====================================================================
    // PUBLIC
    // =====================================================================
    public float SpeedKmh => currentSpeedKmh;
    public bool IsDrifting => isDrifting;
    public float SlipAngle => slipAngle;
    public float CurrentRearGrip => currentRearGrip;
    public float SpeedRatio => Mathf.Clamp01(currentSpeedKmh / maxSpeed);
}
