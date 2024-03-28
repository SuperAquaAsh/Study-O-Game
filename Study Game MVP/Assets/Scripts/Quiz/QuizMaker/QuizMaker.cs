using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

//This handles the Quiz storing in the quiz editor and comunicates with the QuizSaver to save it
public class QuizMaker : MonoBehaviour
{
    public static QuizMaker instance;

    [Header("Inputs")]
    [SerializeField] TMP_InputField QuestionInput;
    [SerializeField] TMP_InputField QuizNameInput;
    [SerializeField] GameObject AnswerParent;
    TMP_InputField[] AnswerInputs = new TMP_InputField[4];
    Toggle[] AnswerToggles = new Toggle[4];

    [Header("UI")]
    [SerializeField] QuizMakerScrollArea quizScroll;

    [Header("QuizSelection")]
    [SerializeField] GameObject selectorGO;
    [SerializeField] GameObject QuizButton;

    [Header("Section Parent Objects")]
    [SerializeField] GameObject editorParent;
    [SerializeField] GameObject selectorParent;
    [SerializeField] ScrollRect selectorScroll;
    [SerializeField] GridLayoutGroup grid;

    [Header("Animations")]
    [SerializeField] AnimationCurve gridSlideCurve;

    Quiz editingQuiz;
    Quiz oldQuiz;
    string quizLocation;
    int currentQuestion = 0;
    void Awake() {
        if(instance == null){
            instance = this;
        }else{
            Debug.LogWarning("Two Instances of QuizMaker Found!");
            Destroy(this);
        }
    }
    #region Initialization Functions
    
    void InitializeEditor(bool newQuiz){
        selectorParent.SetActive(false);
        editorParent.SetActive(true);
        
        SetAnswerObjects(ref AnswerInputs, ref AnswerToggles, AnswerParent);
        if(newQuiz) {
            CreateNewQuiz();
            quizLocation = "";
        }

        currentQuestion = 0;
        //Tell the Sidebar to update
        quizScroll.UpdateFromQuiz();

        //Tell the quiz to display the first question
        ChangeQuestion(0);

        //Get the right name showing
        QuizNameInput.text = editingQuiz.quizName;
    }

    public async void InitializeSelector(){
        editorParent.SetActive(false);
        selectorParent.SetActive(true);
        
        DeleteChildren(grid.transform);
        await QuizSaver.instance.FindAllQuizes(this);

        //Start animation AFTER it's done loading the quizzes
        StartCoroutine(GridSlide(0.5f));
    }

    #endregion

    #region SetData
    public void SetQuestion(string question){
        editingQuiz.questions[currentQuestion].questionText = question;
        quizScroll.ChangeQuestionTitle(currentQuestion, question);
    }

    #region SetAnswers

    public void SetAnswerText(string answer, int ID){
        editingQuiz.questions[currentQuestion].answers[ID].answerText = answer;
        //print("Question 0's first answer is: " + editingQuiz.questions[0].answers[0].answerText);
    }

    public void SetAnswerBool(bool IsRight, int ID){
        editingQuiz.questions[currentQuestion].answers[ID].isRight = IsRight;

        if(IsRight) quizScroll.SetQuestionWarning(true);
        else quizScroll.SetQuestionWarning(CheckForCorrectAnswer(currentQuestion));
    }

    public void SetQuizName(string name){
        editingQuiz.quizName = name;
    }
    #endregion

    #endregion

