using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : MonoBehaviour
{
    private GameObject player;
    
    void Start()
    {
        player = GameObject.Find("Player");

    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0f, player.GetComponent<PlayerController>().tuneAmplifier, 0f,Space.Self);
    }
}
