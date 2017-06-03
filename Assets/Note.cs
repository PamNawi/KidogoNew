using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum BasicNote { C4, CS4, D4, DS4, E4, F4, FS4, G4, GS4, A4, AS4, B4 };

public class Note : MonoBehaviour
{
    public float screenAngle;
    private BasicNote pitch;
    public float magicNumber;
    SteeringBehavior st;
    public int i;

    public ParticleSystem fireworkPS;
    public ParticleSystem ps;
    void Awake()
    {
        st = GetComponent<SteeringBehavior>();
        ps = GetComponent<ParticleSystem>();
        fireworkPS = GetComponentInChildren<ParticleSystem>();
    }

    public void load(int numberOfNotes, float radius, int i)
    {
        Renderer renderer = GetComponent<Renderer>();
        float angle = i * Mathf.PI * 2 / numberOfNotes;
        transform.position = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
        ps.startColor = Color.HSVToRGB(Mathf.Rad2Deg * angle / 360, 1, 1);
        fireworkPS.startColor = ps.startColor;

        pitch = (BasicNote)i;
        screenAngle = i;
        this.i = i;
        
        switch (i)
        {
            case 0:
                magicNumber = 0.01871f;
                break;
            case 1:
                magicNumber = 0.0173f;
                break;
            case 2:
                magicNumber = 0.0178f;
                break;
            case 3:
                magicNumber = 0.0245f;
                break;
            case 4:
                magicNumber = 0.0252f;
                break;
            case 5:
                magicNumber = 0.0228f;
                break;
            case 6:
                magicNumber = 0.0258f;
                break;
            case 7:
                magicNumber = 0.0246f;
                break;
            case 8:
                magicNumber = 0.0236f;
                break;
            case 9:
                magicNumber = 0.0239f;
                break;
            case 10:
                magicNumber = 0.0395f;
                break;
            case 11:
                magicNumber = 0.0414f;
                break;
        }
    }

    public static int geti(float magicNumber)
    {
        magicNumber = magicNumber / 10;
        if (magicNumber < 0.016)
            return (int)Random.Range((int)BasicNote.A4, (int)BasicNote.GS4);

        if (magicNumber > 0.0185 && magicNumber < 0.0224)
            return (int) BasicNote.C4;

        if (magicNumber > 0.015 && magicNumber < 0.0178 )
            return (int) BasicNote.CS4;

        if (magicNumber > 0.0177 && magicNumber < 0.0186)
            return (int)BasicNote.D4;

        if (magicNumber > 0.0243 && magicNumber < 0.0246)
            return (int)BasicNote.DS4;

        if (magicNumber > 0.0249 && magicNumber < 0.0256 )
            return (int)BasicNote.E4;

        if (magicNumber > 0.0223 && magicNumber < 0.0232)
            return (int)BasicNote.F4;

        if (magicNumber > 0.0255  && magicNumber < 0.0370)
            return (int)BasicNote.FS4;

        if (magicNumber > 0.0245  && magicNumber < 0.0250)
            return (int)BasicNote.G4;

        if (magicNumber > 0.0231  && magicNumber <  0.0238)
            return (int)BasicNote.GS4;

        if (magicNumber > 0.0237  && magicNumber < 0.0244)
            return (int)BasicNote.A4;

        if (magicNumber > 0.0369  && magicNumber < 0.04)
            return (int)BasicNote.AS4;

        if (magicNumber > 0.04 )
            return (int)BasicNote.B4;

        throw new System.Exception("OOOOOPS:" + magicNumber);
    }

    public void changePosition(int numberOfNotes, int i)
    {
        Renderer renderer = GetComponent<Renderer>();
        float angle = i * Mathf.PI * 2 / numberOfNotes;
        ps = GetComponent<ParticleSystem>();
        ps.startColor = Color.HSVToRGB(Mathf.Rad2Deg * angle / 360, 1, 1);
        fireworkPS.startColor = ps.startColor;
        //renderer.material.color = Color.HSVToRGB(Mathf.Rad2Deg * angle / 360, 1, 1);
        // gameObject.GetComponentInChildren<MeshRenderer>().material.color = Color.HSVToRGB(Mathf.Rad2Deg * angle / 360, 1, 1);

        pitch = (BasicNote)i;
        screenAngle = i;
        this.i = i;
    }

    public void pause()
    {
        ps.Stop();
    }

    public void unpause()
    {
        ps.Play();
    }
}