    #region Editor Functions
    public void ChangeQuestion(int QuestionID){
        
        print("I'm changing the question to question: " + QuestionID);
        currentQuestion = QuestionID;
        QuestionInput.text = editingQuiz.questions[currentQuestion].questionText;

        for (int i = 0; i < AnswerInputs.Length; i++)
        {
            AnswerInputs[i].text = editingQuiz.questions[currentQuestion].answers[i].answerText;
        }

        for (int i = 0; i < AnswerToggles.Length; i++)
        {
            AnswerToggles[i].isOn = editingQuiz.questions[currentQuestion].answers[i].isRight;
        }

        //Then, have the button that corosponds to this question light up
        quizScroll.SetQuestionButton(QuestionID);
    }
    public void CreateNewQuestion(){
        //Create a new Array with one more data slot than the last
        Question[] newQuestions = new Question[editingQuiz.questions.Length + 1];

        //Populate that Array with the old ones data
        for (int i = 0; i < editingQuiz.questions.Length; i++)
        {
            AddQuestionToArray(ref newQuestions, i);
        }

        //Fill in the extra data spot with a brand new question
        newQuestions[editingQuiz.questions.Length] = new Question{
            questionText = "",
            answers = new Answer[] {
                new Answer {
                    answerText = "",
                    isRight = false,
                },
                new Answer {
                    answerText = "",
                    isRight = false,
                },
                new Answer {
                    answerText = "",
                    isRight = false,
                },
                new Answer {
                    answerText = "",
                    isRight = false,
                }
            }
        };
        //print("New question's answer at " + editingQuiz.questions.Length + " is equal to '" + newQuestions[editingQuiz.questions.Length].answers[0].answerText + "' when it should be " + EMPTY_QUESTION.answers[0].answerText);

        //Set the new array to the current editable question
        editingQuiz.questions = newQuestions;

        ChangeQuestion(newQuestions.Length - 1);

        //Tell the sidebar to update
        quizScroll.AddQuestion();
    }
    public void CreateNewQuiz(){
        Question emptyQuestion = new Question{
            questionText = "",
            answers = new Answer[] {
                new Answer {
                    answerText = "",
                    isRight = false,
                },
                new Answer {
                    answerText = "",
                    isRight = false,
                },
                new Answer {
                    answerText = "",
                    isRight = false,
                },
                new Answer {
                    answerText = "",
                    isRight = false,
                }
            }
        };

        editingQuiz = new Quiz{
            quizName = "New Quiz",
            questions = new Question[1] {emptyQuestion},
            quizCode = GenerateQuizCode()
        };
    }

    uint GenerateQuizCode(){
        int firstValue = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        uint final = ((uint)firstValue) + int.MaxValue;
        return final;
    }

    void SetAnswerObjects(ref TMP_InputField[] inputs, ref Toggle[] toggles, GameObject parentObj){
        AnswerInput[] foundInputs = parentObj.GetComponentsInChildren<AnswerInput>();
        foreach (var item in foundInputs)
        {
            if(inputs[item.AnswerID] == null) inputs[item.AnswerID] = item.inputField;
            if(toggles[item.AnswerID] == null) toggles[item.AnswerID] = item.toggle;
        }

    }

    public async void SaveQuiz(){
        //Change the quiz code every time you save (Only if the quiz changed)
        if(!oldQuiz.Equals(editingQuiz)) {
            editingQuiz.quizCode = GenerateQuizCode();

            //Remeber the save location
            quizLocation = await QuizSaver.instance.SaveQuiz(editingQuiz, quizLocation);
            //THIS COULD CAUSE ERRORS!!! PAST AQUA WAS TOO LAZY TO TEST THOUGH
        }

        
    }

    public void InitializeNewQuiz(){
        InitializeEditor(true);
    }

    //This checks if there are any right anwers in a given question
    public bool CheckForCorrectAnswer(int QuestionID = -1){
        if(QuestionID == -1) QuestionID = currentQuestion;
        
        bool containsRight = false;
        foreach (var item in editingQuiz.questions[QuestionID].answers){
            if(item.isRight) containsRight = true;
        }
        return containsRight;
    }
    void AddQuestionToArray(ref Question[] questions, int i, int iQuiz = -2){
        //This variable has to exist because sometimes the question we are refrencing has to be writen to a different part of the questions array
        if(iQuiz == -2){
            iQuiz = i;
        }
        
        //Create an empty question
        Question emptyQuestion = new Question{
            questionText = "",
            answers = new Answer[] {
                new Answer {
                    answerText = "",
                    isRight = false,
                },
                new Answer {
                    answerText = "",
                    isRight = false,
                },
                new Answer {
                    answerText = "",
                    isRight = false,
                },
                new Answer {
                    answerText = "",
                    isRight = false,
                }
            }
        };
        
        //questions[i].answers = new Answer[emptyQuestion.answers.Length];
        //Array.Copy(emptyQuestion.answers, questions[i].answers, emptyQuestion.answers.Length);

        questions[i].answers = new Answer[emptyQuestion.answers.Length];
        Array.Copy(editingQuiz.questions[iQuiz].answers, questions[i].answers, emptyQuestion.answers.Length);

        questions[i].questionText = editingQuiz.questions[iQuiz].questionText;

        return;

        for (int n = 0; n < editingQuiz.questions[iQuiz].answers.Length; n++)
        {
            questions[i].answers[n].answerText = editingQuiz.questions[iQuiz].answers[n].answerText;
            questions[i].answers[n].isRight = editingQuiz.questions[iQuiz].answers[n].isRight;
        }
    }
    public int GetCurrentQuestion() {return currentQuestion;}
    public Quiz GetQuiz() {return editingQuiz;}

