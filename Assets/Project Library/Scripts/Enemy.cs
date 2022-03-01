using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Enemy : MonoBehaviour
{
    public float speed;
    private Rigidbody enemyRb;
    private GameObject player;

    private AudioSource playerAudio;
    public AudioClip enemyFallSound;
    private bool onlyFirstTime;
    public ParticleSystem explosionParticle;

   
    void Start()
    {
        enemyRb = GetComponent<Rigidbody>();
        player = GameObject.Find("Player");
        playerAudio = GetComponent<AudioSource>();
        playerAudio.Stop(); // just incase PlayOnAwake is ticked
        onlyFirstTime = true;
    }
   
    void Update()
    {
        if (player.GetComponent<PlayerController>().gameOver == true)
        {
            Destroy(gameObject);
            return;
        }

        if (onlyFirstTime && ((transform.position.y < -1.0f) || (Mathf.Abs(transform.position.x) > 20) || (Mathf.Abs(transform.position.z) > 20)))
        {
            // enemy is falling down
            onlyFirstTime = false;
            playerAudio.PlayOneShot(enemyFallSound, 1.0f);

        }

        if (transform.position.y < -10)
        {
            player.GetComponent<PlayerController>().UpdateScore(10);
            Destroy(gameObject);
        }

        // -- enemy behaviour 
        // ------------------
        if (!player.GetComponent<PlayerController>().hasPowerup) {
            // player in normal status
            Vector3 lookDirection = (player.transform.position - transform.position).normalized + new Vector3(0, -1f, 0); 
            enemyRb.AddForce(lookDirection * 2 * speed * player.GetComponent<PlayerController>().tuneAmplifier);
        }
        else
        {
            // player has powerup
            // ------------------
            Vector3 lookDirection = (transform.position - player.transform.position).normalized + new Vector3(0, -1f, 0); ;
            //Vector3 lookDirection = (transform.position - player.transform.position).normalized ;
            //enemyRb.AddForce(lookDirection * 2 * speed * player.GetComponent<PlayerController>().tuneAmplifier);
            // go away via the perpendicular 
            //Vector3 perp = Vector3.Cross(lookDirection, new Vector3(0, 1f, 0)) ;
            
            enemyRb.AddForce(lookDirection * 10 * speed * player.GetComponent<PlayerController>().tuneAmplifier);
            enemyRb.AddForce(lookDirection * 10 * speed );

        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Ground") && !collision.gameObject.CompareTag("Fence") && !collision.gameObject.CompareTag("Enemy"))
        {
            // -- manage the collision with the player
            // ---------------------------------------

            //Instantiate(explosionParticle, transform.position, transform.rotation);
            Rigidbody colliderRb = collision.gameObject.GetComponent<Rigidbody>();
            float coeff = speed; // * player.GetComponent<PlayerController>().tuneAmplifier;            
            Vector3 awayFromEnemy = (collision.gameObject.transform.position - transform.position);
            
            awayFromEnemy = awayFromEnemy + new Vector3(0,2.0f, 0);
           
            colliderRb.AddForce(awayFromEnemy * coeff, ForceMode.Impulse);
           
        }
    }
}
