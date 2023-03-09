using UnityEngine;

public class GameUI : MonoBehaviour
{
    public GameObject heart;
    private float x_offset = -2;
    private float y_offset = -4;
    private float heart_width = 1;
    private GameObject[] hearts = new GameObject[5];
    public Transform cameraTransform;
    public GameObject heart_percent;

    void Start() { }

    void Update() { }

    public void showHearts()
    {
        float offset = -40 * (hp - 100) / 100;
        heart_percent.GetComponent<RectTransform>().localPosition = new Vector2(0, offset);
    }

    public void clearHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] != null)
            {
                Destroy(hearts[i]);
            }
        }
    }
}
