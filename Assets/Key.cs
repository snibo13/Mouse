using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{
    public Sprite newSprite;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            Debug.Log("Unlocking...");
            collision.gameObject.GetComponent<Movement>().lock1 = false;
            GetComponent<SpriteRenderer>().sprite = newSprite;
        }
    }
}
