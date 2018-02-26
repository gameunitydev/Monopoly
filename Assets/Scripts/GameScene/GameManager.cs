using System.Collections;
using System.Collections.Generic;
using Monopoly.Lobby_v2;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private int answeredQuestionsCount;
    private bool timerActive;
    private bool allAnswered;

    [HideInInspector]
    public bool answered;

    private Color32 colorProcess = new Color32(225, 216, 98, 255);
    private Color32 colorSuccess = new Color32(98, 225, 114, 255);
    private Color32 colorError = new Color32(225, 98, 98, 255);

    public PlayerInfo playerInfo;

    [SerializeField]
    private Transform playerFigureTransform;

    [SerializeField]
    private Transform[] waypointsTransforms;

    [Header("Question Window")]

    [SerializeField]
    private GameObject questionWindow;

    [SerializeField]
    private Text questionNumberText;

    [SerializeField]
    private Text questionText;

    [SerializeField]
    private Text questionResultText;

    [SerializeField]
    private GameObject[] answers_GOs;

    [Header("Timer")]

    [SerializeField]
    private Text timerText;

    [SerializeField]
    private Image timerFillImage;

    private void Start()
    {
        Debug.Log("GameManager started.");
        playerInfo.Refresh();
        StartGame();
    }

    private void Update()
    {
        // Host computer will only spectate
        //if (NetworkServer.active)
        //{
        //    return;
        //}
    }

    private void StartGame()
    {
        QuestionPopUp(answeredQuestionsCount);
    }

    private void QuestionPopUp(int questionNumber)
    {
        ClearWindow();

        // If there's still questions
        if (questionNumber < QuestionsList.questionsList.Count)
        {
            FillWindow();
            questionWindow.SetActive(true);

            Debug.Log("Question: " + (answeredQuestionsCount + 1) +
                      ". ScoreValue: " + QuestionsList.questionsList[questionNumber].scoreValue +
                      ". Right answerResult is: " + QuestionsList.questionsList[questionNumber].rightAnswerNumber +
                      ". Duration: " + QuestionsList.questionsList[questionNumber].duration);

            timerActive = true;
            StartCoroutine(StartTimer(QuestionsList.questionsList[questionNumber].duration));
        }
        // If questions are ended
        else
        {
            questionWindow.SetActive(false);
            Debug.Log("Final!");
        }
    }

    private IEnumerator StartTimer(int duration)
    {
        int timerTime = 0;
        timerActive = true;

        while (timerActive)
        {
            // Question End
            if (timerTime < duration /*|| !allAnswered || !answered*/)
            {
                yield return new WaitForSeconds(1);
                timerTime++;
                timerText.text = timerTime.ToString();
                timerFillImage.fillAmount += 1f / duration;
            }
            else
            {
                timerActive = false;
                yield return new WaitForSeconds(1);
                questionWindow.SetActive(false);

                // MoveFigures
                MoveFigures(playerFigureTransform, playerInfo.GetPlayerScore());
                Debug.Log("Question fade out");
                answeredQuestionsCount++;
                yield return new WaitForSeconds(5);
                QuestionPopUp(answeredQuestionsCount);
                yield break;
            }
        }
    }

    private void ClearWindow()
    {
        timerText.text = string.Empty;
        questionText.text = string.Empty;
        questionNumberText.text = string.Empty;
        timerFillImage.fillAmount = 0f;
    }

    private void FillWindow()
    {
        questionNumberText.text = (answeredQuestionsCount + 1).ToString();
        questionText.text = QuestionsList.questionsList[answeredQuestionsCount].questionText;
        timerText.text = "0";

        // ToDo Answers from QList
    }

    public void CheckAnswerClick(GameObject clickedAnswer)
    {
        // Badass logic
        for (int i = 0; i < answers_GOs.Length; i++)
        {
            if ((answers_GOs[i] == clickedAnswer) && (i + 1) == QuestionsList.questionsList[answeredQuestionsCount].rightAnswerNumber)
            {
                Debug.Log("Correct Answer!");
                playerInfo.AddPlayerScore(QuestionsList.questionsList[answeredQuestionsCount].scoreValue);
                StartCoroutine(MessageResult(true));
            }
            else if ((answers_GOs[i] == clickedAnswer) && (i + 1) != QuestionsList.questionsList[answeredQuestionsCount].rightAnswerNumber)
            {
                Debug.Log("Incorrect Answer!");
                StartCoroutine(MessageResult(false));
            }

            // ToDo In any case: Waiting for allAnswered OR timerEnded. Then Next question.
        }
    }

    // ToDO Handle if 0

    private void MoveFigures(Transform figureTransform, uint pointsGained)
    {
        //Debug.Log("Step: " + (pointsGained));
        Vector3 target = new Vector3(waypointsTransforms[pointsGained - 1].position.x, waypointsTransforms[pointsGained - 1].position.y, waypointsTransforms[pointsGained - 1].position.z);
        StartCoroutine(Movement(figureTransform, target));
    }

    private IEnumerator Movement(Transform figureTransform, Vector3 targetTransform)
    {
        // Will need to perform some of this process and yield until next frames
        const float closeEnough = 0.05f;
        float distance = (figureTransform.position - targetTransform).magnitude;

        // GC will trigger unless we define this ahead of time
        WaitForEndOfFrame wait = new WaitForEndOfFrame();

        // Continue until we're there
        while (distance >= closeEnough)
        {
            // Confirm that it's moving
            Debug.Log("Executing Movement");

            // Move a bit then  wait until next  frame
            figureTransform.position = Vector3.Slerp(figureTransform.position, targetTransform, 0.01f);
            yield return wait;

            // Check if we should repeat
            distance = (figureTransform.position - targetTransform).magnitude;
        }

        // Complete the motion to prevent negligible sliding
        figureTransform.position = targetTransform;

        // Confirm  it's ended
        Debug.Log("Movement Complete");
    }

    private IEnumerator MessageResult(bool answerResult)
    {
        const float delay = 3f;

        answered = true;
        questionWindow.SetActive(false);

        if (answerResult == true)
        {
            questionResultText.color = colorSuccess;
            questionResultText.text = "Correct!";
        }
        else
        {
            questionResultText.color = colorError;
            questionResultText.text = "Wrong!";
        }

        questionResultText.gameObject.SetActive(true);

        yield return new WaitForSeconds(delay);
        questionResultText.gameObject.SetActive(false);
    }
}
