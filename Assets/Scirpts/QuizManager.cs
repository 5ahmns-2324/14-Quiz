using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Collections;

[System.Serializable]
public class Answer
{
    public string answerText;
    public bool isCorrect;
}

public class Question
{
    public string questionText;
    public Sprite questionSprite;
    public List<Answer> answers;

    public Question(string questionText, Sprite questionSprite, List<Answer> answers)
    {
        this.questionText = questionText;
        this.questionSprite = questionSprite;
        this.answers = answers;
        Debug.Log(questionSprite.name);
    }
}

public class QuizManager : MonoBehaviour
{
    public TMP_Text questionText;
    public Image questionImage;
    public List<Button> answerButtons;
    public Button nextButton;
    public Button checkButton;
    public TMP_Text progressText;
    public TMP_Text timerText;
    public TMP_Text scoreText;
    public Canvas endCanvas;
    public float timerDuration = 10f;

    private List<Question> questions;
    private List<Question> shuffledQuestions;
    private int currentQuestionIndex = 0;
    private float timer;
    private int correctAnswersCount = 0;
    private int score = 0;
    private List<int> selectedAnswers = new List<int>();
    private bool isChecking = false;

    void Start()
    {
        Debug.Log("QuizManager gestartet.");
        LoadQuestions();
        ShuffleQuestions();
        UpdateUI();
        StartTimer();
        Debug.Log("Fragen geladen. Anzahl: " + questions.Count);
        Sprite questionSprite = Resources.Load<Sprite>("pic_2");
        Debug.Log(questionSprite.name);
    }

    void LoadQuestions()
    {
        questions = new List<Question>
        {
            new Question
            (
                "Welches Glücksbärchi hat einen Regenbogen als Bauchzeichen?",
               Resources.Load<Sprite>("pic_1"),

                new List<Answer>
                {
                    new Answer { answerText = "Sonnenscheinbärchi", isCorrect = true },
                    new Answer { answerText = "Glückbärchi", isCorrect = false },
                    new Answer { answerText = "Freundschaftsbärchi", isCorrect = false }
                }
            ),
            new Question
            (
                "Welches Glücksbärchi ist für seinen Optimismus bekannt?",
                Resources.Load<Sprite>("pic_2"),
                new List<Answer>
                {
                    new Answer { answerText = "Glückbärchi", isCorrect = true },
                    new Answer { answerText = "Grummelbärchi", isCorrect = false },
                    new Answer { answerText = "Harmoniebärchi", isCorrect = false }
                }
            ),
            new Question
            (
                "Was ist das Symbol auf dem Bauch von Gute-Laune-Bärchi?",
               Resources.Load<Sprite>("pic_3"),
                new List<Answer>
                {
                    new Answer { answerText = "Lächelndes Gesicht", isCorrect = false },
                    new Answer { answerText = "Blume", isCorrect = false },
                    new Answer { answerText = "Musiknote", isCorrect = true }
                }
            ),
            new Question
            (
                "Welches Glücksbärchi trägt einen Stern auf seinem Bauch?",
                Resources.Load<Sprite>("pic_4"),
                new List<Answer>
                {
                    new Answer { answerText = "Sternenbärchi", isCorrect = true },
                    new Answer { answerText = "Schlummerbärchi", isCorrect = false },
                    new Answer { answerText = "Wunderbärchi", isCorrect = false }
                }
            ),
            new Question
            (
                "Was repräsentiert Glanzbärchi auf seinem Bauch?",
               Resources.Load<Sprite>("pic_5"),
                 new List<Answer>
                {
                    new Answer { answerText = "Funkelnder Regenbogen", isCorrect = true },
                    new Answer { answerText = "Glänzender Stern", isCorrect = false },
                    new Answer { answerText = " Glitzender Diamant", isCorrect = false }
                }
            ),
        };
    }

