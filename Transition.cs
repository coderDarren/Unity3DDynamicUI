using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Transition : MonoBehaviour {

    /// <summary>
    /// This script should be placed on all pages that transition
    /// </summary>
    /// 
    public enum OutTransitionType { Fade, FadeInstant, Flicker };
    public OutTransitionType outTransitionType;
    public enum InTransitionType { Fade, FadeInstant, Flicker };
    public InTransitionType inTransitionType;
    public string parentTag;
    public Vector3 spawnPosition = Vector3.zero;
    public float fadeSpeed = 2;
    public float flickerRate = 0.025f;

    //variables to handle bringing the new page in
    bool transitionInitialized = false;
    bool startTransition = false;
    float inColorAlpha = 0;
    float outColorAlpha = 0;
    Text[] transitionTxts, txts;
    Image[] transitionImgs, imgs;
    RectTransform transitionPage;
    RectTransform thisPage;


    //INITIALIZE

    void Start()
    {
        //if our out transition is fade (fade out), we need our alpa to be fully opaque
        if (outTransitionType == OutTransitionType.Fade)
            outColorAlpha = 1;
        thisPage = GetComponent<RectTransform>();
        //hold images and texts that are going to be faded
        imgs = GetComponentsInChildren<Image>();
        txts = GetComponentsInChildren<Text>();
    }

    public GameObject InitializeTransitionPage(GameObject transition)
    {
        //set the transition page
        GameObject go = Instantiate(transition as GameObject);
        transitionPage = go.GetComponent<RectTransform>();
        //the transition page parent needs to be the canvas (or one of its children)
        transitionPage.transform.SetParent(GameObject.FindGameObjectWithTag(parentTag).transform); //keep the canvas as the parent
        transitionPage.transform.localScale = Vector3.one;
        //fill the text and image arrays with components that need to be faded
        transitionTxts = transitionPage.GetComponentsInChildren<Text>();
        transitionImgs = transitionPage.GetComponentsInChildren<Image>();

        //start the page off at transparent
        foreach (Text t in transitionTxts)
            t.color = new Vector4(t.color.r, t.color.g, t.color.b, 0);
        foreach (Image i in transitionImgs)
            i.color = new Vector4(i.color.r, i.color.g, i.color.b, 0);

        transitionInitialized = true;

        return transitionPage.gameObject;
    }


    //END INITIALIZE

    //called from menu controller
    public void StartTransition()
    {
        startTransition = true;
    }


    //UPDATING

    void Update()
    {
        if (startTransition)
        {
            switch (outTransitionType) //page leaving
            {
                case OutTransitionType.Fade: FadePageOut(); break;
                case OutTransitionType.FadeInstant: outColorAlpha = 0; break;
                case OutTransitionType.Flicker: StartCoroutine("FlickerOut", flickerRate); break;
            }
            switch (inTransitionType) //new page coming in
            {
                case InTransitionType.Fade: FadePageIn(); break;
                case InTransitionType.FadeInstant: inColorAlpha = 1; break;
                case InTransitionType.Flicker: StartCoroutine("FlickerIn", flickerRate); break;
            }
            UpdateTransitionPageColors();
            UpdateCurrentPageColors();
        }
    }


    //END UPDATING


    //OUT TRANSITION FUNCTIONS

    void FadePageOut()
    {
        outColorAlpha = Mathf.Lerp(outColorAlpha, 0, fadeSpeed * Time.deltaTime);
    }

    IEnumerator FlickerOut(float frequency)
    {
        for (int i = 0; i < 8; i++)
        {
            yield return new WaitForSeconds(frequency);
            outColorAlpha = 0.35f;
            yield return new WaitForSeconds(frequency);
            outColorAlpha = 0.8f;
        }

    }

    //END OUT TRANSITION FUNCTIONS


    //IN TRANSITION FUNCTIONS

    void FadePageIn()
    {
        inColorAlpha = Mathf.Lerp(inColorAlpha, 1, fadeSpeed * Time.deltaTime);

        if (inColorAlpha > 0.99f)
            inColorAlpha = 1; //object wont be destroyed until alpha is 1

        if (inColorAlpha == 1)
        {
            //page is loaded in, so we can destroy this
            Destroy(gameObject);
        }
    }

    IEnumerator FlickerIn(float frequency)
    {
        for (int i = 0; i < 8; i++)
        {
            yield return new WaitForSeconds(frequency);
            inColorAlpha = 0.35f;
            yield return new WaitForSeconds(frequency);
            inColorAlpha = 1.0f;
        }

        if (inColorAlpha == 1.0f)
            Destroy(gameObject);
    }

    //END IN TRANSITION FUNCTIONS


    //HELPER METHODS

    void UpdateTransitionPageColors()
    {
        if (transitionImgs != null)
        {
            foreach (Image i in transitionImgs)
                i.color = new Vector4(i.color.r, i.color.g, i.color.b, inColorAlpha);
        }
        if (transitionTxts != null)
        {
            foreach (Text t in transitionTxts)
                t.color = new Vector4(t.color.r, t.color.g, t.color.b, inColorAlpha);
        }
    }

    void UpdateCurrentPageColors()
    {
        if (imgs != null)
        {
            foreach (Image i in imgs)
                i.color = new Vector4(i.color.r, i.color.g, i.color.b, outColorAlpha);
        }
        if (txts != null)
        {
            foreach (Text t in txts)
                t.color = new Vector4(t.color.r, t.color.g, t.color.b, outColorAlpha);
        }
    }

    //END HELPER METHODS

}
