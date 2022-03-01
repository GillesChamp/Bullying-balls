using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.Audio;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public AudioMixer mixer;
    public AudioClip powerupSound;
    public AudioClip jumpSound;
    public AudioClip collisionSound;
    public AudioClip collisionPowerSound;
    public AudioClip collisionFenceSound;
    public AudioClip fallSound;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI systemText;
    public TextMeshProUGUI fpsText;

    [HideInInspector]
    public Canvas restartButton;

    public Canvas endPanel;
    public GameObject canvas0;
    public GameObject canvas1;
    public GameObject canvas2;
    public GameObject hud;
    public GameObject cover;
    public GameObject powerupIndicator;
    public GameObject lancet;
    public GameObject enemyPrefab;
    public GameObject powerupPrefab;

    public GameObject[] layout = new GameObject[3];


    public ParticleSystem explosionParticle;
    public Slider volumeSlider;
    public Slider speedSlider;

    public float tuneAmplifier = 1.0f; // to tune the speed depending on platform
    public bool gameOver = false;
    public float speed = 5.0f;
    public bool hasPowerup;
    public int level = 0;

    private Rigidbody playerRb;
    private float powerupStrength = 15.0f;
    private GameObject focalPoint;
    private float jumpForce = 15f;
    private bool isOnGround = true;
    private int score;
    private int highScore;
    public int wave;


    public int waveNumber;
    private float spawnRange = 9;

    private int enemyCount;
    private int powerupCount;
    private AudioSource playerAudio;
    private bool onlyFirstTime;
    private GameObject videocamera;
    private AudioSource themeSource;
    private GameObject spawnManager;
    private int minLevel = 0;
    private int maxLevel = 2;
    private float volume;
    //private float fps = 30f;

    void Start()
    {
        DOTween.Init();

        // Load persistent previosly saved data
        LoadPrefs();
        if (volume <= 0.15f)
        {
            volume = 0.15f;
        }
        

        // -- GUI: present the Welcome Canvas and wait for user pushing the start button
        canvas0.gameObject.SetActive(true);
        canvas1.gameObject.SetActive(false);
        canvas2.gameObject.SetActive(false);
        cover.gameObject.SetActive(true);
        hud.gameObject.SetActive(false);

        // -- init mixer volume and volume slider
        volumeSlider.SetValueWithoutNotify(volume);
        mixer.SetFloat("musicVol", Mathf.Log10(volume) * 20); // dB

        // do a small jump during the title initialization
        DOTween.Init();
        Jump(new Vector3(0, -0.4f, -5f));

        gameOver = true;

        // -- manage game level
        level = 0;
        UpdateLevel(0);

        // -- manage the initial speed correction needed for different player platforms
        tuneAmplifier = 1.0f;                
        if (Application.platform.ToString() == "WebGLPlayer")
        {
            tuneAmplifier = 10.0f;
        }
        speedSlider.SetValueWithoutNotify(tuneAmplifier); // set the slider consequently


        // -- update HUD
        UpdateScore(0);
        UpdateHighScore(highScore);
        UpdateWave(0);
        systemText.text = "FR99887722050 Bullying Balls, concept demo,  Beta 0.1.1 /" + Application.platform + "/" + Application.systemLanguage;
         
        // -- camera audio to manage main theme
        videocamera = GameObject.Find("Main Camera");
        themeSource = videocamera.gameObject.GetComponent<AudioSource>();

        // -- spawn powerup
        Instantiate(powerupPrefab, GenerateSpawnPosition(0.5f), powerupPrefab.transform.rotation);

    }

    void Update()
    {
        //fpsText.text = "Fps: " + ((int)fps).ToString();

        if (gameOver)
        {
            return;
        }
     

        // -- manage keyboard
        // ------------------
        // if space the player jumps
        if (Input.GetKeyDown(KeyCode.Space) && isOnGround && !gameOver)
        {
            playerAudio.PlayOneShot(jumpSound, 1.0f);
            playerRb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isOnGround = false;
        }

        // -- COMMENT IN PRODUCTION
        if (Input.GetKeyDown(KeyCode.F10))
        {
            UpdateWave(1);
        }
        // ------------------------

        // -- manage input system
        // ----------------------
        // this is a sumo-like  game engine, the horizontal axys is controlled by the focal point, check it
        float forwardInput = Input.GetAxis("Vertical");
        playerRb.AddForce(focalPoint.transform.forward * speed * forwardInput * tuneAmplifier);
        

        // -- The power indicator follows the player, but is deactivated
        powerupIndicator.transform.position = transform.position + new Vector3(0, 0.1f, 0);

        // -- The particle lancet follows the player and is oriented in same direction of the focalpoint
        lancet.transform.position = transform.position + new Vector3(0, 0f, 0);
        lancet.transform.rotation = focalPoint.transform.rotation;

        // -- Game status
        // --------------
        if (onlyFirstTime && ((transform.position.y < -2) || (Mathf.Abs(transform.position.x) > 20) || (Mathf.Abs(transform.position.z) > 20)))
        {
            // -- the player is falling down            
            onlyFirstTime = false;
            SavePrefs();
            playerAudio.PlayOneShot(fallSound, 1.0f);

            // -- GAME OVER                       
            gameOver = true;   
            lancet.gameObject.SetActive(false);
            canvas1.gameObject.SetActive(true); // UI managing Game Over
        }

        // -- Enemy status
        // ---------------
        enemyCount = FindObjectsOfType<Enemy>().Length;
        if ((enemyCount == 0) && !gameOver)
        {
            UpdateWave(1);            
            // if the level is increased, the waveNumber will be reset by the playerController
            waveNumber = waveNumber + 2;
            SpawnEnemyWave(waveNumber);

            // deploy the powerup
            if (GameObject.Find("Powerup(Clone)") == null)
            {
                Instantiate(powerupPrefab, GenerateSpawnPosition(0.5f), powerupPrefab.transform.rotation);
            }

        }
    }

    public void InitGame()
    {
        gameOver = false;
        lancet.gameObject.SetActive(true);

        //level = minLevel;
        //layout[level].gameObject.SetActive(true);
                
        hud.gameObject.SetActive(true);
        canvas1.gameObject.SetActive(false);
        canvas0.gameObject.SetActive(false);
        canvas2.gameObject.SetActive(false); 
        cover.gameObject.SetActive(false);

        // -- start asudio theme
        themeSource.Play();

        // the focalpoint contains the camera, so the user rotates the focalpoint
        focalPoint = GameObject.Find("Focal Point");

        playerRb = GetComponent<Rigidbody>();
        playerAudio = GetComponent<AudioSource>();
        playerAudio.Stop(); // just incase PlayOnAwake is ticked
        gameOver = false;
        onlyFirstTime = true;
    }

    public void UpdateScore(int scoreToAdd) 
    {
        score += scoreToAdd;
        scoreText.text = "Score: " + score;
        if (score > highScore)
        {
            highScore = score;
            UpdateHighScore(score);
        }
    }

    public void UpdateHighScore(int score)
    {       
        highScoreText.text = "High Score: " + score;
    }

    public void UpdateLevel(int newLevel)
    {
        level = newLevel;
        levelText.text = "Level: " + newLevel;
        
    }

    public void UpdateWave(int waveToAdd)
    {
        wave += waveToAdd;        
        if (wave > 3)
        {
            wave = 1;
            level += 1; // 1 level increment each 3 waves
            
            // level is increased, change the board layout 
            if (level > maxLevel)
            {
                // -- THE WINNER
                // -------------
                SavePrefs();
                gameOver = true;
                powerupIndicator.gameObject.SetActive(false);
                lancet.gameObject.SetActive(false);                
                canvas2.gameObject.SetActive(true); //  WINNER PANEL
                return;
            }
            layout[level].gameObject.SetActive(true); // enable the current lavel layout, as for the arry  object loaded in edit mode
            layout[level-1].gameObject.SetActive(false); // disable the old one
            
            // reset number of enemies
            waveNumber = 1;
        }
        waveText.text  = "Wave: " + wave + " of 3";
        levelText.text = "Level: " + level;
    }

    private void OnTriggerEnter(Collider other)
    {
        // check if the player hits the powerup indicator
        if (other.CompareTag("Powerup") && !hasPowerup)
        {
            // hits the powerup
            playerAudio.PlayOneShot(powerupSound, 1.0f);
            
            hasPowerup = true;
            Destroy(other.gameObject);
            powerupIndicator.gameObject.SetActive(true);
            
            // start the counter
            StartCoroutine(PowerupCountdownRoutine());
        }
    }

    IEnumerator PowerupCountdownRoutine()
    {
        // delay some seconds 
        yield return new WaitForSeconds(8);
        hasPowerup = false;
        powerupIndicator.gameObject.SetActive(false);

        // wait 60 seconds and redeploy another powerup
        StartCoroutine(PowerupRedeploy());
    }

    IEnumerator PowerupRedeploy()
    {
        // delay some seconds and reallow the powerup object if none y
        yield return new WaitForSeconds(30*(level+1));
        if ((GameObject.Find("Powerup(Clone)") == null) && !hasPowerup)
        {
            Instantiate(powerupPrefab, GenerateSpawnPosition(0.5f), powerupPrefab.transform.rotation);
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isOnGround = true;          
        }

        if (collision.gameObject.CompareTag("Enemy") && hasPowerup)
        {
            // collision with powerup
            playerAudio.PlayOneShot(collisionPowerSound, 1.0f);
            Rigidbody enemyRb = collision.gameObject.GetComponent<Rigidbody>();
            Vector3 awayFromPlayer = (collision.gameObject.transform.position - transform.position);
            awayFromPlayer = awayFromPlayer + new Vector3(0,2,0);
            enemyRb.AddForce(awayFromPlayer * powerupStrength, ForceMode.Impulse);
            Instantiate(explosionParticle, transform.position, transform.rotation);

            //Debug.Log("Collided with:" + collision.gameObject.name + " with powerup set to " + hasPowerup);
        }

        if (collision.gameObject.CompareTag("Enemy") && !hasPowerup)
        {
            // normal collision
            playerAudio.PlayOneShot(collisionSound, 0.7f);            
            UpdateScore(-1);          
        }

        if (collision.gameObject.CompareTag("Fence") )
        {
            playerAudio.PlayOneShot(collisionFenceSound, 0.7f);      
        }
    }

    public void UpdateSpeed(float newSpeed)
    {
        if (Application.platform.ToString() == "WebGLPlayer")
        {
            // increase speed of things and so on
            tuneAmplifier = 10f + newSpeed;
        }
        else
        {
            tuneAmplifier = 1f + newSpeed;
        }
        Debug.Log("tuneAmplifier: " + tuneAmplifier);
    }

    public void RestartGame()
    {      
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SavePrefs()
    {
        PlayerPrefs.SetFloat("Volume", volumeSlider.value);
        PlayerPrefs.SetFloat("Speed", speedSlider.value);
        PlayerPrefs.SetInt("HighScore", highScore);


        PlayerPrefs.Save();       
    }

    public void LoadPrefs()
    {
        volume = PlayerPrefs.GetFloat("Volume");
        tuneAmplifier = PlayerPrefs.GetFloat("Speed");
        highScore = PlayerPrefs.GetInt("HighScore");

        //Debug.Log("volume: "+ volume + ", tuneAmplifier: " + tuneAmplifier);
    }

    private Vector3 GenerateSpawnPosition(float height)
    {
        float spawnPosX = Random.Range(-spawnRange, spawnRange);
        float spawnPozZ = Random.Range(-spawnRange, spawnRange);
        Vector3 randomPos = new Vector3(spawnPosX, height, spawnPozZ);
        return randomPos;
    }

    void SpawnEnemyWave(int enemiesToSpawn)
    {
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            Instantiate(enemyPrefab, GenerateSpawnPosition(0.8f), enemyPrefab.transform.rotation);
        }
    }

    void Jump(Vector3 finalPosition)
    {
        // the small jump on cover page
        transform.DOJump(finalPosition, 1.0f, 3,1.5f, false); //.SetLoops(-1, LoopType.Yoyo);        
    }


    /*
     * NON FUNZIONA IN webGL
     * 
    void OnGUI()
    {
        
        float newFPS = 1.0f / Time.smoothDeltaTime;
        fps = Mathf.Lerp(fps, newFPS, 0.0005f);
        GUI.Label(new Rect(0, 0, 100, 100), "FPS: " + ((int)fps).ToString());
       
        if (GUI.Button(new Rect(0, 25, 100, 30), "I am a button"))
        {
            print("Pushed");
        }
        GUI.Label(new Rect(0, 55, 100, 50), "Hello");
     }
    */

}
