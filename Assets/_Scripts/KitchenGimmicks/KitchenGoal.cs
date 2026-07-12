using UnityEngine;

[RequireComponent(typeof(Collider))]
public class KitchenGoal : MonoBehaviour
{
    private bool cleared;
    private float clearTime;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (cleared || other.GetComponentInParent<PlayerBall>() == null)
            return;

        cleared = true;
        clearTime = Time.timeSinceLevelLoad;
    }

    private void OnGUI()
    {
        GUIStyle label = new(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 24,
            fontStyle = FontStyle.Bold
        };
        label.normal.textColor = Color.white;

        string text = cleared
            ? $"COURSE CLEAR!  {clearTime:0.0}s"
            : $"KITCHEN RUN  {Time.timeSinceLevelLoad:0.0}s";
        GUI.Label(new Rect(Screen.width * 0.5f - 220f, 18f, 440f, 50f), text, label);

        GUIStyle help = new(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 16
        };
        help.normal.textColor = Color.white;
        GUI.Label(new Rect(Screen.width * 0.5f - 260f, Screen.height - 50f, 520f, 30f),
            "WASD / Arrow Keys  |  Reach the checkered goal", help);
    }
}
