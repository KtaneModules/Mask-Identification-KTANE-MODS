using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using UnityEditor;
using UnityEngine.SocialPlatforms;
using Rnd = UnityEngine.Random;
using UnityEngine.UI;
using UnityEditorInternal;

public class CustomerIdentificationScript : MonoBehaviour
{
	public KMAudio Audio;
	public KMBombInfo Bomb;
	public KMBombModule Module;

	public KMSelectable[] Keyboard;
	public KMSelectable Border;
	
	public MeshRenderer[] BorderAndTile;
	public SpriteRenderer SeedPacket;
	public Sprite[] SeedPacketIdentifier;
	public Sprite DefaultSprite;
	public Material[] ImageLighting;

    public MeshRenderer[] LightBulbs;
	public Material[] TheLights;
	
	public TextMesh[] Text;
	public TextMesh TextBox;
	public GameObject TheBox;
	public SpriteRenderer AnotherAnotherShower;
	public Sprite ThumbsUp;

	private bool focused;
    private bool capsLock;
    private bool shift;

    public GameObject[] IShow;

    public AudioClip[] SuccessClips;
    public AudioClip[] FailClips;
    public AudioClip   KeyboardClip;
	public AudioClip   IntroClip;
    public AudioClip   OutroClip;


    private bool[] successArr;

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

			Audio.PlaySoundAtTransform(KeyboardClip.name, transform);

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
                    Keyboard[i].OnInteract();
                }
            }
        }
    }
	
	void Introduction()
	{
		StartCoroutine(Reintroduction());
    }

    void UniquePlay()
	{

		for (int c = 0; c < Unique.Count(); c++)
        {
			Unique[c] = Rnd.Range(0, SeedPacketIdentifier.Count());
        }
		
		if (Unique[0] == Unique[1] || Unique[0] == Unique[2] || Unique[1] == Unique[2])
		{
			UniquePlay();
		}
		
	}
	
	IEnumerator Reintroduction()
	{
		Intro = true;
		Audio.PlaySoundAtTransform(IntroClip.name, transform);
        successArr = new bool[] { false, false, false };

        yield return new WaitForSecondsRealtime(IntroClip.length);

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
				AudioClip audioClip = GetSuccessClip();
				Audio.PlaySoundAtTransform(OutroClip.name, transform);
                StartCoroutine(RoulleteToWin(OutroClip.length));
				StartCoroutine(SolveAnimation(OutroClip.length));
			}
			
			else
			{
				Animating1 = true;
				AudioClip clip = GetSuccessClip();
				Audio.PlaySoundAtTransform(clip.name, transform);
				
				yield return new WaitForSecondsRealtime(clip.length);
				
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
			AudioClip clip = FailClips[Rnd.Range(0,3)];
			Audio.PlaySoundAtTransform(clip.name, transform);

            Enterable = false;
			LightBulbs[0].material = TheLights[2];
			LightBulbs[1].material = TheLights[2];
			LightBulbs[2].material = TheLights[2];

			yield return new WaitForSecondsRealtime(clip.length);
			
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
	
	IEnumerator RoulleteToWin(float length)
	{
		float currentTime = 0;
		while (currentTime < length)
		{
            currentTime += Time.deltaTime;

			Debug.LogFormat("{0} | {1}", currentTime, length);

            for (int x = 0; x < 3; x++)
			{
                SeedPacket.sprite = SeedPacketIdentifier[Unique[x]];
				yield return new WaitForSecondsRealtime(0.2f);
				currentTime += .2f;
			}

			Debug.Log(currentTime);
            yield return null;
		}

	}


	IEnumerator SolveAnimation(float length)
	{
        float currentTime = 0;
        while (currentTime < length)
        {
            currentTime += Time.deltaTime;

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

			currentTime += .06f;
            yield return null;
        }
        LightBulbs[0].material = TheLights[1];
        LightBulbs[1].material = TheLights[1];
        LightBulbs[2].material = TheLights[1];
        Logging("Module Solved");
        Module.HandlePass();
        Animating1 = false;
    }

    AudioClip GetSuccessClip() 
	{
		int index;

		do
		{
			index = Rnd.Range(0, 3);

		} while (successArr[index]);

		if (!successArr.Any(x => !x))
		{
			successArr = new bool[] { false, false, false };
		}

		return SuccessClips[index];
	}
    private void Logging(string s)
    {
        Debug.LogFormat("[Mask Identification #{0}] {1}", moduleId, s);
    }

	//twitch plays
#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"Use `!{0} border` to reveal the mask. Use `!{0} backspace 2` to remove the last two characters typed. `!{0} type [mask name]` to type the name of the mask. Use `!{0} submit` to press enter.";
    #pragma warning restore 414
	
	int StartingNumber = 0;
	bool Intro = false;
	bool ActiveBorder = false;
	bool Animating1 = false;
	string Current = "";

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use !{0} to do something.";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string Command)
    {
        yield return null;
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
    }
}
	
