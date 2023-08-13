using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class MaskIdentification : MonoBehaviour
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

    public TextMesh TextBox;
    public GameObject TheBox;
    
    private bool focused;
    private bool capsLock;
    private bool shift;

    public AudioClip[] SuccessClips;
    public AudioClip[] FailClips;
    public AudioClip KeyboardClip;
    public AudioClip IntroClip;
    public AudioClip OutroClip;

    public static bool playingIntro = false;


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

    int[] Unique = { 0, 0, 0 };

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
        if (!playingIntro)
        {
            Audio.PlaySoundAtTransform(IntroClip.name, transform);
            playingIntro = true;
        }
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
        Logging("You submitted " + Analysis);

        if (Analysis == SeedPacketIdentifier[Unique[Stages]].name)
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

                LightBulbs[Stages - 1].material = TheLights[1];
                SeedPacket.sprite = DefaultSprite;
                Playable = true;
                Toggleable = true;
                Animating1 = false;
            }
        }

        else
        {
            Animating1 = true;
            StrikeIncoming = true;
            AudioClip clip = FailClips[Rnd.Range(0, 3)];
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
            StrikeIncoming = false;
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

            for (int x = 0; x < 3; x++)
            {
                SeedPacket.sprite = SeedPacketIdentifier[Unique[x]];
                yield return new WaitForSecondsRealtime(0.2f);
                currentTime += .2f;
            }

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
        ModuleSolved = true;
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

    bool ActiveBorder = false;
    bool Animating1 = false;
    bool Intro = true;
    bool StrikeIncoming = false;

    IEnumerator ProcessTwitchCommand(string Command)
    {
        if (Command.EqualsIgnoreCase("border"))
        {
            if (Intro || ActiveBorder || Animating1 || Enterable)
            {
                yield return "sendtochaterror The border cannot be pressed right now!";
                yield break;
            }
            yield return null;
            Border.OnInteract();
            yield break;
        }
        if (Command.EqualsIgnoreCase("submit"))
        {
            if (Intro || ActiveBorder || Animating1 || !Enterable)
            {
                yield return "sendtochaterror The enter key cannot be pressed right now!";
                yield break;
            }
            yield return null;
            Keyboard[37].OnInteract();
            yield break;
        }
        string[] parameters = Command.Split(' ');
        if (parameters[0].EqualsIgnoreCase("backspace"))
        {
            if (parameters.Length > 2)
            {
                yield return "sendtochaterror Too many parameters!";
                yield break;
            }
            else if (parameters.Length == 1)
            {
                if (Intro || ActiveBorder || Animating1 || !Enterable)
                {
                    yield return "sendtochaterror The backspace key cannot be pressed right now!";
                    yield break;
                }
                if (TextBox.text.Length == 0)
                {
                    yield return "sendtochaterror There are no typed characters in the text box!";
                    yield break;
                }
                yield return null;
                Keyboard[59].OnInteract();
            }
            else
            {
                int temp;
                if (!int.TryParse(parameters[1], out temp) || temp <= 0)
                {
                    yield return "sendtochaterror!f The specified number of times to press the backspace key '" + parameters[1] + "' is invalid!";
                    yield break;
                }
                if (Intro || ActiveBorder || Animating1 || !Enterable)
                {
                    yield return "sendtochaterror The backspace key cannot be pressed right now!";
                    yield break;
                }
                if (temp > TextBox.text.Length)
                {
                    if (TextBox.text.Length == 0)
                        yield return "sendtochaterror There are no typed characters in the text box!";
                    else if (TextBox.text.Length == 1)
                        yield return "sendtochaterror You can only remove at most 1 character!";
                    else
                        yield return "sendtochaterror You can only remove at most " + TextBox.text.Length + " characters!";
                    yield break;
                }
                yield return null;
                for (int i = 0; i < temp; i++)
                {
                    Keyboard[59].OnInteract();
                    yield return new WaitForSeconds(0.05f);
                }
            }
            yield break;
        }
        if (parameters[0].EqualsIgnoreCase("type"))
        {
            if (parameters.Length == 1)
                yield return "sendtochaterror Please specify the characters you wish to type!";
            else
            {
                if (Intro || ActiveBorder || Animating1 || !Enterable)
                {
                    yield return "sendtochaterror The keyboard keys cannot be pressed right now!";
                    yield break;
                }
                string charsToType = Command.Substring(5);
                for (int i = 0; i < charsToType.Length; i++)
                {
                    bool good = false;
                    if (charsToType[i] == '"' || charsToType[i] == '\'')
                        good = true;
                    for (int j = 0; j < Keyboard.Length; j++)
                    {
                        if (Keyboard[j].GetComponentInChildren<TextMesh>() != null && Keyboard[j].GetComponentInChildren<TextMesh>().text.Length == 1)
                        {
                            if (Keyboard[j].GetComponentInChildren<TextMesh>().text.ToUpper()[0] == charsToType[i] || Keyboard[j].GetComponentInChildren<TextMesh>().text.ToLower()[0] == charsToType[i])
                            {
                                good = true;
                                break;
                            }
                        }
                    }
                    if (!good)
                    {
                        yield return "sendtochaterror!f The specified character '" + charsToType[i] + "' cannot be typed!";
                        yield break;
                    }
                }
                yield return null;
                for (int i = 0; i < charsToType.Length; i++)
                {
                    if (TextBox.text.Length < 30)
                    {
                        if (charsToType[i] == '"')
                        {
                            if (!capsLock && !shift)
                            {
                                if (i != charsToType.Length - 1 && "ABCDEFGHIJKLMNOPQRSTUVWXYZ\"".Contains(charsToType[i + 1]))
                                {
                                    Keyboard[36].OnInteract();
                                    yield return new WaitForSeconds(0.05f);
                                }
                                else
                                {
                                    Keyboard[38].OnInteract();
                                    yield return new WaitForSeconds(0.05f);
                                }
                            }
                            Keyboard[26].OnInteract();
                        }
                        else if (charsToType[i] == '\'')
                        {
                            if (capsLock)
                            {
                                Keyboard[36].OnInteract();
                                yield return new WaitForSeconds(0.05f);
                            }
                            else if (shift)
                            {
                                Keyboard[38].OnInteract();
                                yield return new WaitForSeconds(0.05f);
                            }
                            Keyboard[26].OnInteract();
                        }
                        else
                        {
                            for (int j = 0; j < Keyboard.Length; j++)
                            {
                                if (Keyboard[j].GetComponentInChildren<TextMesh>() != null && Keyboard[j].GetComponentInChildren<TextMesh>().text.Length == 1)
                                {
                                    if (Keyboard[j].GetComponentInChildren<TextMesh>().text.ToLower()[0] == charsToType[i])
                                    {
                                        if (capsLock)
                                        {
                                            Keyboard[36].OnInteract();
                                            yield return new WaitForSeconds(0.05f);
                                        }
                                        else if (shift)
                                        {
                                            Keyboard[38].OnInteract();
                                            yield return new WaitForSeconds(0.05f);
                                        }
                                        Keyboard[j].OnInteract();
                                        break;
                                    }
                                    if (Keyboard[j].GetComponentInChildren<TextMesh>().text.ToUpper()[0] == charsToType[i])
                                    {
                                        if (!capsLock && !shift)
                                        {
                                            if (i != charsToType.Length - 1 && "ABCDEFGHIJKLMNOPQRSTUVWXYZ\"".Contains(charsToType[i + 1]))
                                            {
                                                Keyboard[36].OnInteract();
                                                yield return new WaitForSeconds(0.05f);
                                            }
                                            else
                                            {
                                                Keyboard[38].OnInteract();
                                                yield return new WaitForSeconds(0.05f);
                                            }
                                        }
                                        Keyboard[j].OnInteract();
                                        break;
                                    }
                                }
                            }
                        }
                        yield return new WaitForSeconds(0.05f);
                    }
                    else
                    {
                        yield return "sendtochaterror Typing halted due to the text box being full.";
                        yield break;
                    }
                }
            }
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        if (StrikeIncoming)
        {
            StopAllCoroutines();
            LightBulbs[0].material = TheLights[1];
            LightBulbs[1].material = TheLights[1];
            LightBulbs[2].material = TheLights[1];
            SeedPacket.sprite = SeedPacketIdentifier[Unique[Rnd.Range(0, 3)]];
            Module.HandlePass();
            yield break;
        }
        while (Intro) yield return true;
        int start = Stages;
        for (int i = start; i < 3; i++)
        {
            while (Animating1) yield return true;
            if (!Enterable)
            {
                if (!ActiveBorder)
                    Border.OnInteract();
                while (ActiveBorder) yield return true;
            }
            while (!SeedPacketIdentifier[Unique[Stages]].name.StartsWith(TextBox.text))
            {
                Keyboard[59].OnInteract();
                yield return new WaitForSeconds(0.05f);
            }
            yield return ProcessTwitchCommand("type " + SeedPacketIdentifier[Unique[Stages]].name.Substring(TextBox.text.Length));
            Keyboard[37].OnInteract();
        }
        while (!ModuleSolved) yield return true;
    }
}
