using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraMove : MonoBehaviour
{
    
    public GameObject spielberg;
    public float OffsetX;
    public float OffsetY;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag != "Player") return;
        // spielberg.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenY = OffsetY;
        // spielberg.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenX = OffsetX;
        spielberg.transform.position = new Vector3(OffsetX, OffsetY, 0);
        Debug.Log("Changing Camera Shot");
    }
}
