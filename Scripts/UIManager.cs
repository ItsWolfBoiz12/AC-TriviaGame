using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

[Serializable()]
public struct UIManagerParameters //references to the UIParameters and show them in the inspector
{
    [Header("AnswersOptions")]
    [SerializeField] float margins;
    public float Margins { get { return margins; } }

    [Header("ResolutionScreenOptions")]
    [SerializeField] Color correctBGColor;
    public Color CorrectBGColor { get { return correctBGColor; } }

    [SerializeField] Color incorrectBGColor;
    public Color IncorrectBGColor { get { return incorrectBGColor; } }

    [SerializeField] Color finalBGColor;
    public Color FinalBGColor { get { return finalBGColor; } }

    [SerializeField] Color startBGColor;
    public Color StartBGColor { get {return startBGColor; } }

}

[Serializable()]
public struct UIElements //references to the UIElements and show them in the inspector to be able to assign
{
    [SerializeField] RectTransform answersContentArea;
    public RectTransform AnswersContentArea { get { return answersContentArea; } }
    
    [SerializeField] TextMeshProUGUI questionInfoTextObject;
    public TextMeshProUGUI QuestionInfoTextObject { get { return questionInfoTextObject; } }
    
    [SerializeField] TextMeshProUGUI scoreText;
    public TextMeshProUGUI ScoreText { get { return scoreText; } }
    [Space]
    
    [SerializeField] Animator resolutionScreenAnimator;
    public Animator ResolutionScreenAnimator { get { return resolutionScreenAnimator; } }

    [SerializeField] Image resolutionBG;
    public Image ResolutionBG { get { return resolutionBG; } }
    
    [SerializeField] TextMeshProUGUI resolutionStateInfoText;
    public TextMeshProUGUI ResolutionStateInfoText { get { return resolutionStateInfoText; } }

    [SerializeField] TextMeshProUGUI resolutionScoreText;
    public TextMeshProUGUI ResolutionScoreText { get { return resolutionScoreText; } }

    [Space]
    
    [SerializeField] TextMeshProUGUI highScoreText;
    public TextMeshProUGUI HighScoreText { get { return highScoreText; } }
    
    [SerializeField] CanvasGroup mainCanvasGroup;
    public CanvasGroup MainCanvasGroup { get { return mainCanvasGroup; } }
    
    [SerializeField] RectTransform finishUIElements;
    public RectTransform FinishUIElements { get { return finishUIElements; } }
}
public class UIManager : MonoBehaviour
{
    public enum ResolutionScreenType { Correct, Incorrect, Finish, Start } //sets the resolution screen states

    [Header("References")]
    [SerializeField] GameEvents events;

    [Header("UI Elements (Prefabs)")]
    [SerializeField] AnswerData answerPrefab;

    [SerializeField] UIElements uIElements;

    [Space]
    [SerializeField] UIManagerParameters parameters;

    List<AnswerData> currentAnswer = new List<AnswerData>(); //keeps track of what kind of answers there are
    private int resolutionStateParaHash = 0;

    private IEnumerator IE_DisplayTimeResolution; //sets how long each resolution screen displays

    private void OnEnable()
    {
        events.UpdateQuestionUI += UpdateQuestionUI;
        events.DisplayResolutionScreen += DisplayResolution;
        events.ScoreUpdated += UpdateScoreUI;
    }

    private void OnDisable()
    {
        events.UpdateQuestionUI -= UpdateQuestionUI;
        events.DisplayResolutionScreen -= DisplayResolution;
        events.ScoreUpdated -= UpdateScoreUI;
    }

    private void Start()
    {
        UpdateScoreUI();
        resolutionStateParaHash = Animator.StringToHash("ScreenState"); //accesses the animation to change screen
    }

