using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class OffscreenPointer : MonoBehaviour
{
    const float PercentMargin = 0.05f;
    const float lerpConstant = 5f;

    const float fadeTime = 0.25f;

    const float minSize = 0.35f;

    [SerializeField] GameObject target;
    [SerializeField] Image image;
    [SerializeField] Player player;
    float rotation;
    bool isSim = true;

    bool isFadedIn = true;

    Coroutine fadeCor;

    // Update is called once per frame
    void Update()
    {
        //We are going to check if we should be able to see this pointer
        if(player != null){
            //this code is run if the state changes from what it was
            if(Player.isRightPlayer.CanPointToTeam(player.GetTeam()) && !isSim){
                isSim = true;
                print("We are fading in");

                if(fadeCor != null) StopCoroutine(fadeCor);
                fadeCor = StartCoroutine(Fade(true));
            }else if(!Player.isRightPlayer.CanPointToTeam(player.GetTeam()) && isSim){
                isSim = false;

                if(fadeCor != null) StopCoroutine(fadeCor);
                fadeCor = StartCoroutine(Fade(false));
            }
        }

        if(!isSim) return;

        if(target == null) return;

        //We get the position
        Vector3 unclampedGoal = Camera.main.WorldToScreenPoint(target.transform.position);
        unclampedGoal.z = 0;
        Vector3 goal = unclampedGoal;

        //We clamp it around a margin
        goal.x = Mathf.Clamp(goal.x, 0f + (Screen.width * PercentMargin), Screen.width - (Screen.width * PercentMargin));
        goal.y = Mathf.Clamp(goal.y, 0f + (Screen.height * PercentMargin), Screen.height - (Screen.height * PercentMargin));
        goal.z = 0;

        //Now, we rotate

        float angle;
        if(goal.x == unclampedGoal.x && goal.y == unclampedGoal.y){
            //This is run if the target is on the screen (or at least, the pointer and the target overlap)
            angle = Mathf.Atan2(goal.y - (Screen.height/2), goal.x - (Screen.width/2)) * Mathf.Rad2Deg;

            if(isFadedIn){
                if(fadeCor != null) StopCoroutine(fadeCor);
                fadeCor = StartCoroutine(Fade(false));
            }
        }
        else{
            angle = Mathf.Atan2(goal.y - unclampedGoal.y, goal.x - unclampedGoal.x) * Mathf.Rad2Deg;
            angle -= 180;

            if(!isFadedIn){
                if(fadeCor != null) StopCoroutine(fadeCor);
                fadeCor = StartCoroutine(Fade(true));
            }
        }
        ClampRotation(angle);

        //We will now check distance to get the right size

        //We need the actual distances (world space goal)
        Vector3 wsGoal = Camera.main.ScreenToWorldPoint(goal);
        wsGoal.z = 0;
        Vector3 targPos = target.transform.position;
        targPos.z = 0;

        //At about 30 units the size would be zero
        float size = 1 - (Vector3.Distance(wsGoal, targPos) * 0.03f);
        size = Mathf.Clamp(size, minSize, 1);
        
        //This isn't perfect, but good enough!
        rotation = Mathf.LerpAngle(rotation, angle, lerpConstant * Time.deltaTime);
        transform.rotation = Quaternion.AngleAxis(rotation, Vector3.forward);
        transform.position = goal;
        transform.localScale = Vector2.one * size;

        if(player == null) return;
        //All code bellow here is only run if the player exists (and, as checked earlier, if this is a team we can point to)

        //image.color = TeamColorManager.teamColorDictionary[player.GetTeam()];
    }
    
    float ClampRotation(float r){
        while(r > 360){ r -= 360;}
        while(r < 0){ r += 360;}

        return r;
    }

    IEnumerator Fade(bool fadeIn){
        Color startColor = image.color;
        Color goalColor = new Color(1, 1, 1, 0);
        if(fadeIn){
            goalColor = TeamColorManager.teamColorDictionary[player.GetTeam()];
            isFadedIn = true;
        }else{
            isFadedIn = false;
        }

        image.color = startColor;

        float time = fadeTime;

        while(time > 0f){

            float t = (-time / fadeTime) + 1;
            //float evaluatedT = curve.Evaluate(t);

            image.color = Color.Lerp(startColor, goalColor, t);

            time -= Time.deltaTime;
            yield return null;
        }

        image.color = goalColor;
    }

    public void SetFollow(Player play){
        target = play.gameObject;
        player = play;

        image.color = Color.clear;
        isFadedIn = false;
    }
    public void SetFollow(GameObject obj){
        target = obj;
    }

    public void DeletPointer(){
        Destroy(gameObject);
    }
}
