using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuToGameTransition : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "Game";

    [Header("UI / Fade")]
    [SerializeField] private ScreenFader fader;
    [SerializeField] private float fadeOutDuration = 0.35f;
    [SerializeField] private float fadeInDuration = 0.25f;

    [Header("Input Fields (TMP)")]
    [SerializeField] private TMP_InputField dayInputField;
    [SerializeField] private TMP_InputField monthInputField;
    [SerializeField] private TMP_InputField hourInputField;


    private int m_day = 1;
    private int m_month = 1;
    private int m_hour = 1;

    void OnEnable()
    {
        if (dayInputField) dayInputField.onValueChanged.AddListener(OnDayValueChanged);
        if (monthInputField) monthInputField.onValueChanged.AddListener(OnMonthValueChanged);
        if (hourInputField) hourInputField.onValueChanged.AddListener(OnHourValueChanged);
    }

    void OnDisable()
    {
        if (dayInputField) dayInputField.onValueChanged.RemoveListener(OnDayValueChanged);
        if (monthInputField) monthInputField.onValueChanged.RemoveListener(OnMonthValueChanged);
        if (hourInputField) hourInputField.onValueChanged.RemoveListener(OnHourValueChanged);
    }

    IEnumerator Start()
    {
        SetTMP(monthInputField, "1");
        SetTMP(dayInputField, "1");
        SetTMP(hourInputField, "1");
        yield break;
    }

    public void OnClickPlay()
    {
        if (isActiveAndEnabled) StartCoroutine(PlayFlow());
    }

    private IEnumerator PlayFlow()
    {

        if (fader) yield return fader.FadeOut(fadeOutDuration);

        var data = ClimateData.Instance;
        int engineHour = (m_hour == 24) ? 0 : m_hour; // 24 becomes 0 (midnight)
        data.SetMonth((Month)m_month);
        data.SetDayInMonth(m_day);
        data.SetHourOfDay(engineHour);

        var op = SceneManager.LoadSceneAsync(gameSceneName, LoadSceneMode.Single);
        while (!op.isDone) yield return null;
        yield return null; // let one frame settle

        if (fader) yield return fader.FadeIn(fadeInDuration);
    }

  
    public void OnMonthValueChanged(string value)
    {
        if (!int.TryParse(value, out int month)) { SetTMP(monthInputField, "1"); month = 1; }
        month = Mathf.Clamp(month, 1, 12);
        if (m_month != month) { m_month = month; SetTMP(monthInputField, month.ToString()); }

        int year = 1;
        int maxDays = DateTime.DaysInMonth(year, m_month);
        if (m_day > maxDays)
        {
            m_day = maxDays;
            SetTMP(dayInputField, m_day.ToString());
        }
    }

    public void OnDayValueChanged(string value)
    {
        if (!int.TryParse(value, out int day)) { SetTMP(dayInputField, "1"); day = 1; }

        int year = 1;
        int safeMonth = Mathf.Clamp(m_month, 1, 12);
        int maxDays = DateTime.DaysInMonth(year, safeMonth);

        day = Mathf.Clamp(day, 1, maxDays);
        if (m_day != day) { m_day = day; SetTMP(dayInputField, day.ToString()); }
    }

    public void OnHourValueChanged(string value)
    {
        if (!int.TryParse(value, out int hour)) { SetTMP(hourInputField, "1"); hour = 1; }
        hour = Mathf.Clamp(hour, 1, 24); // UI shows 1..24
        if (m_hour != hour) { m_hour = hour; SetTMP(hourInputField, hour.ToString()); }
    }

 
    private static void SetTMP(TMP_InputField field, string text)
    {
        if (!field) return;
        if (field.text != text)
        {
            field.SetTextWithoutNotify(text);
            field.caretPosition = text.Length;
            field.ForceLabelUpdate();
        }
    }
}
