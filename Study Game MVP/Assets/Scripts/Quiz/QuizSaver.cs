using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.VisualScripting;

public class QuizSaver : MonoBehaviour
{
    public static QuizSaver instance;
    public static string SAVE_FOLDER = "/Saved_Quizes/";

    public static List<Quiz> lastQuizzes;
    // Start is called before the first frame update
    void Awake() {
        if(instance == null){
            instance = this;
        }else{
            Destroy(this);
            return;
        }
        DontDestroyOnLoad(this);
        
        //Fill up the SAVE_FOLDER
        SAVE_FOLDER = Application.persistentDataPath + "/Saved_Quizes/";
        
        if(!Directory.Exists(SAVE_FOLDER)){
            Directory.CreateDirectory(SAVE_FOLDER);
        }
    }

    
    #region Save Quizzes
    public async Task<string> SaveQuiz(Quiz quiz){
        string save = JsonUtility.ToJson(quiz);
        int saveNum = 1;
        while(File.Exists(SAVE_FOLDER + "/save" + saveNum + ".qiz")){
            saveNum++;
        }
        await File.WriteAllTextAsync(SAVE_FOLDER + "/save" + saveNum +".qiz", save);
        return SAVE_FOLDER + "/save" + saveNum +".qiz";
    }

    public async Task<string> SaveQuiz(Quiz quiz, string fileLocation = ""){
        if(fileLocation == ""){
            return await SaveQuiz(quiz);
        }
        print("made it here with quiz at: " + fileLocation);
        string save = JsonUtility.ToJson(quiz);
        await File.WriteAllTextAsync(fileLocation, save);
        return fileLocation;
    }
    #endregion

    #region Load Quizzes
    public async Task<Quiz> LoadQuiz(){
        //First, lets find the file with the highest number
        int saveNum = 1;
        while(File.Exists(SAVE_FOLDER + "/save" + saveNum + ".qiz")){
            saveNum++;
        }
        saveNum -= 1;
        string saveFile = SAVE_FOLDER + "/save" + saveNum +".qiz";
        if(!File.Exists(saveFile)) return new Quiz();
        string load = await File.ReadAllTextAsync(saveFile);
        Quiz loadedQuiz = JsonUtility.FromJson<Quiz>(load);

        return loadedQuiz;
    }
    public async Task<Quiz> LoadQuiz(string fileLocation){
        if(!File.Exists(fileLocation)) return new Quiz();
        string load = await File.ReadAllTextAsync(fileLocation);
        Quiz loadedQuiz = JsonUtility.FromJson<Quiz>(load);

        return loadedQuiz;
    }
    #endregion

    #region Delete Quizzes
    //This function deletes a quiz at a location
    public bool DeleteQuiz(string location){
        //First, check if the quiz even exists
        if(!File.Exists(location)){
            Debug.LogWarning("No quiz found at: " + location);
            return false;
        }

        File.Delete(location);
        return true;
    }
    #endregion
    
    #region Find Quizzes
    //This finds all the quizzes on disk and returns them in a list
    public async Task<List<SavedQuiz>> FindAllQuizes(QuizMaker maker = null){
        //We first need to look for every file in the save location with the right name
        List<SavedQuiz> savedQuizzes = new List<SavedQuiz>();
        string saveFile;
        SavedQuiz savedQuiz;

        //Code stolen from: https://discussions.unity.com/t/getting-list-of-files-from-specified-folder/13641
        DirectoryInfo dir = new DirectoryInfo(SAVE_FOLDER);
        FileInfo[] info = dir.GetFiles("*.qiz*");
        foreach (FileInfo f in info) 
        { 
            //Note the save location
            saveFile = f.DirectoryName + "\\" + f.Name;
            //Extract the text from that save location
            string load = await File.ReadAllTextAsync(saveFile);
            try{
                //Turn that text (JSON) into a quiz
                Quiz loadedQuiz = JsonUtility.FromJson<Quiz>(load);
                //Turn that Quiz into a SavedQuiz
                savedQuiz = QuizToSavedQuiz(loadedQuiz, saveFile);
                //Add that saved quiz to the list
                savedQuizzes.Add(savedQuiz);
            }catch {}
            
        }

        if(savedQuizzes != null) {
            if(maker != null) maker.SpwanQuizButtons(savedQuizzes);
            return savedQuizzes;
        }
        else{
            if(maker != null) maker.SpwanQuizButtons(null);
            Debug.LogWarning("No saved quizzes found");
            return null;
        }
    }
    SavedQuiz QuizToSavedQuiz(Quiz quiz, string save){
        return new SavedQuiz{
            quizName = quiz.quizName,
            saveLocation = save,
            quizCode = quiz.quizCode
        };
    }

    public bool IsMoreThanOneQuiz(){
        //Code stolen from: https://discussions.unity.com/t/getting-list-of-files-from-specified-folder/13641
        DirectoryInfo dir = new DirectoryInfo(SAVE_FOLDER);
        FileInfo[] info = dir.GetFiles("*.qiz*");
        foreach (FileInfo f in info) 
        { 
            //Note the save location
            string saveFile = f.DirectoryName + "\\" + f.Name;
            //Extract the text from that save location
            string load = File.ReadAllText(saveFile);
            try{
                //Turn that text (JSON) into a quiz
                Quiz loadedQuiz = JsonUtility.FromJson<Quiz>(load);
                //Turn that Quiz into a SavedQuiz
                SavedQuiz savedQuiz = QuizToSavedQuiz(loadedQuiz, saveFile);
                
                //If we made it this far, then we haver more than one quiz
                return true;
            }catch {}
            
        }

        //if we didn't find any quizzes, then we have none
        return false;
    }
    #endregion

    #region WebGL Quiz Stuff

    public void AddQuizWebGL(Quiz quiz){
        print("Saving Quiz in WEBGL!");
        lastQuizzes.Add(quiz);

        //If there are too many, then lets remove the oldest one
        if(lastQuizzes.Count > 4){
            lastQuizzes.RemoveRange(0, 1);
        }
    }

    #endregion
}
