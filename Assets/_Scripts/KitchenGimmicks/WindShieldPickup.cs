using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WindShieldPickup : MonoBehaviour
{
    [Range(0f, .95f)] [SerializeField] private float windReduction = .75f;
    [SerializeField] private float spinSpeed = 80f;
    [SerializeField] private float bobHeight = .2f;
    [SerializeField] private float bobSpeed = 2.2f;

    private Vector3 startPosition;
    private bool collected;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        startPosition = transform.position;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
        transform.position = startPosition + Vector3.up *
            (Mathf.Sin(Time.time * bobSpeed) * bobHeight);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected)
            return;

        PlayerBall player = other.GetComponentInParent<PlayerBall>();
        if (player == null)
            return;

        collected = true;
        player.ActivateWindShield(windReduction);
        KitchenGameManager.Instance?.ShowMessage("WIND SHIELD!  FAN FORCE -75%", 2.5f);
        gameObject.SetActive(false);
    }
}
