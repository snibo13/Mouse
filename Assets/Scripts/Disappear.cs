using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Disappear : MonoBehaviour
{
    public float lifetime = 0.2f;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("FadeOut");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }
}
