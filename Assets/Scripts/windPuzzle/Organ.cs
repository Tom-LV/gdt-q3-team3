using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Audio;

public class Organ : MonoBehaviour
{
    [SerializeField] private List<AudioClip> notes;
    [SerializeField] private float delay;

    private float timer = 0;
    private bool audioPlayed = true;

    private AudioSource audioSource;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (!audioPlayed && timer >= delay)
        {
            audioSource.Play();
            audioPlayed = true;
        }
    }

    public void PlayNote(int note)
    {
        if (note < 0 || note >= notes.Count)
        {
            Debug.LogError("Note out of range");
            return;
        }

        audioSource.clip = notes[note];
        
        timer = 0;
        audioPlayed = false;
    }
}
