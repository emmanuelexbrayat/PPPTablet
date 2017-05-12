﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class QuizzGame :
    Game,
    ITextureReceiver,
    IMediaListener
{
    public float tempsReponse;
    public float tempsExplication;
    public List<TextQuestion> questions;

    protected float timeAtQuestionLaunch;

    protected Canvas canvas;

    protected TextQuestion currentQuestion;
    protected RawImage questionImage;
    protected RectTransform questionImagePanel;

    protected GameObject explicationsPanel;
    protected Text explicationsText;

    protected Text questionText;
    protected Text countDownText;
    protected QuizzAnswer[] answers;

    protected QuizzAnswer currentAnswer;

    protected Vector2 answerPos;
    protected Vector2 initExplicationsPos;

    override public void Awake()
    {
        base.Awake();

        canvas = transform.FindChild("Canvas").GetComponent<Canvas>();

        questions = new List<TextQuestion>();
        questionText = transform.Find("Canvas/QuestionPanel/Question").GetComponent<Text>();
        questionImagePanel = transform.Find("Canvas/QuestionImagePanel").GetComponent<RectTransform>();
        questionImage = transform.Find("Canvas/QuestionImagePanel/QuestionImage").GetComponent<RawImage>();
        countDownText = transform.Find("Canvas/CountdownPanel/Countdown").GetComponent<Text>();
        explicationsPanel = transform.Find("Canvas/ExplicationsPanel").gameObject;
        explicationsText = transform.Find("Canvas/ExplicationsPanel/Explications").GetComponent<Text>();

        explicationsPanel.SetActive(false);

        answers = GetComponentsInChildren<QuizzAnswer>();
        int index = 1;
        foreach (QuizzAnswer a in answers)
        {
            a.answerSelectedHandler += answerSelected;
            a.answerID = index++;
        }

        answerPos = answers[0].GetComponent<RectTransform>().anchoredPosition;
        initExplicationsPos = explicationsPanel.GetComponent<RectTransform>().anchoredPosition;
    }

    public override void Update()
    {
        base.Update();
        if (Input.GetKeyDown(KeyCode.N)) nextQuestion();

        if (timeAtQuestionLaunch > 0)
        {
            float timeLeft = (timeAtQuestionLaunch + tempsReponse) - Time.time;
            if (timeLeft > 0)
            {

                int secondsLeft = (int)timeLeft % 60;
                int minutesLeft = Mathf.FloorToInt(timeLeft / 60);
                countDownText.text = string.Format("{0,2}:{1,2}", minutesLeft, secondsLeft);
            }
            else
            {
                showAnswer();
                timeAtQuestionLaunch = 0;
            }
        }
    }


    public override void startGame()
    {
        nextQuestion();
    }

    public void nextQuestion()
    {
        nextQuestion(false);
    }

    public void nextQuestion(bool videoAlreadyPlayed)
    {
        if (currentQuestion == null) setQuestion(0);
        else
        {
            string outroPath = getOutroVideoPath(questions.IndexOf(currentQuestion)+1);
            if (outroPath != "" && !videoAlreadyPlayed)
            {
                canvas.enabled = false;
                MediaPlayer.play(outroPath, false, this, "outro");
                return;
            }

            canvas.enabled = true;

            if (questions.IndexOf(currentQuestion) < questions.Count - 1)
            {
                setQuestion(questions.IndexOf(currentQuestion) + 1);
            }
            else
            {
                endGame();
            }
        }
    }

    public void setQuestion(int index)
    {
        if (index < 0 || index >= questions.Count) return;

        if (currentQuestion != null)
        {

        }

        currentQuestion = questions[index];

        if (currentQuestion != null)
        {
            loadCurrentQuestion();
        }
    }

    public void loadCurrentQuestion(bool videoAlreadyPlayed = false)
    {

        if (currentQuestion == null) return;


        int qNum = questions.IndexOf(currentQuestion) + 1;

        string introPath = getIntroVideoPath(qNum);
        if (introPath != "" && !videoAlreadyPlayed)
        {
            canvas.enabled = false;
            MediaPlayer.play(introPath, false, this, "intro");
            return;
        }

        canvas.enabled = true;

        explicationsPanel.SetActive(false);
        questionText.text = currentQuestion.question;

        for (int i = 0; i < answers.Length; i++)
        {
            if (i >= currentQuestion.reponses.Length)
            {
                answers[i].gameObject.SetActive(false);
                continue;
            }
            answers[i].gameObject.SetActive(true);
            bool isGood = currentQuestion.bonneReponse == i + 1;
            answers[i].setData(id, qNum, currentQuestion.reponses[i], isGood);
        }

        currentAnswer = null;
        timeAtQuestionLaunch = Time.time + .001f;//force if Time.time == 0 here


        questionImage.color = Color.black;
        StartCoroutine(AssetManager.loadGameTexture(id, "question" + qNum + ".jpg", "questionImage", this));

    }

    void showAnswer()
    {
        countDownText.text = "Fini !";

        bool isGood = false;
        foreach (QuizzAnswer a in answers)
        {
            if (a.showAnswer()) isGood = true;
        }
        if (isGood) score++;
        Invoke("showExplications", 1);
    }

    public virtual void showExplications()
    {

        Invoke("nextQuestion", tempsExplication + 1);
    }


    [OSCMethod("selectReponse")]
    public void selectAnswer(int answerIndex)
    {
        answerSelected(answers[answerIndex]);
    }

    public void answerSelected(QuizzAnswer a)
    {
        if (currentAnswer == a) return;

        if (currentAnswer != null)
        {
            currentAnswer.setSelected(false);
        }

        currentAnswer = a;

        if (currentAnswer != null)
        {
            currentAnswer.setSelected(true);
        }
    }

    public void textureReady(string texID, Texture2D tex)
    {
        if (texID == "questionImage")
        {
            float texRatio = (float)tex.width / (float)tex.height;
            Rect qRect = questionImagePanel.rect;
            float tWidth = qRect.width - 16;
            float tHeight = qRect.height - 16;
            float imgRatio = tWidth / tHeight;

            if (texRatio > imgRatio)
            {
                questionImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, tWidth);
                questionImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, tWidth / texRatio);
            }
            else
            {
                questionImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, tHeight * texRatio);
                questionImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, tHeight);
            }
        }
        questionImage.color = Color.white;
        questionImage.texture = tex;
    }

    public string getIntroVideoPath(int index)
    {
        return AssetManager.getGameMediaFile(id, "question" + index + "_intro.mp4");
    }

    public string getOutroVideoPath(int index)
    {
        return AssetManager.getGameMediaFile(id, "question" + index + "_outro.mp4");
    }

    void IMediaListener.mediaFinished(string id)
    {
        if (id == "intro")
        {
            loadCurrentQuestion(true);
        }
        else if (id == "outro")
        {
            nextQuestion(true);
        }
    }
}