using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameEvents", menuName = "Quiz/new GameEvents")]
public class GameEvents : ScriptableObject
{
    //updates the new question UI. invoke this delegate, UIManager receives it and display the new question
    public delegate void UpdateQuestionCallback(Question question);
    public UpdateQuestionCallback UpdateQuestionUI;

    //updates the answer UI. when invoked, determines what answer picked
    public delegate void UpdateQuestionAnswerCallback(AnswerData pickedAnswer);
    public UpdateQuestionAnswerCallback UpdateQuestionAnswer;

    //when question is finished, update the resolution screen, correct, incorrect, or final screen
    public delegate void DisplayResolutionScreenCallback(UIManager.ResolutionScreenType type, int score);
    public DisplayResolutionScreenCallback DisplayResolutionScreen;

    //update the score UI
    public delegate void ScoreUpdatedCallback();
    public ScoreUpdatedCallback ScoreUpdated;

    [HideInInspector]
    public int CurrentFinalScore;
    [HideInInspector]
    public int StartupHighScore;
}
