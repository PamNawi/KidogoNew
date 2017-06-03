using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

enum GameStatus { waiting, started, paused, ended};

public class GameController : MonoBehaviour
{
    #region Game Variables
    //Game balance things
    public float radius = 5f;
    public int jumpFrames = 15;
    public float timeStartSecondTrack = 3.0f;

    //Visual Effects;
    public GameObject fireworkParticles;
    public GameObject fireworkImplosionParticles;
    public GameObject mouseEffects;
    ParticleSystem firework;
    
    public AudioSource countdownSound;
    public GameObject Score;

    //Music Information
    public int numberOfNotes = 12;
    public GameObject notePrefab;
    
    //Other private infos
    AudioSource firstTrack;
    AudioSource secondTrack;
    float[] spectrum = new float[1024];
    float[] secondSpectrum = new float[1024];
    List<Note> basicNotes;
    float lastPitch, lastSecondPitch;

    int frameAtual = 0;
    List<GameObject> sequenceNotes;
    GameStatus gameStatus = GameStatus.waiting;
    int countdown = 3;
    float diffTime;

    List<ParticleSystem> notesFireworks;
    List<ParticleSystem> notesImplosion;
    Note glowMouseEffect;
    ScoreController scoreController;
    int totalNotes = 0, totalPlayedNotes = 0;
    float timePauseStarted, timePauseEnd;
    List<float> totalTimePaused;
    #endregion

    #region Music Menu Variables
    private string path = "";
    private bool windowOpen = false;

    public string GameMenu = "GameMenu";
    public string thisScene = "kidogoGame";
    public Text menuText;
    #endregion
    
    // Use this for initialization
    void Awake()
    {
        FileSelector.GetFile(GotFile, ".ogg"); //generate a new FileSelector window
        windowOpen = true; //record that we have a window open

    }

    // Update is called once per frame
    void Update()
    {
        switch (gameStatus)
        {
            case GameStatus.started:
                if (secondTrack != null && !secondTrack.isPlaying && sequenceNotes.Count == 0)
                {
                    GameHasEnded();
                    break;
                }
                updateInput();
                GameUpdate();
                break;
            case GameStatus.paused:
                updateInput();
                GamePausedUpdate();
                break;
            default:
                break;
        }

    }

    public void backToMainMenu()
    {
        Application.LoadLevel(GameMenu);
    }

    public void nextMusic()
    {
        Application.LoadLevel(thisScene);
    }

    #region Music Menu Logic
    void GotFile(FileSelector.Status status, string path)
    {
        //So Start The Game
        Debug.Log("File Status : " + status + ", Path : " + path);
        this.path = path;
        this.windowOpen = false;
        if (status == FileSelector.Status.Successful)
            startGame(path);
        else
            Application.LoadLevel(GameMenu);
    }
    #endregion

    #region Game Load Logic

