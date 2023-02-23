using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InteractionGlyph : MonoBehaviour
{
    public TextMeshProUGUI text;
    public string content;

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Triggering In");
        if (other.gameObject.tag == "Player")
        {
            text.enabled = true;
            text.text = content;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            text.enabled = false;
        }
    }
}
