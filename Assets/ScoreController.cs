using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreController : MonoBehaviour {
    //public AudioSource starSound;
    int combo = 0;
    Text comboUIText;
    int maxCombo = 0;

    public GameObject endScoreWindow;
    public Text maxScoreText;
    public Text maxComboText;
    public List<GameObject> stars;

    public Sprite starOn;
    // Use this for initialization
    void Awake () {
        comboUIText = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
        comboUIText.text = combo.ToString() + "\n Combo";
	}

    public void showCombo() {
        comboUIText.enabled = true;
    }
    public void hideCombo() {
        comboUIText.enabled = false;
    }

    public void addCombo() {
        combo += 1;
        if (combo > maxCombo)
            maxCombo = combo;
        if (!comboUIText.enabled)
            showCombo();
        else
            //restart combo ui animation
            return;
    }
    public void comboBreak()
    {
        combo = 0;
        hideCombo();
    }

    public void showFinalScore(int totalNotes, int totalPlayedNotes) {
        comboUIText.enabled = false;
        endScoreWindow.active = true;
        float totalPlayedNotesPercentage = (float) totalPlayedNotes / (float) totalNotes * 100f;
        Debug.Log(totalPlayedNotesPercentage);
        for (int i = 0; i <stars.Count; i++)
        {
            if(i * 100/stars.Count < totalPlayedNotesPercentage)
            {
                stars[i].GetComponent<ParticleSystem>().Play();
                stars[i].GetComponent<Image>().sprite = starOn;
            }
        }
        maxScoreText.text = totalPlayedNotesPercentage.ToString("0.00") + "%";
        maxComboText.text = "Max combo: " + maxCombo.ToString();

    }
}
