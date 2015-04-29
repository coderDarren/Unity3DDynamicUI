using UnityEngine;
using System.Collections;
using UnityEngine.UI; //--THIS ALLOWS US TO USE UI DATA TYPES (IMAGES, BUTTONS, ETC)

public class Tooltip : MonoBehaviour {

	[System.Serializable] //--THIS WILL SERIALIZE OUR CLASS FOR INSPECTOR USE (IT WILL ALSO CLEAN OUR INSPECTOR UP AND ORGANIZE IT
    public class AnimationSettings //--THIS CLASS SHOULD CONTAIN SETTINGS FOR HOW OUR UI IS GOING TO CHANGE DURING RUNTIME
    {
        public enum OpenStyle { WidthToHeight, HeightToWidth, HeightAndWidth };
        public OpenStyle openStyle; //--HOW WE WANT THE TOOLTIP TO OPEN
        public float widthSmooth = 4.6f, heightSmooth = 4.6f; //--HOW FAST OR SLOW THE TEXTBOX WILL OPEN UP
        public float textSmooth = 0.1f; //--HOW FAST OR SLOW THE TEXT WILL APPEAR

        [HideInInspector] //--THIS IS ONLY TRUE FOR THE NEXT LINE OF CODE DOWN...WE DO NOT NEED TO SEE THESE BOOLEAN VALUES FROM THE INSPECTOR
        public bool widthOpen = false, heightOpen = false; //--WE MAY BE OPENING ONLY ONE AT A TIME, SO WE NEED TO KNOW WHEN EACH IS DONE OPENING

        public void Initialize() //--WE NEED AN INITIALIZE TO BE CALLED EACH TIME THE ANIMATION STARTS OVER
        {
            widthOpen = false;
            heightOpen = false;
        }
    }

    [System.Serializable]
    public class UISettings
    {
        public Image textBox; //--THE BOX THAT WILL CONTAIN TEXT CONTENT
        public Text text; //--THE TOOLTIP MESSAGE
        public Vector2 initialBoxSize = new Vector2(0.25f, 0.25f); //--THE SIZE OF THE TEXTBOX INITIALLY, WITHOUT ANY SCALING
        public Vector2 openedBoxSize = new Vector2(400, 200);
        public float snapToSizeDistance = 0.25f; //--THE DISTANCE FROM CURRENT SIZE TO TARGET SIZE BEFORE CURRENT IS SNAPPED TO TARGET
        public float lifeSpan = 5;

        //--THE FOLLOWING VARIABLES DO NOT NEED TO BE SEEN FROM THE INPSECTOR, BUT THEY NEED TO BE PUBLIC SO THE METHODS OUTSIDE THIS CLASS CAN USE THEM
        [HideInInspector]
        public bool opening = false; //--WE ONLY START OPENING WHEN OPENING IS FALSE
        [HideInInspector]
        public Color textColor; //--OUR TEXT COLOR REFERENCE - EASIER TO CHANGE THE ALPHA VALUE FROM A VALUE OF TYPE COLOR - AS OPPOSE TO TEXT.COLOR WHICH REQUIRES A NEW VECTOR4
        [HideInInspector]
        public Color textBoxColor;
        [HideInInspector]
        public RectTransform textBoxRect; //--THIS REFERENCE IS GOING TO ALLOW US TO MODIFY THE SIZE OF OUR TEXT BOX
        [HideInInspector]
        public Vector2 currentSize; //--OUR TEXT BOX SIZE REFERENCE. MODIFYING THIS IS MUCH EASIER THAN MODIFYING SIZEDELTA AS IT REQUIRES US TO CREATE A NEW VECTOR2

        public void Initialize() //--AGAIN WE HAVE TO INITIALIZE OUR VALUES FOR A NEW ANIMATION CYCLE
        {
            textBoxRect = textBox.GetComponent<RectTransform>();
            textBoxRect.sizeDelta = initialBoxSize; //--SIZE DELTA IS THE VECTOR2 SIZE OF THE RECT WITH RESPECT TO ITS ANCHORS (WE CAN THINK OF X AS WIDTH AND Y AS HEIGHT)
            currentSize = textBoxRect.sizeDelta;
            opening = false;
            //--SET THE TEXT COLOR ALPHA BACK TO 0;
            textColor = text.color; 
            textColor.a = 0;
            text.color = textColor;
            textBoxColor = textBox.color;
            textBoxColor.a = 1;
            textBox.color = textBoxColor;
            
            textBox.gameObject.SetActive(false);
            text.gameObject.SetActive(false);
        }
    }

