using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameLogEntry : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] AnimationCurve curve;


    int entryIndex;

    Coroutine moveCor;
    public void SetText(string s){
        text.text = s;
    }

    /// <summary>
    /// Returns a random verb to use in leaderboards (eg: "Rekt", "Tagged")
    /// </summary>
    /// <returns></returns>
    public string RandomTagVerb(){
        int r = Random.Range(0, 6);
        if(r == 0) return "Rekt";
        if(r == 1) return "Tagged";
        if(r == 2) return "Deep Fried";
        if(r == 3) return "Cooked";
        if(r == 4) return "Alt + F4ed";
        if(r == 5) return "Shreaded";

        Debug.LogWarning("Reached an unreachable event. There is an error with the way this code is written");
        return "Tagged";
    }
    public string RandomEscVerb(){
        int r = Random.Range(0, 6);
        if(r == 0) return "Was Faster Than";
        if(r == 1) return "Escaped";
        if(r == 2) return "Knocked Back";
        if(r == 3) return "Esc";
        if(r == 4) return "Outsmarted";
        if(r == 5) return "Pushed";

        Debug.LogWarning("Reached an unreachable event. There is an error with the way this code is written");
        return "Escaped";
    }

    public void SpwanInEntry(){
        entryIndex = 0;

        if(moveCor != null) StopCoroutine(moveCor);

        transform.position = transform.parent.position + new Vector3(0f, GameLogManager.DistBetweenEntries);
        moveCor = StartCoroutine(spwanInEntry(transform.parent.position));

        StartCoroutine(waitTillDelete(4f));
    }
    public int NextEntry(){
        entryIndex += 1;

        if(moveCor != null) StopCoroutine(moveCor);

        if(entryIndex < GameLogManager.MaxLeadEntries){
            Vector3 goalPos = transform.parent.position - new Vector3(0f, GameLogManager.DistBetweenEntries * entryIndex);
            moveCor = StartCoroutine(lerpToNextPos(goalPos));
            return entryIndex;
        }else{
            Vector3 goalPos = transform.parent.position - new Vector3(0f, GameLogManager.DistBetweenEntries * entryIndex);
            StartCoroutine(fadeOut(goalPos));
            return GameLogManager.MaxLeadEntries;
        }
        
    }
    public int GetIndex(){
        return entryIndex;
    }

    IEnumerator spwanInEntry(Vector3 goal){
        Color startColor = new Color(1, 1, 1, 0);
        Color goalColor = Color.white;

        Vector3 startPos = text.transform.position;
        Vector3 goalPos = goal;

        text.color = startColor;

        float time = GameLogManager.EntryLerpTime;

        while(time > 0f){

            float t = (-time / GameLogManager.EntryLerpTime) + 1;
            float evaluatedT = curve.Evaluate(t);

            text.color = Color.Lerp(startColor, goalColor, evaluatedT);
            text.transform.position = Vector3.LerpUnclamped(startPos, goalPos, evaluatedT);

            time -= Time.deltaTime;
            yield return null;
        }

        text.color = goalColor;
        text.transform.position = goal;
    }

    IEnumerator lerpToNextPos(Vector3 goal){
        text.color = Color.white;
        Vector3 startPos = text.transform.position;
        Vector3 goalPos = goal;

        float time = GameLogManager.EntryLerpTime;

        while(time > 0f){

            float t = (-time / GameLogManager.EntryLerpTime) + 1;
            float evaluatedT = curve.Evaluate(t);

            text.transform.position = Vector3.LerpUnclamped(startPos, goalPos, evaluatedT);

            time -= Time.deltaTime;
            yield return null;
        }

        text.transform.position = goal;
    }

    IEnumerator fadeOut(Vector3 goal){
        Color startColor = text.color;
        Color goalColor = new Color(1, 1, 1, 0);

        Vector3 startPos = text.transform.position;
        Vector3 goalPos = goal;

        text.color = startColor;

        float time = GameLogManager.EntryLerpTime;

        while(time > 0f){

            float t = (-time / GameLogManager.EntryLerpTime) + 1;
            float evaluatedT = curve.Evaluate(t);

            text.color = Color.Lerp(startColor, goalColor, evaluatedT);
            text.transform.position = Vector3.Lerp(startPos, goalPos, evaluatedT);

            time -= Time.deltaTime;
            yield return null;
        }

        text.color = goalColor;
        text.transform.position = goal;

        GameLogManager.instance.ReturnEntry(this);
    }

    IEnumerator waitTillDelete(float time){
        yield return new WaitForSecondsRealtime(time);

        Color startColor = text.color;
        Color goalColor = new Color(1, 1, 1, 0);

        text.color = startColor;

        float timer = GameLogManager.EntryLerpTime;

        while(timer > 0f){

            float t = (-timer / GameLogManager.EntryLerpTime) + 1;
            float evaluatedT = curve.Evaluate(t);

            text.color = Color.Lerp(startColor, goalColor, evaluatedT);

            timer -= Time.deltaTime;
            yield return null;
        }

        text.color = goalColor;

        GameLogManager.instance.ReturnEntry(this);

        StopAllCoroutines();
    }
}
