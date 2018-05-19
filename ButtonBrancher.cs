using UnityEngine;
using System.Collections;
using UnityEngine.UI; //because we will be requiring UI data types
using System.Collections.Generic; //because we will be using dynamic lists

public class ButtonBrancher : MonoBehaviour {

    public class ButtonScaler
    {
        enum ScaleMode { MatchWidthHeight, IndependentWidthHeight }
        ScaleMode mode;
        Vector2 referenceButtonSize;

        [HideInInspector]
        public Vector2 referenceScreenSize;
        public Vector2 newButtonSize;

        public void Initialize(Vector2 refButtonSize, Vector2 refScreenSize, int scaleMode)
        {
            mode = (ScaleMode)scaleMode;
            referenceButtonSize = refButtonSize;
            referenceScreenSize = refScreenSize;
            SetNewButtonSize();
        }

        void SetNewButtonSize()
        {
            if (mode == ScaleMode.IndependentWidthHeight)
            {
                newButtonSize.x = (referenceButtonSize.x * Screen.width) / referenceScreenSize.x;
                newButtonSize.y = (referenceButtonSize.y * Screen.height) / referenceScreenSize.y;
            }
            else if (mode == ScaleMode.MatchWidthHeight)
            {
                newButtonSize.x = (referenceButtonSize.x * Screen.width) / referenceScreenSize.x;
                newButtonSize.y = newButtonSize.x;
            }
        }
    }

	[System.Serializable]//we want our class and its members to be seen from the inspector
    public class RevealSettings
    {
        public enum RevealOption { Linear, Circular };
        public RevealOption option;
        public float translateSmooth = 5f; //how fast the buttons move to their positions
        public float fadeSmooth = 0.01f; //how fast the buttons fade in (if they fade in)
        public bool revealOnStart = false;

        [HideInInspector] //we do not need to see these variables in the inspector
        public bool opening = false;
        [HideInInspector]
        public bool spawned = false;
    }

    [System.Serializable]
    public class LinearSpawner
    {
        public enum RevealStyle { SlideToPosition, FadeInAtPosition };
        public RevealStyle revealStyle;
        public Vector2 direction = new Vector2(0, 1); //slide down
        public float baseButtonSpacing = 5f; //how much space between each button
        public int buttonNumOffset = 0; //how many button spaces offset? Sometimes necessary when using multiple button branchers

        [HideInInspector]
        public float buttonSpacing = 5f; 

        public void FitSpacingToScreenSize(Vector2 refScreenSize)
        {
            float refScreenFloat = (refScreenSize.x + refScreenSize.y) / 2;
            float screenFloat = (Screen.width + Screen.height) / 2;
            buttonSpacing = (baseButtonSpacing * screenFloat) / refScreenFloat;
        }
    }

    [System.Serializable]
    public class CircularSpawner
    {
        public enum RevealStyle { SlideToPosition, FadeInAtPosition };
        public RevealStyle revealStyle;
        public Angle angle;
        public float baseDistFromBrancher = 20;

        [HideInInspector]
        public float distFromBrancher = 0;

        [System.Serializable]
        public struct Angle { public float minAngle; public float maxAngle; }

        public void FitDistanceToScreenSize(Vector2 refScreenSize)
        {
            float refScreenFloat = (refScreenSize.x + refScreenSize.y) / 2;
            float screenFloat = (Screen.width + Screen.height) / 2;
            distFromBrancher = (baseDistFromBrancher * screenFloat) / refScreenFloat;
        }
    }


    public GameObject[] buttonRefs; //PREFABS
    [HideInInspector]
    public List<GameObject> buttons;

    public enum ScaleMode { MatchWidthHeight, IndependentWidthHeight };
    public ScaleMode mode;
    public Vector2 referenceButtonSize;
    public Vector2 referenceScreenSize;

    ButtonScaler buttonScaler = new ButtonScaler();
    public RevealSettings revealSettings = new RevealSettings();
    public LinearSpawner linSpawner = new LinearSpawner();
    public CircularSpawner circSpawner = new CircularSpawner();

    float lastScreenWidth = 0;
    float lastScreenHeight = 0;

