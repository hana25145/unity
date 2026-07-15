using System.Collections.Generic;
using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerBall : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float rollPower = 30f;
    [SerializeField] private float maxGroundSpeed = 12f;
    [SerializeField] private float normalLinearDamping = 0.25f;
    [SerializeField] private float wallCheckDistance = 0.35f;

    [Header("Respawn")]
    [SerializeField] private float fallY = -10f;
    [SerializeField] private Transform initialCheckpoint;

    private readonly Dictionary<object, SurfaceEffect> surfaceEffects = new();
    private Rigidbody rb;
    private Vector3 checkpointPosition;
    private Quaternion checkpointRotation;
    private bool controlEnabled = true;
    private float windResistance;

    public Rigidbody Body => rb;
    public float WindForceMultiplier => 1f - windResistance;
    public event Action Respawned;

    private struct SurfaceEffect
    {
        public float AccelerationMultiplier;
        public float MaxSpeedMultiplier;
        public float LinearDamping;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        Transform checkpoint = initialCheckpoint != null ? initialCheckpoint : transform;
        SetCheckpoint(checkpoint.position, checkpoint.rotation);
    }

    private void FixedUpdate()
    {
        float accelerationMultiplier = 1f;
        float speedMultiplier = 1f;
        float stickyDamping = normalLinearDamping;
        float slipperyDamping = normalLinearDamping;
        bool hasSlipperySurface = false;

        foreach (SurfaceEffect effect in surfaceEffects.Values)
        {
            accelerationMultiplier = Mathf.Min(accelerationMultiplier, effect.AccelerationMultiplier);
            speedMultiplier = Mathf.Min(speedMultiplier, effect.MaxSpeedMultiplier);
            if (effect.LinearDamping < normalLinearDamping)
            {
                hasSlipperySurface = true;
                slipperyDamping = Mathf.Min(slipperyDamping, effect.LinearDamping);
            }
            else
            {
                stickyDamping = Mathf.Max(stickyDamping, effect.LinearDamping);
            }
        }

        Vector3 input = controlEnabled ? new Vector3(
            Input.GetAxisRaw("Horizontal"),
            0f,
            Input.GetAxisRaw("Vertical")
        ) : Vector3.zero;

        if (input.sqrMagnitude > 1f)
            input.Normalize();

        if (input.sqrMagnitude > 0.001f)
        {
            Vector3 rotationAxis = Vector3.Cross(Vector3.up, input);
            if (IsBlockedByWall(input, out _))
            {
                // Forward rolling torque turns into wall-climbing friction at a vertical wall.
                rb.angularVelocity -= Vector3.Project(rb.angularVelocity, rotationAxis.normalized);
            }
            else
            {
                rb.AddTorque(
                    rotationAxis * rollPower * accelerationMultiplier,
                    ForceMode.Acceleration
                );
            }
        }

        // 겹친 구역에서는 세제(미끄러움)를 우선하고, 그 외에는 가장 끈적한 값을 사용합니다.
        rb.linearDamping = hasSlipperySurface ? slipperyDamping : stickyDamping;
        LimitHorizontalSpeed(maxGroundSpeed * speedMultiplier);

        if (transform.position.y < fallY)
            Respawn();
    }

    private void LimitHorizontalSpeed(float limit)
    {
        Vector3 horizontal = Vector3.ProjectOnPlane(rb.linearVelocity, Vector3.up);
        if (horizontal.sqrMagnitude <= limit * limit)
            return;

        Vector3 vertical = rb.linearVelocity - horizontal;
        rb.linearVelocity = horizontal.normalized * limit + vertical;
    }

    private bool IsBlockedByWall(Vector3 direction, out RaycastHit hit)
    {
        return rb.SweepTest(
            direction.normalized,
            out hit,
            wallCheckDistance,
            QueryTriggerInteraction.Ignore
        ) && hit.normal.y < 0.45f;
    }

    public void AddSurfaceEffect(
        object source,
        float accelerationMultiplier,
        float maxSpeedMultiplier,
        float linearDamping)
    {
        if (source == null)
            return;

        surfaceEffects[source] = new SurfaceEffect
        {
            AccelerationMultiplier = Mathf.Clamp01(accelerationMultiplier),
            MaxSpeedMultiplier = Mathf.Clamp01(maxSpeedMultiplier),
            LinearDamping = Mathf.Max(0f, linearDamping)
        };
    }

    public void RemoveSurfaceEffect(object source)
    {
        if (source != null)
            surfaceEffects.Remove(source);
    }

    public void SetCheckpoint(Vector3 position, Quaternion rotation)
    {
        checkpointPosition = position;
        checkpointRotation = rotation;
    }

    public void SetControlEnabled(bool enabled)
    {
        controlEnabled = enabled;
        if (!enabled && rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void ActivateWindShield(float reduction)
    {
        windResistance = Mathf.Max(windResistance, Mathf.Clamp(reduction, 0f, .95f));
    }

    public void Respawn()
    {
        surfaceEffects.Clear();
        rb.position = checkpointPosition;
        rb.rotation = checkpointRotation;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        Respawned?.Invoke();
    }
}