    void startGame(string musicPath)
    {
        //Load all things
        scoreController = Score.GetComponent<ScoreController>();
        menuText.text = "Loading Music...";
        sequenceNotes = new List<GameObject>();
        basicNotes = new List<Note>();
        notesFireworks = new List<ParticleSystem>();
        notesImplosion = new List<ParticleSystem>();
        firework = fireworkParticles.GetComponent<ParticleSystem>();
        totalTimePaused = new List<float>();

        //Create the basic notes
        for (int i = 0; i < numberOfNotes; i++)
        {
            GameObject note = Instantiate(notePrefab, Vector3.zero, Quaternion.identity);
            Note pitch = note.GetComponent<Note>();
            pitch.load(numberOfNotes, radius, i);
            basicNotes.Add(pitch);

            //Particle System for the explosion on pause
            ParticleSystem noteFirework = Instantiate(fireworkParticles, pitch.transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
            noteFirework.startColor = pitch.ps.startColor;
            notesFireworks.Add(noteFirework);

            //Particle System for the implosion when unpause
            ParticleSystem noteImplosion = Instantiate(fireworkImplosionParticles, pitch.transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
            noteImplosion.startColor = pitch.ps.startColor;
            notesImplosion.Add(noteImplosion);
            //When start the scene, them implode too
            noteImplosion.Play();
        }
        //Load the audio source
        StartCoroutine(LoadFile(musicPath));
        glowMouseEffect = mouseEffects.GetComponentInChildren<Note>();

    }

    IEnumerator LoadFile(string path)
    {
        WWW www = new WWW("file://" + path);
        print("loading " + path);

        AudioClip clip = www.GetAudioClip(false);
        clip.LoadAudioData();
        while (!clip.isReadyToPlay)
            yield return www;

        print("done loading");

        AudioSource[] vAudioSource = GetComponents<AudioSource>();
        firstTrack = vAudioSource[0];
        secondTrack = vAudioSource[1];

        firstTrack.clip.name = path;
        firstTrack.clip = clip;
        secondTrack.clip = clip;

        firstTrack.volume = 0.0f;
        Debug.Log("First track has volume = 0.0f");
        StartCoroutine(StartFirstTrack());
        StartCoroutine(StartSecondTrack());

        spectrum = new float[1024];
        firstTrack.GetSpectrumData(spectrum, 0, FFTWindow.Hamming);
        lastPitch = 0.0f;
        secondSpectrum = new float[1024];
        secondTrack.GetSpectrumData(secondSpectrum, 0, FFTWindow.Hamming);
        lastSecondPitch = 0.0f;

        countdown = 3;
    }

    public IEnumerator StartSecondTrack()
    {
        for (int i = countdown; i > 0; i--)
        {
            Debug.Log(i);
            menuText.text = i.ToString();
            countdownSound.Play();
            yield return new WaitForSeconds(timeStartSecondTrack / countdown);
        }
        Debug.Log("Playing the Second Track");
        menuText.enabled = false;
        diffTime = Time.time - diffTime;
        secondTrack.Play();

        gameStatus = GameStatus.started;
        //StartCoroutine("GameHasEnded");
    }

    public IEnumerator StartFirstTrack()
    {
        yield return new WaitForSeconds(timeStartSecondTrack - 1.0f);
        Debug.Log("Playing the First Track");
        diffTime = Time.time;
        firstTrack.Play();
    }

    #endregion

    #region Game Update Logic

    void GameUpdate()
    {
        #region Note Sequence Creation
        //Get a future frame, put on a list, analyse this new info
        float pitch = GetFundamentalFrequency(firstTrack);
        float secondPitch = GetFundamentalFrequency(secondTrack);

        frameAtual++;
        if (lastSecondPitch > secondPitch && lastSecondPitch > 0.0025 && frameAtual > jumpFrames)
        {
            GameObject input = Instantiate(notePrefab, Vector3.zero, Quaternion.identity) as GameObject;
            input.GetComponent<Note>().load(numberOfNotes, radius, Note.geti(lastSecondPitch));
            SteeringBehavior inputSB = input.GetComponent<SteeringBehavior>();
            inputSB.seekPosition = Vector3.zero;
            inputSB.wSeek = 1.0f;
            inputSB.maxVelocity = (radius / diffTime);
            sequenceNotes.Add(input);
            frameAtual = 0;

            totalNotes++;
        }
        lastSecondPitch = secondPitch;
        #endregion

        #region Note Position Test and Destruction
        if (sequenceNotes.Count > 0)
        {
            GameObject bolinha;
            bolinha = sequenceNotes[0];

            Vector2 mousePos = new Vector2();
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float angleMouse = ((Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg));
            if (angleMouse < 0)
                angleMouse += 360;

            int i = getIByAngle(angleMouse);
            //Update de mouse color
            glowMouseEffect.changePosition(numberOfNotes, i);

            //Verify if the first note in the sequence is in the destruction range and the mouse is on the right position
            //If so, destroy this note, explode the firework and count as a Score
            if (Vector3.Distance(bolinha.transform.position, Vector3.zero) <= radius / 4 && i == bolinha.GetComponent<Note>().screenAngle)
            {
                sequenceNotes.Remove(bolinha);
                firework.startColor = bolinha.GetComponent<Note>().ps.startColor;
                firework.Play();
                Destroy(bolinha);
                scoreController.addCombo();
                totalPlayedNotes++;
            }
            else
                firework.Stop();
            //If not, d
            destroiBolinhas();
        }
        #endregion

    }

    void GamePausedUpdate()
    {

    }

    void destroiBolinhas()
    {
        List<GameObject> bolinhasDestruir = new List<GameObject>();
        foreach (GameObject bolinha in sequenceNotes)
        {
            if (Vector3.Distance(bolinha.transform.position, Vector3.zero) < radius / 10)
            {
                bolinhasDestruir.Add(bolinha);
            }
        }
        if (bolinhasDestruir.Count > 0)
            scoreController.comboBreak();
        foreach (GameObject bolinha in bolinhasDestruir)
        {
            sequenceNotes.Remove(bolinha);
            Destroy(bolinha);
        }

    }

    float GetFundamentalFrequency(AudioSource audio)
    {
        float fundamentalFrequency = 0.0f;
        float[] data = new float[8192];
        audio.GetSpectrumData(data, 0, FFTWindow.BlackmanHarris);
        float s = 0.0f;
        int i = 0;
        for (int j = 0; j < 8192; j++)
        {
            if (s < data[j])
            {
                s = data[j];
                i = j;
            }
        }
        fundamentalFrequency = s * AudioSettings.outputSampleRate / 8192;
        return fundamentalFrequency;
    }

    void updateInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.P))
        {
            if (gameStatus != GameStatus.paused)
            {
                Debug.Log("PARÔ PARÔ PARÔ");
                gameStatus = GameStatus.paused;
                timePauseStarted = Time.time;
                foreach (Note pitch in basicNotes)
                {
                    pitch.pause();

                }
                foreach (ParticleSystem firework in notesFireworks)
                    firework.Play();
                foreach (GameObject bolinha in sequenceNotes)
                {
                    bolinha.GetComponent<Note>().pause();
                    bolinha.GetComponent<SteeringBehavior>().maxVelocity = 0.0f;
                }
                menuText.enabled = true;
                menuText.text = "Pause";
                firstTrack.Pause();
                secondTrack.Pause();
                glowMouseEffect.ps.startColor = Color.white;
                scoreController.hideCombo();
            }
            else if (gameStatus == GameStatus.paused)
            {
                Debug.Log("Continuaaaaaa");
                foreach (Note pitch in basicNotes)
                    pitch.unpause();
                foreach (GameObject bolinha in sequenceNotes)
                    bolinha.GetComponent<Note>().unpause();
                StartCoroutine(UnpauseCountdown());
            }
        }
    }

    int getIByAngle(float angle)
    {
        float sizeInterval = 360 / (numberOfNotes);
        float minAngleInterval, maxAngleInterval;
        for (int j = 0; j <= numberOfNotes; j++)
        {
            minAngleInterval = ((sizeInterval * j - sizeInterval / 2) + 360) % 360;
            maxAngleInterval = ((sizeInterval * j + sizeInterval / 2) + 360) % 360;
            //Debug.Log(minAngleInterval + ", "+ maxAngleInterval);
            if (minAngleInterval < angle && angle < maxAngleInterval)
                return j;
        }
        return 0;
    }

    IEnumerator UnpauseCountdown()
    {
        for (int i = countdown; i > 0; i--)
        {
            menuText.text = i.ToString();
            countdownSound.Play();
            yield return new WaitForSeconds(timeStartSecondTrack / countdown);
        }
        foreach (GameObject bolinha in sequenceNotes)
        {
            bolinha.GetComponent<SteeringBehavior>().maxVelocity = (radius / diffTime); ;
        }
        firstTrack.UnPause();
        secondTrack.UnPause();
        menuText.enabled = false;
        gameStatus = GameStatus.started;
        timePauseEnd = Time.time;
        totalTimePaused.Add(timePauseStarted - timePauseEnd);
    }

    void GameHasEnded()
    {
        explodeBasicNotesFireworks();
        foreach (Note pitch in basicNotes)
            pitch.pause();
        glowMouseEffect.ps.startColor = Color.white;
        scoreController.showFinalScore(totalNotes, totalPlayedNotes);
        gameStatus = GameStatus.ended;
        //yield return new WaitForSeconds(5.0f);
        //Application.LoadLevel(GameMenu);
    }

    #endregion

    #region Visual Effects
    void explodeBasicNotesFireworks()
    {
        foreach (ParticleSystem firework in notesFireworks)
            firework.Play();
    }

    void implosionBasicNotesFireworks()
    {
        foreach (ParticleSystem implosion in notesImplosion)
            implosion.Play();
    }
    #endregion

}
