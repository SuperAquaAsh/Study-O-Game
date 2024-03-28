using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
public class QuestionDisplay : MonoBehaviour
{
    [HideInInspector] public static QuestionDisplay instance;

    [Header("Objects")]
    [SerializeField] GameObject wholeContainer;
    [SerializeField] GameObject questionContainer;
    [SerializeField] TextMeshProUGUI questionText;

    [SerializeField] AnswerButton[] answers;

    [SerializeField] Slider timerSlider;
    [SerializeField] Animator animator;

    Question lastQuestion;

    public event EventHandler<OnAnswerArgs> OnAnswer;

    public class OnAnswerArgs : EventArgs{
        public float time;
        public bool isRight;
    }

    public float timer {get; private set;}
    public bool SliderActive {get; private set;}
    public bool canSeeAnswer {get; private set;}

    //This just tells that we are displaying the wrong answer, and CAN'T be overriden (yet...)
    public bool showingWrong {get; private set;} = false;
    bool pendingShow = false;
    bool pendingNewQuestion = false;
    int pendingNewQuestionInt = -1;


    //ALWAYS make sure this matches the active state of the slider
    public bool isQuestionUp {get; private set;}

    Coroutine timerCor;

    void Awake()
    {
        if(instance == null) instance = this;

        if(instance != this)
        {
            Debug.LogWarning("Two instances of QuestionDisplay found! Destroying an object");
            Destroy(gameObject);
            return;
        }
        
        //This does get destroyed between loads
    }

    void Start()
    {
        questionContainer.SetActive(false);
        timerSlider.gameObject.SetActive(false);
        wholeContainer.SetActive(false);
        isQuestionUp = timerSlider.gameObject.activeSelf;
    }

    void Update(){
        if(Input.GetKeyDown(KeyCode.H)){
            DisplayQuestion();
        }
        timer += Time.deltaTime;
    }
    #region Timer Slider Functions
    public void DisplaySider(bool d){
        timerSlider.gameObject.SetActive(d);
        isQuestionUp = timerSlider.gameObject.activeSelf;
    }

    public void ChangeSlider(float v){
        if(!timerSlider.gameObject.activeSelf) timerSlider.gameObject.SetActive(true);
        isQuestionUp = timerSlider.gameObject.activeSelf;
        timerSlider.value = v;
    }

    #endregion

    #region Question Functions
    public void DisplayQuestion(bool overrideWrong = false)
    {
        if(showingWrong && !overrideWrong){
            pendingNewQuestion = true;
            pendingNewQuestionInt = -1;
            pendingShow = true;
            return;
        }
        if(showingWrong && overrideWrong){
            showingWrong = false;
        }
        //Make the question appear
        wholeContainer.SetActive(true);
        questionContainer.SetActive(true);

        //Also play the animation
        animator.Play("Fade_In");

        //Reset the Timer
        //CanSeeAnswer is set here
        timerCor = StartCoroutine(KeepTimerAtZero(1.7f));
        timer = 0f;
        isQuestionUp = true;
        Question question = QuizManager.instance.FetchQuestion();

        //Remember the last question just in case we need to display it because we got it wrong
        lastQuestion = question;
        
        //Set all the variables
        questionText.text = question.questionText;

        //Shuffle the answers
        question.answers = reshuffleAnswers(question.answers);

        //Set the answers
        for (int i = 0; i < answers.Length; i++)
        {
            answers[i].SetAnswer(question.answers[i].answerText, question.answers[i].isRight);
        }
    }

    public void DisplayQuestion(int QuestionNum, bool overrideWrong = false)
    {
        if(showingWrong && !overrideWrong){
            pendingNewQuestion = true;
            pendingNewQuestionInt = QuestionNum;
            pendingShow = true;
            return;
        }
        if(showingWrong && overrideWrong){
            showingWrong = false;
        }
        //Make the question appear
        wholeContainer.SetActive(true);
        questionContainer.SetActive(true);

        //Also play the animation
        animator.Play("Fade_In", 0, 0);

        //Reset the Timer
        timer = 0f;
        timerCor = StartCoroutine(KeepTimerAtZero(1.7f));
        isQuestionUp = true;
        Question question = QuizManager.instance.FetchQuestion(QuestionNum);

        //Remember the last question just in case we need to display it because we got it wrong
        lastQuestion = question;

        //Set all the variables

        //Shuffle the answers (From now on, the order of answers no longer matters)
        question.answers = reshuffleAnswers(question.answers);

        //Set Question text
        questionText.text = question.questionText;

        //Set the answers
        for (int i = 0; i < answers.Length; i++)
        {
            answers[i].SetAnswer(question.answers[i].answerText, question.answers[i].isRight);
        }
    }

    IEnumerator KeepTimerAtZero(float time){
        while(time > 0f){
            time -= Time.deltaTime;
            timer = 0f;
            yield return null;
        }

        canSeeAnswer = true;
        timer = 0f;
    }

    public void HideQuestion(bool overrideWrong = false)
    {
        if(showingWrong && !overrideWrong){
            pendingShow = false;
        }else if (!showingWrong){
            questionContainer.SetActive(false);
            wholeContainer.SetActive(false);
        }

        if(showingWrong && overrideWrong){
            showingWrong = false;
            questionContainer.SetActive(false);
            wholeContainer.SetActive(false);
        }
        
    }
    public void Answered(bool isAnswerRight){
        isQuestionUp = false;

        canSeeAnswer = false;
        if(timerCor != null) StopCoroutine(timerCor);

        if (!isAnswerRight){
            showingWrong = true;
            SFXManager.instance.StartSound(5);
            StartCoroutine(WaitDisplayQuestion());
        }
        if(isAnswerRight) SFXManager.instance.StartSound(4);

        //Change the color of the buttons
        for (int i = 0; i < answers.Length; i++)
        {
            answers[i].ChangeDisplay(true);
        }
        if(OnAnswer != null) OnAnswer(this, new OnAnswerArgs {time = timer, isRight = isAnswerRight});

        //reset the timer
        timer = 0f;
    }

    //STOLEN CODE FROM: "https://forum.unity.com/threads/randomize-array-in-c.86871/"
    Answer[] reshuffleAnswers(Answer[] texts)
    {
        // Knuth shuffle algorithm :: courtesy of Wikipedia :)
        for (int t = 0; t < texts.Length; t++ )
        {
            Answer tmp = texts[t];
            int r = UnityEngine.Random.Range(t, texts.Length);
            texts[t] = texts[r];
            texts[r] = tmp;
        }
        return texts;
    }

    IEnumerator WaitDisplayQuestion(){
        float timer = 5f;
        while(timer > 0f && showingWrong){
            timer -= Time.unscaledDeltaTime;
            yield return null;
        }
        if(!showingWrong) yield break;
        if(!showingWrong) Debug.LogError("YIELD BREAK DIDN'T WORK!!!");


        showingWrong = false;

        //If we don't want a new question showing, then just hide it
        if(!pendingShow){
            wholeContainer.SetActive(false);
        }
        
        //If we want a new question shown, then get a new one shown
        print("Are we pending New question: " + pendingNewQuestion + " | And Pending show: " + pendingShow);
        if(pendingNewQuestion && pendingShow){
            if(pendingNewQuestionInt != -1) DisplayQuestion(pendingNewQuestionInt);
            else DisplayQuestion();
            pendingNewQuestionInt = -1;
            pendingNewQuestion = false;
        }

        pendingShow = false;
    }
    #endregion
}
