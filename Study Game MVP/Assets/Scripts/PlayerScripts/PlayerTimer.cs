using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTimer : MonoBehaviour
{
    [SerializeField] bool countdown = true;

    float timer;

    [SerializeField] Slider slider;

    bool isFading;

    void Start(){
        ToggleVisuals(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(countdown) timer -= Time.deltaTime;
        slider.value = timer;

        if(slider.value >= slider.maxValue && slider.IsActive() && !isFading){
            StartCoroutine(FadeOutSlider());
        }
    }

    IEnumerator FadeOutSlider(){
        isFading = true;

        Image img = slider.fillRect.GetComponent<Image>();
        Color startColor = new Color(img.color.r, img.color.g, img.color.b, img.color.a);
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0);

        float time = 1f;
        float maxTime = time;

        while(time > 0f){
            time -= Time.deltaTime;

            //we need to flip these because the timer counts down
            img.color = Color.Lerp(endColor, startColor, time / maxTime);

            yield return null;
        }
        img.color = startColor;
        
        isFading = false;
        
        slider.gameObject.SetActive(false);
    }

    public void SetCountdown(bool canCountdown){
        countdown = canCountdown;
    }
    public void SetTimer(float t){
        timer = t;
    }
    public void ToggleVisuals(bool active){
        slider.gameObject.SetActive(active);
    }
    public void SetMaxValue(float t){
        slider.maxValue = t;
    }
}
