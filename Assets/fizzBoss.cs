using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class fizzBoss : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMAudio Audio;
    public static string[] ignoredModules = null;
    public KMSelectable[] buttons;
    public TextMesh[] displayTexts;
    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool ModuleSolved;

    List<int> stages = new List<int>();
    int currentStage = 0;
    bool submitTime;
    int ModCount = 0;
    int SolveCount = 0;
    bool NoModsLeft;
    int rollingTotal = 0;
    List<int> primes = new List<int>();
    List<string> solution = new List<string>();
    List<string> phrases = new List<string>();
    List<string> input = new List<string>();
    int stageNumber = 0;
    bool buttonPressed;

    void Awake () {
        ModuleId = ModuleIdCounter++;
        GetComponent<KMBombModule>().OnActivate += Activate;
        /*
        foreach (KMSelectable object in keypad) {
            object.OnInteract += delegate () { keypadPress(object); return false; };
        }
        */

        //button.OnInteract += delegate () { buttonPress(); return false; };
        if(ignoredModules == null) {
            ignoredModules = GetComponent<KMBossModule>().GetIgnoredModules("FizzBoss", new string[] {
                "14",
                "42",
                "501",
                "A>N<D",
                "Bamboozling Time Keeper",
                "Black Arrows",
                "Brainf---",
                "The Board Walk",
                "Busy Beaver",
                "Don't Touch Anything",
                "FizzBoss",
                "Floor Lights",
                "Forget Any Color",
                "Forget Enigma",
                "Forget Ligma",
                "Forget Everything",
                "Forget Infinity",
                "Forget It Not",
                "Forget Maze Not",
                "Forget Me Later",
                "Forget Me Not",
                "Forget Perspective",
                "Forget The Colors",
                "Forget Them All",
                "Forget This",
                "Forget Us Not",
                "Iconic",
                "Keypad Directionality",
                "Kugelblitz",
                "Multitask",
                "OmegaDestroyer",
                "OmegaForest",
                "Organization",
                "Password Destroyer",
                "Purgatory",
                "Reporting Anomalies",
                "RPS Judging",
                "Security Council",
                "Shoddy Chess",
                "Simon Forgets",
                "Simon's Stages",
                "Souvenir",
                "Speech Jammer",
                "Tallordered Keys",
                "The Time Keeper",
                "Timing is Everything",
                "The Troll",
                "Turn The Key",
                "The Twin",
                "Übermodule",
                "Ultimate Custom Night",
                "The Very Annoying Button",
                "Whiteout"
            });
        }

    }

    void OnDestroy () { //Shit you need to do when the bomb ends
      
    }

    void Activate () { //Shit that should happen when the bomb arrives (factory)/Lights turn on

    }

    void Start () { //Shit
        for(int i = 0; i < 7; i++) {
            buttons[i].transform.Find("buttonText").GetComponent<TextMesh>().color = Color.gray;
        }
        int StageOne = Rnd.Range(100, 1000);
        //int StageOne = 2239;
        //phrases table variables
        string[] letters = { "f", "b", "z", "i", "u" };
        string[] doubleLetters = { "ff", "bb", "zz", "ii", "uu" };
        for(int b=0; b<5; b++) {
            for (int m = 0; m < 5; m++) {
                for (int e = 0; e < 5; e++) {
                    phrases.Add(letters[b] + letters[m] + doubleLetters[e]);
                }
            }
        }
        primes = generatePrimesNaive(702);
        //other calculation variables
        currentStage = StageOne;
        stages.Add(currentStage);
        rollingTotal += currentStage;
        
        //Updating Display
        displayTexts[0].text = StageOne.ToString();
        displayTexts[1].text = "";

        //Debug.LogFormat("[FizzBoss #{0}]", ModuleId);
        Debug.LogFormat("[FizzBoss #{0}] First Stage is {1}", ModuleId, currentStage);

        //buttons
        for(int i=0; i<5; i++) {
            int dummy = i;
            buttons[i].OnInteract += delegate () { letterPress(buttons[dummy]); return false; };
        }
        buttons[5].OnInteract += delegate () { buttons[5].AddInteractionPunch(); Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, buttons[5].transform); resetInput(); return false; };
        buttons[6].OnInteract += delegate () { buttons[6].AddInteractionPunch(); Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, buttons[6].transform); Submit(); return false; };
    }

    //replaying stages
    IEnumerator replayStages() {
        submitTime = false;
        buttonPressed = false;
        yield return new WaitUntil(() => buttonPressed);
        Debug.LogFormat("[FizzBoss #{0}] Replaying Stages.", ModuleId);
        for(int i = 0; i < 7; i++) {
            buttons[i].transform.Find("buttonText").GetComponent<TextMesh>().color = Color.gray;
        }
        for(int i=0;  i<stages.Count(); i++) {
            displayTexts[0].text = stages[i].ToString();
            yield return new WaitForSeconds(1f);
        }
        submitTime = true;
        for(int i = 0; i < 7; i++) {
            buttons[i].transform.Find("buttonText").GetComponent<TextMesh>().color = Color.white;
        }
    }

    //reset button
    void resetInput() {
        buttonPressed = true;
        if(!submitTime) { return; }
        if(ModuleSolved) { return; }
        if(displayTexts[1].text == "") {
        input.Clear();
        }
        displayTexts[1].text = "";
    }

    //phrase buttons
    void letterPress(KMSelectable letter) {
        letter.AddInteractionPunch();
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, letter.transform);
        buttonPressed = true;
        if(!submitTime) { return; }
        if(ModuleSolved) { return; }
        string label = letter.transform.Find("buttonText").GetComponent<TextMesh>().text;
        if(displayTexts[1].text == "") { //if display box is empty
            displayTexts[1].text = label;
        } else if(displayTexts[1].text.Length >= 4) { //if display box is full
            input.Add(displayTexts[1].text);
            displayTexts[1].text = label;
        } else if(displayTexts[1].text.Length >=2) { //if display has 2 letters already
            displayTexts[1].text += label;
            displayTexts[1].text += label;
        } else { //if display only has 1 letter
            displayTexts[1].text += label;
        }
    }

    //submit button
    void Submit() {
        buttonPressed = true;
        if(!submitTime) { return; }
        if (ModuleSolved) { return; }
        if(displayTexts[1].text != "") {
            input.Add(displayTexts[1].text);
        }
        Debug.LogFormat("[FizzBoss #{0}] Submitting {1}.", ModuleId, input.Count==0 ? "Nothing" : string.Join(" ", input.ToArray()));
        if (input.Count != solution.Count) {
            Strike();
            return;
        }
        for (int i = 0; i < input.Count; i++) {
            if (input[i] != solution[i]) {
                Strike();
                return;
            }
        }
        Solve();
    }

    static List<int> generatePrimesNaive(int n) {
        List<int> primes = new List<int>();
        primes.Add(3);
        int nextPrime = 5;
        while (primes.Count < n) {
            int sqrt = (int)Math.Sqrt(nextPrime);
            bool isPrime = true;
            for (int i = 0; (int)primes[i] <= sqrt; i++) {
                if (nextPrime % primes[i] == 0) {
                    isPrime = false;
                    break;
                }
            }
            if (isPrime) {
                primes.Add(nextPrime);
            }
            nextPrime += 2;
        }
        return primes;
    }

    void Update () { //Shit that happens at any point after initialization
        //Saves number of Ignored Modules on bomb as Ignored
        int Ignored = 0;
        for(int i = 0; i < Bomb.GetSolvableModuleNames().Count(); i++) {
            if(ignoredModules.Contains(Bomb.GetSolvableModuleNames()[i])) {
                Ignored++;
            }
        }
        //stuff to run when a module is solved on the bomb
        /*
        so take the previous stages, add them together
        take that number apply it to the 125 phrases
        add 1 to the current number for each phrase that worked
         */
        ModCount = Bomb.GetSolvableModuleNames().Count() - Ignored;
        if(SolveCount != Bomb.GetSolvedModuleNames().Count() && !NoModsLeft) {
            while(SolveCount != Bomb.GetSolvedModuleNames().Count()) { //In case multiple modules solve simultaneously
                SolveCount++;
            }
            currentStage = Rnd.Range(100, 1000);
            displayTexts[0].text = currentStage.ToString();
            int modifier = 0;
            for(int i=0; i<125; i++) {
                if(rollingTotal%primes[i]==0) { modifier++; }
            }
            stages.Add(currentStage);
            currentStage += modifier;
            rollingTotal += currentStage;
            stageNumber++;
            Debug.LogFormat("[FizzBoss #{0}] Next stage is Number {1}, Display say {2}", ModuleId, stageNumber, currentStage-modifier);
            Debug.LogFormat("[FizzBoss #{0}] Previous Stages add to {1}, adding {2} to Current Stage", ModuleId, rollingTotal-currentStage, modifier);
        }
        //stuff to run when it's this module's turn to solve
        if(SolveCount >= ModCount && !NoModsLeft) {
            NoModsLeft = true;
            for(int i = 0; i < 125; i++) {
                if(rollingTotal % primes[i] == 0) {
                    solution.Add(phrases[i]);
                }
            }
            submitTime = true;
            Debug.LogFormat("[FizzBoss #{0}] All Stages add to {1}, The Solution is {2}.", ModuleId, rollingTotal, solution.Count == 0 ? "Nothing" : string.Join(" ", solution.ToArray()));
            for(int i=0; i<7; i++) {
                buttons[i].transform.Find("buttonText").GetComponent<TextMesh>().color = Color.white;
            }
        }

    }

    void Solve () {
        GetComponent<KMBombModule>().HandlePass();
        Debug.LogFormat("[FizzBoss #{0}] Correct! Module Solved.", ModuleId);
        string[] pronouns = { "He", "She", "They", "Fae" };
        displayTexts[1].text = String.Format("{3} {0} on my \n {1} till i {2}", phrases[Rnd.Range(0, 125)], phrases[Rnd.Range(0, 125)], phrases[Rnd.Range(0, 125)], pronouns[Rnd.Range(0, 4)]);
        displayTexts[1].fontSize = 150;
        ModuleSolved = true;
    }

    void Strike () {
        GetComponent<KMBombModule>().HandleStrike();
        Debug.LogFormat("[FizzBoss #{0}] Incorrect! Strike Issued.", ModuleId);
        resetInput();
        resetInput();
        StartCoroutine(replayStages());
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use !{0} f/b/z/i/u/s/r to press a button. Inputs may be chained like fizbuzs";
#pragma warning restore 414

    KMSelectable[] ProcessTwitchCommand (string Command) {
        foreach(char i in Command) {
            string f = i.ToString().ToLower();
            string[] accepted = { "f", "b", "z", "i", "u", "s", "r", "" };
            int counting = 0;
            foreach(string h in accepted) {
                if(h==f) {
                    counting++;
                }
            }
            if(counting==0) { return null; }
        }
        List<KMSelectable> buttonList = new List<KMSelectable>();
        foreach(char i in Command) {
            string f = i.ToString().ToLower();
            if(f == "f") {
                buttonList.Add(buttons[0]);
            }
            if(f == "b") {
                buttonList.Add(buttons[1]);
            }
            if(f == "z") {
                buttonList.Add(buttons[2]);
            }
            if(f == "i") {
                buttonList.Add(buttons[3]);
            }
            if(f == "u") {
                buttonList.Add(buttons[4]);
            }
            if(f == "r") {
                buttonList.Add(buttons[5]);
            }
            if(f == "s") {
                buttonList.Add(buttons[6]);
            }
        }
        return buttonList.ToArray();
    }

    KMSelectable[] TwitchHandleForcedSolve () {
        foreach(string s in solution) {
            foreach(char i in s) {
                string f = i.ToString().ToLower();
                string[] accepted = { "f", "b", "z", "i", "u", "s", "r" };
                int counting = 0;
                foreach(string h in accepted) {
                    if(h==f) {
                        counting++;
                    }
                }
                if(counting==0) { return null; }
            }
        }
        List<KMSelectable> buttonList = new List<KMSelectable>();
        foreach(string s in solution) {
            foreach(char i in s) {
                string f = i.ToString().ToLower();
                if(f == "f") {
                    buttonList.Add(buttons[0]);
                }
                if(f == "b") {
                    buttonList.Add(buttons[1]);
                }
                if(f == "z") {
                    buttonList.Add(buttons[2]);
                }
                if(f == "i") {
                    buttonList.Add(buttons[3]);
                }
                if(f == "u") {
                    buttonList.Add(buttons[4]);
                }
                if(f == "r") {
                    buttonList.Add(buttons[5]);
                }
                if(f == "s") {
                    buttonList.Add(buttons[6]);
                }
            }
        }
        return buttonList.ToArray();
    }
}
