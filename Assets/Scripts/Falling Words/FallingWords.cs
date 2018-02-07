﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FallingWords : MonoBehaviour {

    public GameObject validAreas;
    public TextAsset t;
    string[] words;
    public Vector2 spawnRange;
    public float spawnHeight;
    public float destroyHeight;
   
    public TextMesh textPrefab;
    int wordIndex;
    float nextSpawnTime;
    BoxCollider2D[] topScreens;
    Bounds[] validBounds;
    public Vector2 speedMinMax;
    public Vector2 delayMinMax;
    bool done;
    List<Word> activeWords;
    List<Word> potentialWordMatches;
    string inputString;
    bool allWordsSpawned;

	// Use this for initialization
	void Start () {
        activeWords = new List<Word>();
        potentialWordMatches = new List<Word>();

        words = t.text.Split(',');
        //Utility.Shuffle(words);
        for (int i = 0; i < words.Length; i++)
        {
            words[i] = words[i].Trim().ToLower();
        }
        topScreens = FindObjectOfType<ScreenAreas>().topScreens;
        validBounds = validAreas.GetComponents<BoxCollider2D>().Select(v=>v.bounds).ToArray();
	}
	
	// Update is called once per frame
	void Update () {
		float completionPercent = wordIndex / ((float)words.Length - 1);
        float speed = Mathf.Lerp(speedMinMax.x, speedMinMax.y, completionPercent);

        if (wordIndex < words.Length)
        {
           
            if ((Time.time > nextSpawnTime && wordIndex!=words.Length-1 && wordIndex != 1) || activeWords.Count==0)
            {

                TextMesh mesh = Instantiate<TextMesh>(textPrefab);
                mesh.text = words[wordIndex];
                PositionWord(mesh);
                activeWords.Add(new Word(words[wordIndex], mesh));

                wordIndex++;
                nextSpawnTime = Time.time + Mathf.Lerp(delayMinMax.x,delayMinMax.y,completionPercent);
                potentialWordMatches = new List<Word>(activeWords);
            }
        }
        else
        {
            allWordsSpawned = true;
        }

        for (int i = activeWords.Count-1; i >= 0; i--)
        {
   
            Word word = activeWords[i];
            word.mesh.transform.position += Vector3.down * Time.deltaTime * speed;

			if (word.mesh.transform.position.y < destroyHeight)
			{
				OnWordFailed();
                activeWords.Remove(word);
				if (potentialWordMatches.Contains(word))
				{
					potentialWordMatches.Remove(word);
					if (potentialWordMatches.Count == 0)
					{
						inputString = "";
					}
				}
                Destroy(word.mesh.gameObject);
			}
        }


        HandleInput();

        if (activeWords.Count == 0 && allWordsSpawned && !done)
        {
            OnComplete();
        }
	}

    void PositionWord(TextMesh mesh)
    {
        float width = mesh.GetComponent<MeshRenderer>().bounds.size.x;

        int randIndex = Random.Range(0, validBounds.Length);
        Bounds screen = new Bounds();
        for (int i = 0; i < validBounds.Length; i++)
        {
            screen = validBounds[(randIndex+i)%validBounds.Length];
            if (screen.size.x > width)
            {
                break;
            }
        }


		Vector2 minMaxX = new Vector2(screen.min.x, screen.max.x);

		float spawnX = Random.Range(minMaxX.x, minMaxX.y - width);
		mesh.transform.parent = transform;
		mesh.transform.position = new Vector3(spawnX, spawnHeight);
		mesh.transform.localEulerAngles = Vector3.up * 180;

	}

    void OnComplete()
    {
        done = true;
    }

    void OnWordFailed()
    {

    }

    void OnWordSucceeded()
    {

    }

    void HandleInput()
    {
        bool hasChanged = false;

        foreach (char c in Input.inputString.ToLower())
        {
            bool newInputStringValid = false;
            string newInputString = inputString + c;

            foreach (Word word in activeWords)
            {
                if (word.word.StartsWith(newInputString,System.StringComparison.CurrentCulture))
                {
                    newInputStringValid = true;
                    hasChanged = true;
                }
            }

            if (newInputStringValid)
            {
                inputString = newInputString;
            }
            else
            {
                break;
            }
        }

        if (hasChanged)
        {
			for (int i = activeWords.Count - 1; i >= 0; i--)
			{
				Word word = activeWords[i];
                if (word.word == inputString)
                {
                    OnWordSucceeded();
                    inputString = "";
					activeWords.RemoveAt(i);
					Destroy(word.mesh.gameObject);
                }
                word.UpdateColour(inputString);
             
            }
        }
    }


    class Word
    {
        public string word;
        public TextMesh mesh;

        const string highlightCol = "red";

        public Word(string word, TextMesh mesh)
        {
            this.word = word;
            this.mesh = mesh;
        }

        public void UpdateColour(string inputString)
        {
           
            if (word.StartsWith(inputString, System.StringComparison.CurrentCulture)) {
                mesh.text = "<color=" + highlightCol + ">" + inputString + "</color>";

                if (word != inputString)
                {
                    mesh.text += word.Substring(inputString.Length, word.Length - inputString.Length);
                }
            }
            else
            {
                mesh.text = word;
            }

        }
    }
}