using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuizRNG : MonoBehaviour
{
    public Quiz RandomQuiz()
    {
        Debug.LogWarning("THIS IS NOT GOOD! Don't use QuizRNG, it generates bad quizcodes!");
        //Make a question array
        Question[] theQuestions = new Question[]{
            RandomQuestion(),
            RandomQuestion(),
            RandomQuestion(),
            RandomQuestion(),
            RandomQuestion(),
            RandomQuestion(),
            RandomQuestion(),
        };

        //Make a Quiz with that question array
        return new Quiz{
            quizCode = 321,
            quizName = RandomString(10),
            questions = theQuestions
        };
    }
    Question RandomQuestion(){
        //Make an array of Answers:
        Answer[] theAnswers = new Answer[]{
            RandomAnswer(true),
            RandomAnswer(false),
            RandomAnswer(false),
            RandomAnswer(false),
        };

        //Make a question with that array
        return new Question{
            questionText = RandomString(50) + "?",
            answers = theAnswers
        };
    }

    Answer RandomAnswer(bool isItRight){
        //Return a random Answer

        if(isItRight)
        {
            return new Answer{
                answerText = RandomString(20) + "YAY!",
                isRight = true
            };
        }

        return new Answer{
            answerText = RandomString(20),
            isRight = false
        };
    }
    string RandomString(int length){
        //Here are the characters it can contain
        string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        //Make a string that we will return
        string final = "";
        
        //Populate the string
        for (int i = 0; i < length; i++)
        {
            final += characters[UnityEngine.Random.Range(0, characters.Length)];
        }
        
        //return it
        return final;
    }
}
