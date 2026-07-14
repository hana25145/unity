using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HoneyZone : MonoBehaviour
{
    [Header("Honey slowdown")]
    [Range(0.05f, 1f)] [SerializeField] private float accelerationMultiplier = 0.65f;
    [Range(0.05f, 1f)] [SerializeField] private float maxSpeedMultiplier = 0.7f;
    [SerializeField] private float honeyLinearDamping = 1.25f;
    [Header("Optional texture mask")]
    [SerializeField] private Renderer maskRenderer;
    [Range(0f, 1f)] [SerializeField] private float alphaThreshold = 0.15f;

    private readonly HashSet<PlayerBall> affectedPlayers = new();
    private Texture2D maskTexture;
    private MeshFilter maskMeshFilter;

    public void Configure(float acceleration, float maxSpeed, float linearDamping)
    {
        accelerationMultiplier = Mathf.Clamp(acceleration, 0.05f, 1f);
        maxSpeedMultiplier = Mathf.Clamp(maxSpeed, 0.05f, 1f);
        honeyLinearDamping = Mathf.Max(0f, linearDamping);
    }

    public void ConfigureMask(Renderer renderer, float threshold = 0.15f)
    {
        maskRenderer = renderer;
        alphaThreshold = Mathf.Clamp01(threshold);
    }

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        if (maskRenderer != null)
        {
            maskMeshFilter = maskRenderer.GetComponent<MeshFilter>();
            maskTexture = maskRenderer.sharedMaterial != null
                ? maskRenderer.sharedMaterial.mainTexture as Texture2D
                : null;
        }
    }

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerBall player = other.GetComponentInParent<PlayerBall>();
        if (player == null)
            return;

        if (maskRenderer == null)
            ApplyEffect(player);
        else
            UpdateMaskedEffect(player);
    }

    private void OnTriggerStay(Collider other)
    {
        if (maskRenderer == null)
            return;

        PlayerBall player = other.GetComponentInParent<PlayerBall>();
        if (player != null)
            UpdateMaskedEffect(player);
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerBall player = other.GetComponentInParent<PlayerBall>();
        if (player != null)
            RemoveEffect(player);
    }

    private void OnDisable()
    {
        foreach (PlayerBall player in affectedPlayers)
        {
            if (player != null)
                player.RemoveSurfaceEffect(this);
        }
        affectedPlayers.Clear();
    }

    private void UpdateMaskedEffect(PlayerBall player)
    {
        if (IsHoneyPixel(player.transform.position))
            ApplyEffect(player);
        else
            RemoveEffect(player);
    }

    private bool IsHoneyPixel(Vector3 worldPosition)
    {
        if (maskTexture == null || maskMeshFilter == null || !maskTexture.isReadable)
            return false;

        Bounds bounds = maskMeshFilter.sharedMesh.bounds;
        Vector3 local = maskRenderer.transform.InverseTransformPoint(worldPosition);
        Vector3 size = bounds.size;
        int firstAxis = LargestAxis(size, -1);
        int secondAxis = LargestAxis(size, firstAxis);
        float u = Mathf.InverseLerp(Axis(bounds.min, firstAxis), Axis(bounds.max, firstAxis), Axis(local, firstAxis));
        float v = Mathf.InverseLerp(Axis(bounds.min, secondAxis), Axis(bounds.max, secondAxis), Axis(local, secondAxis));
        return maskTexture.GetPixelBilinear(Mathf.Clamp01(u), Mathf.Clamp01(v)).a >= alphaThreshold;
    }

    private void ApplyEffect(PlayerBall player)
    {
        player.AddSurfaceEffect(this, accelerationMultiplier, maxSpeedMultiplier, honeyLinearDamping);
        affectedPlayers.Add(player);
    }

    private void RemoveEffect(PlayerBall player)
    {
        player.RemoveSurfaceEffect(this);
        affectedPlayers.Remove(player);
    }

    private static int LargestAxis(Vector3 size, int excluded)
    {
        int result = excluded == 0 ? 1 : 0;
        for (int axis = 0; axis < 3; axis++)
        {
            if (axis != excluded && Axis(size, axis) > Axis(size, result))
                result = axis;
        }
        return result;
    }

    private static float Axis(Vector3 value, int axis)
    {
        return axis == 0 ? value.x : axis == 1 ? value.y : value.z;
    }
}
