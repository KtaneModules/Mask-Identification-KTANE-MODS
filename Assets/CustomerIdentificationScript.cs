using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using UnityEditor;

public class CustomerIdentificationScript : MonoBehaviour
{
	public KMAudio Audio;
	public KMBombInfo Bomb;
	public KMBombModule Module;
	public AudioSource SecondMusic;

	public KMSelectable[] Keyboard;
	public KMSelectable Border;
	
	public MeshRenderer[] BorderAndTile;
	public Material[] Chapters;
	public SpriteRenderer SeedPacket;
	public Sprite[] SeedPacketIdentifier;
	public Sprite DefaultSprite;
	public Sprite DeathSprite;
	public Material[] ImageLighting;
	
	public MeshRenderer[] LightBulbs;
	public Material[] TheLights;
	
	public TextMesh[] Text;
	public TextMesh TextBox;
	public GameObject TheBox;
	public SpriteRenderer AnotherShower;
	public SpriteRenderer AnotherAnotherShower;
	public Sprite ThumbsUp;

	private bool focused;
    private bool capsLock;
    private bool shift;

    public GameObject[] IShow;
	
	bool Shifted = false;
	
	public AudioClip[] NotBuffer;
	
	string[][] ChangedText = new string[2][]{
		new string[47] {"`", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "-", "=", "q", "w", "e", "r", "t", "y", "u", "i", "o", "p", "[", "]", "\\", "a", "s", "d", "f", "g", "h", "j", "k", "l", ";", "'", "z", "x", "c", "v", "b", "n", "m", ",", ".", "/"},
		new string[47] {"~", "!", "@", "#", "$", "%", "^", "&", "*", "(", ")", "_", "+", "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P", "{", "}", "|", "A", "S", "D", "F", "G", "H", "J", "K", "L", ":", "\"", "Z", "X", "C", "V", "B", "N", "M", "<", ">", "?"}
	};

    private KeyCode[] TypableKeys =
    {
        KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T, KeyCode.Y, KeyCode.U, KeyCode.I, KeyCode.O, KeyCode.P, 
		KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.J, KeyCode.K, KeyCode.L, 
		KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V, KeyCode.B, KeyCode.N, KeyCode.M,
        KeyCode.Quote, KeyCode.LeftBracket, KeyCode.RightBracket, KeyCode.Backslash, KeyCode.Semicolon, KeyCode.BackQuote, KeyCode.Comma, KeyCode.Period, KeyCode.Slash, KeyCode.Tab, KeyCode.CapsLock, KeyCode.Return,
        KeyCode.LeftShift, KeyCode.RightShift, KeyCode.LeftControl, KeyCode.RightControl, KeyCode.LeftWindows, KeyCode.RightWindows, KeyCode.LeftAlt, KeyCode.RightAlt, KeyCode.Menu, KeyCode.Minus, KeyCode.Equals,
        KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0, KeyCode.Backspace, KeyCode.Space
    };


    int[] Unique = {0, 0, 0};
	
	bool Playable = false;
	bool Enterable = false;
	bool Toggleable = true;
	int Stages = 0;


	
	int ChapterNumber;
	
    static int moduleIdCounter = 1;
    int moduleId;
    private bool ModuleSolved;

	void Awake()
	{
		moduleId = moduleIdCounter++;
		for (int i = 0; i < Keyboard.Count(); i++)
		{
			int dummy = i;
            Keyboard[i].OnInteract += delegate () { KeyPress(Keyboard[dummy]); return false; };
        }
		
		Border.OnInteract += delegate () { PressBorder(); return false; };

        GetComponent<KMSelectable>().OnFocus += delegate () { focused = true; };
        GetComponent<KMSelectable>().OnDefocus += delegate () { focused = false; };

        if (Application.isEditor)
            focused = true;
    }

	void KeyPress(KMSelectable button)
	{
		if (Playable && Enterable)
		{
            button.AddInteractionPunch(0.1f);

			//play keyboard sound

			string buttonLabel = "";

            try
			{ 
	            buttonLabel = button.GetComponentInChildren<TextMesh>().text;
            }

			catch 
			{ 
				
			}


            if (new string[] { "Menu", "Alt", "Tab" }.Contains(buttonLabel))
			{
				return;
			}

			//not a windows key
			if (button.GetComponentInChildren<SpriteRenderer>() != null)
			{
				return;
			}

            if (buttonLabel == "Shift")
            {
                shift = !shift;
                DoShift();
            }
            else if (buttonLabel == "Caps Lock")
            {
                capsLock = !capsLock;
                DoShift();
            }
            else
            {
                if (buttonLabel == "Backspace")
                {
                    if (TextBox.text.Length != 0)
                        TextBox.text = TextBox.text.Substring(0, TextBox.text.Length - 1);
                }
                else if (TextBox.text.Length >= 30)
                    return;
                else if (buttonLabel == "Enter")
                    PressEnter();
                else TextBox.text += button.GetComponentInChildren<TextMesh>().text;
                shift = false;
                DoShift();
            }
        }
    }

