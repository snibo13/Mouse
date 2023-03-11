using UnityEngine;

public class GameUI : MonoBehaviour
{
    private GameObject[] hearts = new GameObject[5];
    public GameObject heart_percent;

    private Character character;

    void Start()
    {
        character = GameObject.Find("Character").GetComponent<Character>();
        heart_percent = GameObject.Find("Heart Percent");
    }

    void Update()
    {
        showHearts();
    }

    public void showHearts()
    {
        float offset = -40 * (character.getHP() - 100) / 100;
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