    void Start()
    {
        buttons = new List<GameObject>();
        buttonScaler = new ButtonScaler();
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
        buttonScaler.Initialize(referenceButtonSize, referenceScreenSize, (int)mode);
        circSpawner.FitDistanceToScreenSize(buttonScaler.referenceScreenSize);
        linSpawner.FitSpacingToScreenSize(buttonScaler.referenceScreenSize);

        if (revealSettings.revealOnStart)
        {
            SpawnButtons();
        }
    }

    void Update()
    {
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            buttonScaler.Initialize(referenceButtonSize, referenceScreenSize, (int)mode);
            circSpawner.FitDistanceToScreenSize(buttonScaler.referenceScreenSize);
            linSpawner.FitSpacingToScreenSize(buttonScaler.referenceScreenSize);
            SpawnButtons();
        }

        if (revealSettings.opening)
        {
            if (!revealSettings.spawned)
                SpawnButtons();

            switch (revealSettings.option)
            {
                case RevealSettings.RevealOption.Linear:
                    
                    switch (linSpawner.revealStyle)
                    {
                        case LinearSpawner.RevealStyle.SlideToPosition: RevealLinearlyNormal(); break;
                        case LinearSpawner.RevealStyle.FadeInAtPosition: RevealLinearlyFade(); break;
                    }

                    break;
                case RevealSettings.RevealOption.Circular:
                    switch (circSpawner.revealStyle)
                    {
                        case CircularSpawner.RevealStyle.SlideToPosition: RevealCircularNormal(); break;
                        case CircularSpawner.RevealStyle.FadeInAtPosition: RevealCircularFade(); break;
                    }
                    break;
            }
        }
    }

    public void SpawnButtons() //if revealOnStart == false, this method will be called by the button click event
    {
        revealSettings.opening = true;
        //clear button list, in case there are some already in it
        for (int i = buttons.Count - 1; i >= 0; i--)
            Destroy(buttons[i]);
        buttons.Clear();

        //clear buttons on any other button brancher that has the same parent as this brancher
        ClearCommonButtonBranchers();

        for (int i = 0; i < buttonRefs.Length; i++ )
        {
            GameObject b = Instantiate(buttonRefs[i] as GameObject);
            b.transform.SetParent(transform); //make button child of button brancher
            b.transform.position = transform.position; //zeroing the position places the button on the button brancher
            //check if button will fade or not
            if (linSpawner.revealStyle == LinearSpawner.RevealStyle.FadeInAtPosition || circSpawner.revealStyle == CircularSpawner.RevealStyle.FadeInAtPosition)
            {
                //change color alpha of button and its text to 0;
                Color c = b.GetComponent<Image>().color;
                c.a = 0;
                b.GetComponent<Image>().color = c;
                if (b.GetComponentInChildren<Text>()) //button may not have a text component
                {
                    c = b.GetComponentInChildren<Text>().color;
                    c.a = 0;
                    b.GetComponentInChildren<Text>().color = c;
                }
            }
            buttons.Add(b);
        }

        revealSettings.spawned = true;
    }

    void RevealLinearlyNormal()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            //give the button a position to move toward
            Vector3 targetPos;
            RectTransform buttonRect = buttons[i].GetComponent<RectTransform>();
            //set size
            buttonRect.sizeDelta = new Vector2(buttonScaler.newButtonSize.x, buttonScaler.newButtonSize.y);
            //set pos
            targetPos.x = linSpawner.direction.x * ( (i + linSpawner.buttonNumOffset) * (buttonRect.sizeDelta.x + linSpawner.buttonSpacing) ) + transform.position.x;
            targetPos.y = linSpawner.direction.y * ( (i + linSpawner.buttonNumOffset) * (buttonRect.sizeDelta.y + linSpawner.buttonSpacing) ) + transform.position.y;
            targetPos.z = 0;

            buttonRect.position = Vector3.Lerp(buttonRect.position, targetPos, revealSettings.translateSmooth * Time.deltaTime);
        }
    }

    void RevealLinearlyFade()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            //give the button a position to move toward
            Vector3 targetPos;
            ButtonFader previousButtonFader;
            if (i > 0)
                previousButtonFader = buttons[i - 1].GetComponent<ButtonFader>();
            else
                previousButtonFader = null;
            ButtonFader buttonFader = buttons[i].GetComponent<ButtonFader>();

            RectTransform buttonRect = buttons[i].GetComponent<RectTransform>();
            //set size
            buttonRect.sizeDelta = new Vector2(buttonScaler.newButtonSize.x, buttonScaler.newButtonSize.y);
            //set pos
            targetPos.x = linSpawner.direction.x * ((i + linSpawner.buttonNumOffset) * (buttonRect.sizeDelta.x + linSpawner.buttonSpacing)) + transform.position.x;
            targetPos.y = linSpawner.direction.y * ((i + linSpawner.buttonNumOffset) * (buttonRect.sizeDelta.y + linSpawner.buttonSpacing)) + transform.position.y;
            targetPos.z = 0;

            if (previousButtonFader) //first button wont have a previous button
            {
                if (previousButtonFader.faded)
                {
                    buttons[i].transform.position = targetPos;
                    if (buttonFader)
                        buttonFader.Fade(revealSettings.fadeSmooth);
                    else
                        Debug.LogError("You want to fade your buttons, but they need a ButtonFader script to be attached first.");
                }
            }
            else
            {
                buttons[i].transform.position = targetPos;
                if (buttonFader)
                    buttonFader.Fade(revealSettings.fadeSmooth); //for the first button in the array
                else
                    Debug.LogError("You want to fade your buttons, but they need a ButtonFader script to be attached first.");
            }
        }
    }

    void RevealCircularNormal()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            //find angle
            float angleDist = circSpawner.angle.maxAngle - circSpawner.angle.minAngle;
            float targetAngle = circSpawner.angle.minAngle + (angleDist / buttons.Count) * i;
            //find pos
            Vector3 targetPos = transform.position + Vector3.right * circSpawner.distFromBrancher;
            targetPos = RotatePointAroundPivot(targetPos, transform.position, targetAngle);
            RectTransform buttonRect = buttons[i].GetComponent<RectTransform>();
            //resize button
            buttonRect.sizeDelta = new Vector2(buttonScaler.newButtonSize.x, buttonScaler.newButtonSize.y);

            buttonRect.position = Vector3.Lerp(buttonRect.position, targetPos, revealSettings.translateSmooth * Time.deltaTime);
        }
    }

    void RevealCircularFade()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            ButtonFader previousButtonFader;
            if (i > 0)
                previousButtonFader = buttons[i - 1].GetComponent<ButtonFader>();
            else
                previousButtonFader = null;
            ButtonFader buttonFader = buttons[i].GetComponent<ButtonFader>();

            //find angle
            float angleDist = circSpawner.angle.maxAngle - circSpawner.angle.minAngle;
            float targetAngle = circSpawner.angle.minAngle + (angleDist / buttons.Count) * i;
            //find pos
            Vector3 targetPos = transform.position + Vector3.right * circSpawner.distFromBrancher;
            targetPos = RotatePointAroundPivot(targetPos, transform.position, targetAngle);
            RectTransform buttonRect = buttons[i].GetComponent<RectTransform>();
            //resize button
            buttonRect.sizeDelta = new Vector2(buttonScaler.newButtonSize.x, buttonScaler.newButtonSize.y);

            if (previousButtonFader) //first button wont have a previous button
            {
                if (previousButtonFader.faded)
                {
                    buttonRect.position = targetPos;
                    if (buttonFader)
                        buttonFader.Fade(revealSettings.fadeSmooth);
                    else
                        Debug.LogError("You want to fade your buttons, but they need a ButtonFader script to be attached first.");
                }
            }
            else
            {
                buttonRect.position = targetPos;
                if (buttonFader)
                    buttonFader.Fade(revealSettings.fadeSmooth); //for the first button in the array
                else
                    Debug.LogError("You want to fade your buttons, but they need a ButtonFader script to be attached first.");
            }
        }
    }

    Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, float angle)
    {
        Vector3 targetPoint = point - pivot;
        targetPoint = Quaternion.Euler(0, 0, angle) * targetPoint;
        targetPoint += pivot;
        return targetPoint;
    }

    void ClearCommonButtonBranchers()
    {
        GameObject[] branchers = GameObject.FindGameObjectsWithTag("ButtonBrancher");
        foreach(GameObject brancher in branchers)
        {
            //check if the parent of this brancher is the same as the brancher we are currently looking at
            if (brancher.transform.parent == transform.parent)
            {
                //remove the brancher's buttons to keep things tidy
                ButtonBrancher bb = brancher.GetComponent<ButtonBrancher>();
                for (int i = bb.buttons.Count - 1; i >= 0; i--)
                    Destroy(bb.buttons[i]);
                bb.buttons.Clear();
            }
        }
    }
}