    void DisplayResolution(ResolutionScreenType type, int score) //updates which screen to show
    {
        UpdateResolutionUI(type, score);
        uIElements.ResolutionScreenAnimator.SetInteger(resolutionStateParaHash, 2); //show the animation for screen
        uIElements.MainCanvasGroup.blocksRaycasts = false; //block buttons on main canvas when correct, incorrect, and final screen shows

        if (type != ResolutionScreenType.Finish)
        {
            if(IE_DisplayTimeResolution != null)
            {
                StopCoroutine(IE_DisplayTimeResolution);
            }
            IE_DisplayTimeResolution = DisplayTimedResolution();
            StartCoroutine(IE_DisplayTimeResolution);
        }
    }

    IEnumerator DisplayTimedResolution() //sets the time to display new resolution screen
    {
        yield return new WaitForSeconds(GameUtility.ResolutionDelayTime); //gets the time we set in game utilities
        uIElements.ResolutionScreenAnimator.SetInteger(resolutionStateParaHash, 1);
        uIElements.MainCanvasGroup.blocksRaycasts = true;
    }

    void UpdateResolutionUI(ResolutionScreenType type, int score)
    {
        var highscore = PlayerPrefs.GetInt(GameUtility.SavePrefKey); //gets the key we stored for the highscore int

        switch (type) //switches screens/plays animations for correct, incorrect, or final screen
        {
            case ResolutionScreenType.Correct:
                uIElements.ResolutionBG.color = parameters.CorrectBGColor; //sets background color
                uIElements.ResolutionStateInfoText.text = "CORRECT!"; //shows the text
                uIElements.ResolutionScoreText.text = "+" + score; //displays the score
                break;
            case ResolutionScreenType.Incorrect:
                uIElements.ResolutionBG.color = parameters.IncorrectBGColor;
                uIElements.ResolutionStateInfoText.text = "WRONG!";
                uIElements.ResolutionScoreText.text = "-" + score;
                break;
            case ResolutionScreenType.Finish:
                uIElements.ResolutionBG.color = parameters.FinalBGColor;
                uIElements.ResolutionStateInfoText.text = "Final Score!";

                StartCoroutine(CalculateScore());
                uIElements.FinishUIElements.gameObject.SetActive(true);
                uIElements.HighScoreText.gameObject.SetActive(true);
                //display highscore. compares the highscore to the startup highscore and if its a new highscore, display it in yellow
                uIElements.HighScoreText.text = ((highscore > events.StartupHighScore) ? "<color=yellow>NEW </color>" : string.Empty) + "Highscore: " + highscore;
                break;
            default:
                break;
        }
    }

    IEnumerator CalculateScore() 
    {
        var scoreValue = 0;
        while(scoreValue < events.CurrentFinalScore)
        {
            scoreValue++;
            uIElements.ResolutionScoreText.text = scoreValue.ToString(); //updates the score on screen

            yield return null;
        }
    }

    void UpdateQuestionUI(Question question) //display questions information text
    {
        uIElements.QuestionInfoTextObject.text = question.Info;
        CreateAnswers(question);
    }

    void CreateAnswers(Question question)
    {
        EraseAnswers();

        float offset = 0 - parameters.Margins;
        for (int i = 0; i < question.Answers.Length; i++) //create the new answers by looping through all the questions answers
        {
            AnswerData newAnswer = (AnswerData)Instantiate(answerPrefab, uIElements.AnswersContentArea);
            newAnswer.UpdateData(question.Answers[i].Info, i); //pass through the new answers

            newAnswer.Rect.anchoredPosition = new Vector2(0, offset); //position the answers correctly

            offset -= (newAnswer.Rect.sizeDelta.y + parameters.Margins);
            uIElements.AnswersContentArea.sizeDelta = new Vector2(uIElements.AnswersContentArea.sizeDelta.x, offset * -1); //resize rect transform for new answers

            currentAnswer.Add(newAnswer); //display the answers
        }
    }

    void EraseAnswers() //destroy elements in the current answer list to make room for the next ones
    {
        foreach (var answer in currentAnswer)
        {
            Destroy(answer.gameObject);
        }
        currentAnswer.Clear();
    }

    void UpdateScoreUI() //updates the score UI during game
    {
        uIElements.ScoreText.text = "Score: " + events.CurrentFinalScore;
    }
}