    void DoShift()
    {
        for (int i = 0; i < 26; i++)
        {
            Keyboard[i].GetComponentInChildren<TextMesh>().text = (shift || capsLock) ?
                Keyboard[i].GetComponentInChildren<TextMesh>().text.ToUpper() : Keyboard[i].GetComponentInChildren<TextMesh>().text.ToLower();
        }

        Keyboard[26].GetComponentInChildren<TextMesh>().text = (shift || capsLock) ? "\"" : "'";
    }

    void Start()
	{
		this.GetComponent<KMSelectable>().UpdateChildren();
		UniquePlay();
		Module.OnActivate += Introduction;
	}

	void Update()
	{
        if (focused && Enterable)
        {
            for (int i = 0; i < TypableKeys.Count(); i++)
            {
                if (Input.GetKeyDown(TypableKeys[i]))
                {
                    Debug.Log(i + " " + TypableKeys[i].ToString());
                    Keyboard[i].OnInteract();
                }
            }
        }
    }
	
	void Introduction()
	{
		StartCoroutine(Reintroduction());
		StartCoroutine(StartFade(NotBuffer[0].length, 1, 0));

    }

    void UniquePlay()
	{
		for (int c = 0; c < Unique.Count(); c++)
        {
            Unique[c] = UnityEngine.Random.Range(0, SeedPacketIdentifier.Count());
        }
		
		if (Unique[0] == Unique[1] || Unique[0] == Unique[2] || Unique[1] == Unique[2])
		{
			UniquePlay();
		}
	}
	
	IEnumerator Reintroduction()
	{
		Intro = true;

		SecondMusic.clip = NotBuffer[0];
		SecondMusic.Play();
        while (SecondMusic.isPlaying)
		{
			yield return new WaitForSecondsRealtime(0.01f);
		}
		Playable = true;
		Intro = false;
	}
	
	void PressBorder()
	{
		Border.AddInteractionPunch(.2f);
		if (Playable && Toggleable)
		{
			StartCoroutine(PlayTheQueue());
		}
	}
	
	void PressEnter()
	{
		Keyboard[37].AddInteractionPunch(.2f);
		Audio.PlaySoundAtTransform(NotBuffer[1].name, transform);
		if (Playable && Enterable)
		{
			StartCoroutine(TheCorrect());
		}
	}
	
	IEnumerator PlayTheQueue()
	{
		Toggleable = false;
        ActiveBorder = true;
		Playable = false;

        Logging("The mask is called " + SeedPacketIdentifier[Unique[Stages]].name);
		SeedPacket.sprite = SeedPacketIdentifier[Unique[Stages]];
		SeedPacket.material = ImageLighting[1];
		yield return new WaitForSecondsRealtime(7.5f);
		SeedPacket.sprite = DefaultSprite;
		SeedPacket.material = ImageLighting[0];
		Playable = true;
        ActiveBorder = false;
		Enterable = true;
    }

    IEnumerator TheCorrect()
	{
		string Analysis = TextBox.text;
		TextBox.text = "";
		Logging("You submitted " +  Analysis);

		if (Analysis  == SeedPacketIdentifier[Unique[Stages]].name)
		{
			Stages++;
			Playable = false;
			Enterable = false;
			if (Stages == 3)
			{
				Animating1 = true;
				SecondMusic.clip = NotBuffer[8];
				SecondMusic.Play();
                StartCoroutine(RoulleteToWin());
				while (SecondMusic.isPlaying)
				{
					LightBulbs[0].material = TheLights[0];
					LightBulbs[1].material = TheLights[0];
					LightBulbs[2].material = TheLights[1];
					yield return new WaitForSecondsRealtime(0.02f);
					LightBulbs[0].material = TheLights[0];
					LightBulbs[1].material = TheLights[1];
					LightBulbs[2].material = TheLights[0];
					yield return new WaitForSecondsRealtime(0.02f);
					LightBulbs[0].material = TheLights[1];
					LightBulbs[1].material = TheLights[0];
					LightBulbs[2].material = TheLights[0];
					yield return new WaitForSecondsRealtime(0.02f);
				}
				LightBulbs[0].material = TheLights[1];
				LightBulbs[1].material = TheLights[1];
				LightBulbs[2].material = TheLights[1];
                Logging("Module Solved");
                Module.HandlePass();
				Animating1 = false;
			}
			
			else
			{
				Animating1 = true;
				AnotherShower.sprite = SeedPacketIdentifier[Unique[Stages-1]];
				int Decider = UnityEngine.Random.Range(0,2); if (Decider == 1) SecondMusic.clip = NotBuffer[2];  else SecondMusic.clip = NotBuffer[4];
				SecondMusic.Play();
				while (SecondMusic.isPlaying)
				{
					yield return new WaitForSecondsRealtime(0.075f);
				}
				LightBulbs[Stages-1].material = TheLights[1];
				SeedPacket.sprite = DefaultSprite;
				Playable = true;
				Toggleable = true;
				Animating1 = false;
			}
		}
			
		else
		{
			Animating1 = true;
			SecondMusic.clip = NotBuffer[5 + UnityEngine.Random.Range(0, 3)];
			SecondMusic.Play();
			Enterable = false;
			LightBulbs[0].material = TheLights[2];
			LightBulbs[1].material = TheLights[2];
			LightBulbs[2].material = TheLights[2];
			while (SecondMusic.isPlaying)
			{
				yield return new WaitForSecondsRealtime(0.075f);
			}
			SeedPacket.sprite = DefaultSprite;
			LightBulbs[0].material = TheLights[0];
			LightBulbs[1].material = TheLights[0];
			LightBulbs[2].material = TheLights[0];
			Playable = true;
			Toggleable = true;
			Animating1 = false;
			Stages = 0;
            Logging("Strike! Module will now reset");
            Module.HandleStrike();
			UniquePlay();
		}
	}
	