    #endregion

    #region Selector Functions

    //This takes in a list of all the saved quizes on disk and spwans buttons with there characteristics
    public void SpwanQuizButtons(List<SavedQuiz> savedQuizzes){        
        if(savedQuizzes == null){
            //If nothing is on disk then don't do anything
            Debug.LogWarning("No quizzes Found on Disk");
            return;
        }

        //Delete all the old buttons
        DeleteChildren(selectorGO.transform);

        QuizButton button = null;
        foreach (var item in savedQuizzes)
        {
            //Spwan in buttons and set there values accordingly
            button = Instantiate(QuizButton, selectorGO.transform).GetComponent<QuizButton>();
            button.SetQuizData(item.saveLocation, item.quizName);
        }
    }

    public async void LoadQuiz(string saveLocation = ""){
        if(saveLocation == ""){
            InitializeEditor(true);
            return;
        }
        editingQuiz = await QuizSaver.instance.LoadQuiz(saveLocation);
        oldQuiz = editingQuiz;
        quizLocation = saveLocation;
        InitializeEditor(false);
    }

    void DeleteChildren(Transform trans){
        for (int i = 0; i < trans.childCount; i++)
        {
            Destroy(trans.GetChild(i).gameObject);
        }
    }

    #region Selector Animations

    IEnumerator GridSlide(float time){
        //Remember the original value we are aiming for
        float goal = grid.spacing.x;
        float start = -grid.cellSize.x;
        float startTime = time;

        grid.spacing = new Vector2(start, grid.spacing.y);

        while(time > 0f){
            grid.spacing = new Vector2(Mathf.Lerp(start, goal, gridSlideCurve.Evaluate(1 - (time / startTime))), grid.spacing.y);
            selectorScroll.normalizedPosition = new Vector2 (0, 1);
            time -= Time.deltaTime;
            yield return null;
        }

        grid.spacing = new Vector2(goal, grid.spacing.y);
        selectorScroll.normalizedPosition = new Vector2 (0, 1);
    }
    

    #endregion

    #endregion

    #region Delete Function (lol, only one function)
    public void DeleteQuestion(int questionID){
        //First, lets check if we can even delete a question
        if(editingQuiz.questions.Length < 2){
            Debug.LogWarning("Too few questions to delete!");
            return;
        }
        
        //We need to create a new array to store our new questions
        Question[] removedQuestions = new Question[editingQuiz.questions.Length - 1];

        int hasDeleted = 0;
        //Populate that Array with the old ones data (only if the question isn't the same as the old one)
        for (int i = 0; i < editingQuiz.questions.Length; i++)
        {
            if(i != questionID)
            {   
                AddQuestionToArray(ref removedQuestions, i - hasDeleted, i);
            }else {hasDeleted = 1;}
        }

        //Set that array to the questions
        editingQuiz.questions = removedQuestions;

        //Adjust the current question to fit the new array
        currentQuestion = Mathf.Clamp(currentQuestion, 0, editingQuiz.questions.Length - 1);
        
        //UPDATE SIDEBARs
        quizScroll.DeleteQuestion(questionID);
        //quizScroll.UpdateFromQuiz();

        //Change the question
        ChangeQuestion(currentQuestion);
    }

    #endregion

}
