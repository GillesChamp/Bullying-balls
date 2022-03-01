using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayStopAudioOnGameStatus : MonoBehaviour
{
    private GameObject player;
    AudioSource m_MyAudioSource;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
        m_MyAudioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (player.GetComponent<PlayerController>().gameOver == true)
        {
            m_MyAudioSource.Stop();
            return;
        }
    }
}