	IEnumerator RoulleteToWin()
	{
		while (SecondMusic.isPlaying)
		{
			for (int x = 0; x < 3; x++)
			{
				AnotherShower.sprite = SeedPacketIdentifier[Unique[x]];
				yield return new WaitForSecondsRealtime(0.2f);
			}
		}
	}

    public IEnumerator StartFade(float duration, float startVolumne, float targetVolume)
    {
        float currentTime = 0;
        SecondMusic.volume = startVolumne;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            SecondMusic.volume = Mathf.Lerp(startVolumne, targetVolume, currentTime / duration);
            yield return null;
        }

        yield break;
    }

    private void Logging(string s)
    {
        Debug.LogFormat("[Mask Identification #{0}] {1}", moduleId, s);
    }

	//twitch plays
#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"Use !{0} submit [mask name] to submit the name of the mask. Use !{0} start to just press enter.";
    #pragma warning restore 414
	
	int StartingNumber = 0;
	bool Intro = false;
	bool ActiveBorder = false;
	bool Animating1 = false;
	string Current = "";

    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.Trim();
        List<string> parameters = command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        char[] keyLetters = Keyboard.Select(x => x.GetComponentInChildren<TextMesh>().text.ToUpper()[0]).Concat(" \"").ToArray();

        if (command.Equals("start", StringComparison.InvariantCultureIgnoreCase))
        {
            yield return null;
            Keyboard[37].OnInteract();
        }
        else if (parameters.First().Equals("submit", StringComparison.InvariantCultureIgnoreCase) && parameters.Skip(1).Join("").All(x => keyLetters.Contains(char.ToUpper(x))))
        {
            yield return null;
            while (TextBox.text.Length != 0)
            {
                Keyboard[59].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
            if (capsLock)
                Keyboard[36].OnInteract();
            foreach (char letter in parameters.Skip(1).Join())
            {
                if ("ABCDEFGHIJKLMNOPQRSTUVWXYZ\"".Contains(letter) ^ shift)
                {
                    Keyboard[38].OnInteract();
                    yield return new WaitForSeconds(0.1f);
                }
                if (letter == ' ')
                    Keyboard[60].OnInteract();
                else if (letter == '"')
                    Keyboard[26].OnInteract();
                else Keyboard[Array.IndexOf(keyLetters, char.ToUpperInvariant(letter))].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }

            Keyboard[37].OnInteract();
            
			if (ModuleSolved && Stages > 3)
                yield return "awardpointsonsolve " + (Stages - 3);
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        char[] keyLetters = Keyboard.Select(x => x.GetComponentInChildren<TextMesh>().text.ToUpper()[0]).Concat(" ").ToArray();
        while (!ModuleSolved)
        {
			for (int i = 0; i < 3; i++)
			{
				PressBorder();

				while (!Enterable)
				{
                    yield return null;
                }

				string answer = SeedPacketIdentifier[Unique[Stages]].name;

				foreach (char c in answer)
				{
                    if("ABCDEFGHIJKLMNOPQRSTUVWXYZ\"".Contains(c) ^ shift)
                {
                        Keyboard[38].OnInteract();
                        yield return new WaitForSeconds(c);
                    }
                    if (c  == ' ')
                        Keyboard[60].OnInteract();
                    else Keyboard[Array.IndexOf(keyLetters, char.ToUpperInvariant(c))].OnInteract();
                }
            }

            Keyboard[37].OnInteract();
        }

		while (!ModuleSolved) 
		{
            yield return null;
		}
    }


}
	
