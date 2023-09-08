using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Answer    //what kind of answer is it? what is the answer? for inspector
{
    [SerializeField] private string info;
    public string Info { get { return info; } }

    [SerializeField] private bool isCorrect;
    public bool IsCorrect { get { return isCorrect; } }
}

[CreateAssetMenu(fileName = "NewQuestion", menuName = "Quiz/new Question")] //sets the folder to get questions from
public class Question : ScriptableObject
{
    public enum AnswerType { Multi, Single }

    [SerializeField] private string info = string.Empty;    //to view in inspector
    public string Info { get { return info; } }     //let other scripts view this

    [SerializeField] Answer[] answers = null;
    public Answer[] Answers { get { return answers; } }

    [SerializeField] private bool useTimer = false;
    public bool UseTimer { get { return useTimer; } }

    [SerializeField] private int timer = 0;
    public int Timer { get { return timer; } }

    [SerializeField] private AnswerType answerType = AnswerType.Multi;
    public AnswerType GetAnswerType { get { return answerType; } }

    [SerializeField] private int addScore = 10;
    public int AddScore { get { return addScore; } }

    public List<int> GetCorrectAnswers()    //returns the list of correct answers
    {
        List<int> CorrectAnswers = new List<int>();
        //loop through all the answers to get correct one
        for(int i = 0; i < Answers.Length; i++)
        {
            if(Answers[i].IsCorrect)
            {
                CorrectAnswers.Add(i);
            }
        }
        return CorrectAnswers;
    }
}
