using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameLogBackdrop : MonoBehaviour
{
    RectTransform trans;
    Color visibleColor;

    [SerializeField] AnimationCurve curve;
    [SerializeField] Image image;

    bool isFadedIn;

    Coroutine moveCor;
    Coroutine fadeCor;
    // Start is called before the first frame update
    void Awake()
    {
        trans = GetComponent<RectTransform>();

        visibleColor = image.color;

        isFadedIn = false;
        image.color = Color.clear;

        trans.offsetMin = new Vector2(trans.offsetMin.x, trans.offsetMax.y);
    }

    public void SetSize(int index){
        index = Mathf.Clamp(index, -1, GameLogManager.MaxLeadEntries - 1);

        if(index == -1 && isFadedIn){
            if(fadeCor != null) StopCoroutine(fadeCor);
            fadeCor = StartCoroutine(Fade(false));

            if(moveCor != null) StopCoroutine(moveCor);
            moveCor = StartCoroutine(LerpToPos(new Vector2(trans.offsetMin.x, trans.offsetMax.y)));
            return;
        }else if(index != -1 && !isFadedIn){
            if(fadeCor != null) StopCoroutine(fadeCor);
            fadeCor = StartCoroutine(Fade(true));
        }

        //First, we convert the index into a y position
        float y = IndexToPos(index);
        Vector2 vector2 = new Vector2(trans.offsetMin.x, -y);

        //Then we lerp to that y position
        if(moveCor != null) StopCoroutine(moveCor);
        moveCor = StartCoroutine(LerpToPos(vector2));
    }

    float IndexToPos(int index){
        //THAT 40 IS AN ESTIMATE, IT IS A MUTIPLYER THAT LANDS THE
        //BOTTOM OF THE RECT IN THE CENTER OF THE ENTRIES
        return (index * (GameLogManager.DistBetweenEntries + 40)) + 50;
    }

    IEnumerator LerpToPos(Vector2 goal){
        Vector2 start = trans.offsetMin;
        
        float maxTime = GameLogManager.EntryLerpTime / 2;
        float timer = maxTime;

        while(timer > 0f){
            float t = (-timer / GameLogManager.EntryLerpTime) + 1;
            float evaluatedT = curve.Evaluate(t);

            trans.offsetMin = Vector2.LerpUnclamped(start, goal, evaluatedT);

            timer -= Time.deltaTime;
            yield return null;
        }

        trans.offsetMin = goal;
    }
    
    IEnumerator Fade(bool isIn){
        isFadedIn = isIn;

        Color start = image.color;
        Color goal = visibleColor;
        if(!isIn) goal = Color.clear;
        
        float maxTime = GameLogManager.EntryLerpTime / 2;
        float timer = maxTime;

        while(timer > 0f){
            float t = (-timer / GameLogManager.EntryLerpTime) + 1;
            float evaluatedT = curve.Evaluate(t);

            image.color = Color.LerpUnclamped(start, goal, evaluatedT);

            timer -= Time.deltaTime;
            yield return null;
        }

        image.color = goal;
    }
}