    void ShuffleQuestions()
    {
        shuffledQuestions = new List<Question>(questions);
        System.Random rng = new System.Random();
        int n = shuffledQuestions.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            Question value = shuffledQuestions[k];
            shuffledQuestions[k] = shuffledQuestions[n];
            shuffledQuestions[n] = value;
        }
    }

    void UpdateUI()
    {
        Question currentQuestion = shuffledQuestions[currentQuestionIndex];
        questionText.text = currentQuestion.questionText;
        questionImage.sprite = currentQuestion.questionSprite;

        for (int i = 0; i < answerButtons.Count; i++)
        {
            answerButtons[i].gameObject.SetActive(i < currentQuestion.answers.Count);
            if (i < currentQuestion.answers.Count)
            {
                int index = i;
                answerButtons[i].GetComponentInChildren<TMP_Text>().text = currentQuestion.answers[i].answerText;
                answerButtons[i].image.color = Color.white;
                answerButtons[i].interactable = true;
                answerButtons[i].onClick.RemoveAllListeners();
                answerButtons[i].onClick.AddListener(() => AnswerSelected(index));
            }
        }

        nextButton.interactable = false;
        checkButton.interactable = false;
        progressText.text = $"{correctAnswersCount}/{shuffledQuestions.Count}";
        scoreText.text = $"Score: {score}";
    }

    public void AnswerSelected(int answerIndex)
    {
        if (!isChecking)
        {
            if (!selectedAnswers.Contains(answerIndex))
            {
                selectedAnswers.Add(answerIndex);
                answerButtons[answerIndex].image.color = Color.blue;
            }
            else
            {
                selectedAnswers.Remove(answerIndex);
                answerButtons[answerIndex].image.color = Color.white;
            }

            if (selectedAnswers.Count > 0)
            {
                checkButton.interactable = true;
            }
            else
            {
                checkButton.interactable = false;
            }
        }
    }

    public void NextQuestion()
    {
        Debug.Log("NextQuestion aufgerufen! currentQuestionIndex: " + currentQuestionIndex);
        currentQuestionIndex++;
        selectedAnswers.Clear();
        StopTimer();

        if (currentQuestionIndex < shuffledQuestions.Count)
        {
            UpdateUI();
            StartTimer(); // Starte den Timer nur, wenn es mehr Fragen gibt
            Debug.Log("Nächste Frage geladen.");

            // Deaktiviere die Interaktivität der Antwortbuttons
            foreach (var button in answerButtons)
            {
                button.interactable = true;
            }
        }
        else if (currentQuestionIndex == shuffledQuestions.Count)
        {
            ShowEndMessage();
        }
    }

    void StartTimer()
    {
        timer = timerDuration;
        InvokeRepeating("UpdateTimer", 0f, 1f);
    }

    void StopTimer()
    {
        CancelInvoke("UpdateTimer");
    }

    void UpdateTimer()
    {
        timer -= 1f;
        timerText.text = timer.ToString();
        if (timer <= 0f || Mathf.Approximately(timer, 0f))
        {
            Debug.Log("Time's up!");
            StopTimer();
            NextQuestion();
        }
    }

    void ShowEndMessage()
    {
        Debug.Log("Quiz beendet! Herzlichen Glückwunsch!");
        endCanvas.gameObject.SetActive(true);
        StopTimer();
        scoreText.text = "Score: " + score;
    }

    public void CheckAnswers()
    {
        if (selectedAnswers.Count > 0 && !isChecking)
        {
            isChecking = true;
            StopTimer();
            checkButton.interactable = false;

            for (int i = 0; i < answerButtons.Count; i++)
            {
                answerButtons[i].interactable = false;
                if (shuffledQuestions[currentQuestionIndex].answers[i].isCorrect)
                {
                    answerButtons[i].image.color = Color.green;
                    correctAnswersCount++;
                    score += 10;
                }
                else
                {
                    answerButtons[i].image.color = Color.red;
                    score -= 5;
                }
            }

            UpdateProgressText();
            StartCoroutine(ResetCheckButton());
        }
    }

    IEnumerator ResetCheckButton()
    {
        yield return new WaitForSeconds(1.0f);

        // Setze den CheckButton zurück
        checkButton.interactable = true;

        // Nächste Frage aktivieren
        nextButton.interactable = true;

        // Buttons für die Antworten wieder klickbar machen
        foreach (var button in answerButtons)
        {
            button.image.color = Color.white;
            button.interactable = true;
        }

        NextQuestion();
    }
    void UpdateProgressText()
    {
        progressText.text = $"{correctAnswersCount}/{shuffledQuestions.Count}";
        scoreText.text = $"Score: {score}";
        nextButton.interactable = true;
    }
}
