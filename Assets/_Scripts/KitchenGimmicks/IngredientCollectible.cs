using UnityEngine;

[RequireComponent(typeof(Collider))]
public class IngredientCollectible : MonoBehaviour
{
    [SerializeField] private float spinSpeed = 100f;
    [SerializeField] private float bobHeight = 0.18f;
    [SerializeField] private float bobSpeed = 2.5f;

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
        transform.position = startPosition + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * bobHeight);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected || other.GetComponentInParent<PlayerBall>() == null)
            return;

        collected = true;
        KitchenGameManager.Instance?.CollectIngredient();
        gameObject.SetActive(false);
    }
}
