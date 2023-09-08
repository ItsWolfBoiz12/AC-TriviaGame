using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    Question[] questions = null; //makes the question array in inspector
    public Question[] Questions { get { return questions; } }

    [SerializeField] GameEvents events = null; 

    [SerializeField] Animator timerAnimator = null; //gets animator in inspector
    [SerializeField] TextMeshProUGUI timerText = null; //gets timer text in inspector
    [SerializeField] Color timerHalfwayColor = Color.red; //changes the color of timer text depending on state
    private Color timerDefaultColor; //sets default timer color

    private List<AnswerData> PickedAnswers = new List<AnswerData>();
    private List<int> FinishedQuestions = new List<int>();  //adds the question into the array when finished to make sure not to repeat them
    private int currentQuestion = 0;

    private IEnumerator IE_WaitUntilNextRound = null;
    private IEnumerator IE_StartTimer;

    private int timerStateParaHash= 0;

    void UpdateTimer(bool state)
    {
        switch (state)
        {
            //starts the timer based on parahash we set in inspector
            case true:
                IE_StartTimer = StartTimer();
                StartCoroutine(IE_StartTimer);

                timerAnimator.SetInteger(timerStateParaHash, 2);
                break;
            //stops the timer based on parahash we set in inspector
            case false:
                if (IE_StartTimer != null)
                {
                    StopCoroutine(IE_StartTimer);
                }

                timerAnimator.SetInteger(timerStateParaHash, 1);
                break;
        }
    }

    IEnumerator StartTimer()
    {
        var totalTime = Questions[currentQuestion].Timer; //gets the total time for timer
        var timeLeft = totalTime;

        timerText.color = timerDefaultColor;
        //while the timer is going, countdown
        while (timeLeft > 0)
        {
            timeLeft--;

            AudioManager.Instance.PlaySound("TimerSFX"); //play the timer sound

            if (timeLeft < totalTime / 2) //when timer is halfway gone, change the color
            {
                timerText.color = timerHalfwayColor;
            }

            timerText.text = timeLeft.ToString(); //show the number on the timer text
            yield return new WaitForSeconds(1.0f); //how fast the timer goes
        }
        Accept();
    }

    private bool IsFinished //have we finished the game
    {
        get
        {
            return (FinishedQuestions.Count < Questions.Length) ? false : true; //are the finished questions less than the questions in the list? if not then the game is not finished
        }
    }

    void OnEnable()
    {
        events.UpdateQuestionAnswer += UpdateAnswers;
    }

    private void OnDisable()
    {
        events.UpdateQuestionAnswer -= UpdateAnswers;
    }

    private void Awake()
    {
        events.CurrentFinalScore = 0;
    }

    private void Start()
    {
        events.StartupHighScore = PlayerPrefs.GetInt(GameUtility.SavePrefKey); //gets the current highscore saved in the key in GameUtilities

        timerDefaultColor = timerText.color; //sets the default timer color

        LoadQuestions(); //load all the questions

        timerStateParaHash = Animator.StringToHash("TimerState"); //sets the timer state

        //makes sure the quiz is randomized everytime a new quiz starts
        var seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        UnityEngine.Random.InitState(seed);

        Display();
    }

    public void StartButton()
    {
        RestartGame();
    }

    public void UpdateAnswers(AnswerData newAnswer) //updates the picked answers list by adding or removing the picked answer from the list
    {
        if(Questions[currentQuestion].GetAnswerType == Question.AnswerType.Single) //if single make sure there is only one answer picked
        {
            foreach (var answer in PickedAnswers)
            {
                if(answer != newAnswer)
                {
                    answer.Reset();
                }   
            }
            PickedAnswers.Clear();
            PickedAnswers.Add(newAnswer);
        }
        else
        {
            bool alreadyPicked = PickedAnswers.Exists(x => x == newAnswer); //is the answer already been picked
            if (alreadyPicked)
            {
                PickedAnswers.Remove(newAnswer); //if so remove it
            }
            else
            {
                PickedAnswers.Add(newAnswer); //if not add it
            }
        }
    }

    public void EraseAnswers() //creates new list for answer data
    {
        PickedAnswers = new List<AnswerData>();
    }

    void Display()  //display new question
    {
        EraseAnswers();
        var question = GetRandomQuestion();

        if(events.UpdateQuestionUI != null) //check if question area is null. if not, display question
        {
            events.UpdateQuestionUI(question);
        }
        else
        {
            Debug.Log("Something went wrong while trying to diplay new Question UI Data. GameEvents.UpdateQuestionUI is null. Issue occured in GameManager.Display() method.");
        }

        if (question.UseTimer)
        {
            UpdateTimer(question.UseTimer);
        }
    }

    public void Accept() //accepts the answers the users picked and determines if the answer is correct or not. this adds the questions to a finished question array to make sure the same questions are never duplicated
    {
        UpdateTimer(false);
        bool isCorrect = CheckAnswers();
        FinishedQuestions.Add(currentQuestion);

        UpdateScore((isCorrect) ? Questions[currentQuestion].AddScore : -Questions[currentQuestion].AddScore); //add or remove score depending on if correct or not

        if (IsFinished) //if the game is finished, call SetHighScore
        {
            SetHighScore();
        }

        //display the right type of resolution screen depending on the state of game
        var type = (IsFinished) ? UIManager.ResolutionScreenType.Finish : (isCorrect) ? UIManager.ResolutionScreenType.Correct : UIManager.ResolutionScreenType.Incorrect;

        if(events.DisplayResolutionScreen != null) //gets the type of resolution screen, adjusts score accordingly
        {
            events.DisplayResolutionScreen(type, Questions[currentQuestion].AddScore);
        }

        AudioManager.Instance.PlaySound((isCorrect) ? "CorrectSFX" : "IncorrectSFX"); //if answer is correct play correct sound, if not play incorrect sound

        if (type != UIManager.ResolutionScreenType.Finish) //if the resolution screen != finish, wait for next question
        {
            if (IE_WaitUntilNextRound != null)
            {
                StopCoroutine(IE_WaitUntilNextRound);
            }
            IE_WaitUntilNextRound = WaitUntilNextRound();
            StartCoroutine(IE_WaitUntilNextRound);
        }
    }

    IEnumerator WaitUntilNextRound() //wait to display new question
    {
        yield return new WaitForSeconds(GameUtility.ResolutionDelayTime);
        Display();
    }

    Question GetRandomQuestion()    //choose a random question
    {
        var randomIndex = GetRandomQuestionIndex();     //store random index for question
        currentQuestion = randomIndex;

        return Questions[currentQuestion];
    }
    
    int GetRandomQuestionIndex()
    {
        var random = 0;
        if (FinishedQuestions.Count < Questions.Length) //check if finished all questions
        {
            do //if more questions need to be finished, pick a random question from the array that hasnt been used already
            {
                random = UnityEngine.Random.Range(0, Questions.Length);
            } while (FinishedQuestions.Contains(random) || random == currentQuestion);
        }
        return random;
    }

    void LoadQuestions()
    {
        //go to the questions folder and load all the questions into an array
        Object[] objs = Resources.LoadAll("Questions", typeof(Question));
        questions = new Question[objs.Length];
        for (int i = 0; i < objs.Length; i++)   //iterate through all the questions
        {
            questions[i] = (Question)objs[i];
        }
    }

    public void RestartGame() //play again button function
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame() //quit button function.
    {
        Application.Quit();
    }

    private void SetHighScore()
    {
        var highscore = PlayerPrefs.GetInt(GameUtility.SavePrefKey); //get the key from game utilities
        if (highscore < events.CurrentFinalScore) //if the current score is higher than highscore, replace highscore
        {
            PlayerPrefs.SetInt(GameUtility.SavePrefKey, events.CurrentFinalScore);
        }
    }

    bool CheckAnswers() //is the answer correct?
    {
        if (!CompareAnswers())
        {
            return false;
        }
        return true;
    }
    bool CompareAnswers() //compare the picked answers to the correct ones
    {
        if(PickedAnswers.Count > 0)
        {
            List<int> c = Questions[currentQuestion].GetCorrectAnswers(); //correct answers
            List<int> p = PickedAnswers.Select(x => x.AnswerIndex).ToList(); //picked answers

            //compare the answers
            var f = c.Except(p).ToList();
            var s = p.Except(c).ToList();

            return !f.Any() && !s.Any(); //return the state, incorrect or correct
        }
        return false;
    }

    private void UpdateScore(int add) //add or subtract score depending on what answer the user picks
    {
        events.CurrentFinalScore += add;

        if (events.ScoreUpdated != null)
        {
            events.ScoreUpdated();
        }
    }
}