    //--CREATE PUBLIC REFERENCES TO OUR CLASSES SO THEY MAY BE SEEN AND MODIFIED FROM THE INSPECTOR
    public AnimationSettings animSettings = new AnimationSettings();
    public UISettings uiSettings = new UISettings();

    float lifeTimer = 0;

    void Start()
    {
        animSettings.Initialize();
        uiSettings.Initialize();
    }

    //--THIS METHOD CALLED WHEN BUTTON IS CLICKED
    public void StartOpen()
    {
        uiSettings.opening = true;
        uiSettings.textBox.gameObject.SetActive(true);
        uiSettings.text.gameObject.SetActive(true);
    }

    void Update()
    {
        if (uiSettings.opening)
        {
            OpenToolTip();

            if (animSettings.widthOpen && animSettings.heightOpen)
            {
                lifeTimer += Time.deltaTime;
                if (lifeTimer > uiSettings.lifeSpan)
                {
                    FadeToolTipOut();
                }
                else
                {
                    FadeTextIn();
                }
            }

        }
    }

    void OpenToolTip()
    {
        //--CHECK WHICH OPEN STYLE TO USE
        switch(animSettings.openStyle)
        {
            case AnimationSettings.OpenStyle.HeightToWidth:
                OpenHeightToWidth();
                break;
            case AnimationSettings.OpenStyle.WidthToHeight:
                OpenWidthToHeight();
                break;
            case AnimationSettings.OpenStyle.HeightAndWidth:
                OpenHeightAndWidth();
                break;
            default:
                Debug.LogError("No animation is set for the selected open style!");
                break;
        }

        uiSettings.textBoxRect.sizeDelta = uiSettings.currentSize;
    }

    void OpenWidthToHeight()
    {
        if (!animSettings.widthOpen)
        {
            OpenWidth();
        }
        else
        {
            OpenHeight();
        }
    }

    void OpenHeightToWidth()
    {
        if (!animSettings.heightOpen)
        {
            OpenHeight();
        }
        else
        {
            OpenWidth();
        }
    }

    void OpenHeightAndWidth()
    {
        if (!animSettings.widthOpen)
        {
            OpenWidth();
        }
        if (!animSettings.heightOpen)
        {
            OpenHeight();
        }
    }

    void OpenWidth()
    {
        uiSettings.currentSize.x = Mathf.Lerp(uiSettings.currentSize.x, uiSettings.openedBoxSize.x, animSettings.widthSmooth * Time.deltaTime);

        //--IF THE WIDTH IS CLOSE ENOUGH TO TARGET WIDTH, WE CAN SNAP TO THE TARGET WIDTH AND SET WIDTH OPENED TO TRUE
        if (Mathf.Abs(uiSettings.currentSize.x - uiSettings.openedBoxSize.x) < uiSettings.snapToSizeDistance)
        {
            uiSettings.currentSize.x = uiSettings.openedBoxSize.x;
            animSettings.widthOpen = true;
        }
    }

    void OpenHeight()
    {
        uiSettings.currentSize.y = Mathf.Lerp(uiSettings.currentSize.y, uiSettings.openedBoxSize.y, animSettings.heightSmooth * Time.deltaTime);

        //--IF THE HEIGHT IS CLOSE ENOUGH TO TARGET HEIGHT, WE CAN SNAP TO THE TARGET HEIGHT AND SET HEIGHT OPENED TO TRUE
        if (Mathf.Abs(uiSettings.currentSize.y - uiSettings.openedBoxSize.y) < uiSettings.snapToSizeDistance)
        {
            uiSettings.currentSize.y = uiSettings.openedBoxSize.y;
            animSettings.heightOpen = true;
        }
    }
       
    void FadeTextIn()
    {
        uiSettings.textColor.a = Mathf.Lerp(uiSettings.textColor.a, 1, animSettings.textSmooth * Time.deltaTime);
        uiSettings.text.color = uiSettings.textColor;
    }

    void FadeToolTipOut()
    {
        uiSettings.textColor.a = Mathf.Lerp(uiSettings.textColor.a, 0, animSettings.textSmooth * Time.deltaTime);
        uiSettings.text.color = uiSettings.textColor;
        uiSettings.textBoxColor.a = Mathf.Lerp(uiSettings.textBoxColor.a, 0, animSettings.textSmooth * Time.deltaTime);
        uiSettings.textBox.color = uiSettings.textBoxColor;

        if (uiSettings.textBoxColor.a < 0.01f) //--THIS IS THE POINT WHERE OUR ANIMATION IS DONE, AND WE NEED TO REINITIALIZE
        {
            uiSettings.opening = false;
            animSettings.Initialize();
            uiSettings.Initialize();
            lifeTimer = 0;
        }
    }
}
