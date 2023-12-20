using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    public float zoomSpeed = 5f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float scrollAmount = Input.GetAxis("Mouse ScrollWheel");
        if (scrollAmount > 0f)
        {
            if (!(Camera.main.orthographicSize-zoomSpeed <= 0))
            {
                Camera.main.orthographicSize -= zoomSpeed;
            }
        }
        else if (scrollAmount < 0f)
        {
            Camera.main.orthographicSize += zoomSpeed;
        }

        if (Input.GetKey(KeyCode.D))
        {
            transform.position = new Vector3(transform.position.x+5f, transform.position.y, transform.position.z);
        }
        if (Input.GetKey(KeyCode.Q))
        {
            transform.position = new Vector3(transform.position.x-5f, transform.position.y, transform.position.z);
        }
        if (Input.GetKey(KeyCode.Z))
        {
            transform.position = new Vector3(transform.position.x, transform.position.y+5f, transform.position.z);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position = new Vector3(transform.position.x, transform.position.y-5f, transform.position.z);
        }
    }
}
