using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AnswerData : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] TextMeshProUGUI infoTextObject;
    [SerializeField] Image toggle;

    [Header("Textures")]
    [SerializeField] Sprite uncheckedToggle;
    [SerializeField] Sprite checkedToggle;

    [Header("References")]
    [SerializeField] GameEvents events;

    private RectTransform rect;
    public RectTransform Rect
    {
        get
        {
            if(rect == null) //if recttransform is null, add it
            {
                rect = GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
            }
            return rect;
        }
    }

    private int answerIndex = -1;
    public int AnswerIndex { get { return answerIndex; } }

    private bool Checked = false;

    public void UpdateData(string info, int index) //update the answer data for the current question
    {
        infoTextObject.text = info;
        answerIndex = index;
    }

    public void Reset()
    {
        Checked = false;
        UpdateUI();
    }

    public void SwitchState() //updates the UI for resolution screen state
    {
        Checked = !Checked;
        UpdateUI();

        if (events.UpdateQuestionAnswer != null) //if not null update the question answer
        {
            events.UpdateQuestionAnswer(this);
        }
    }

    void UpdateUI() //shows what answer the user picked with a different sprite/checked or unchecked toggle sprite
    {
        toggle.sprite = (Checked) ? checkedToggle : uncheckedToggle;
    }
}
