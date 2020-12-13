using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    #region Singleton
    public static UIManager instacne;
    private void Awake()
    {
        instacne = this;
    }
    #endregion

    [SerializeField]
    Text currentGenerationText;
    [SerializeField]
    Text currentGenomeText;
    [SerializeField]
    int currentTime;
    [SerializeField]
    Text timeTxt;
    [SerializeField]
    Text cycleText;


    private void Start()
    {
        timeTxt.text = Time.timeScale + "X Time";
    }

    public void UpdateUI(int curGeneration, int curGenome)
    {
        currentGenerationText.text = "Generation : " + curGeneration;
        currentGenomeText.text = "Genome : " + curGenome;
    }

    public void OnBtn_IncreaseTime()
    {
        currentTime++;
        Time.timeScale = currentTime;
        timeTxt.text = Time.timeScale + "X Time";
    }

    public void OnBtn_DecreaseTime()
    {
        if (currentTime > 1)
            currentTime--;
        else
            currentTime = 1;

        Time.timeScale = currentTime;
        timeTxt.text = Time.timeScale + "X Time";
    }

    public void OnBtn_PauseUnPause()
    {
        if (!Time.timeScale.Equals(0))
            Time.timeScale = 0;
        else
            Time.timeScale = currentTime;

        timeTxt.text = Time.timeScale + "X Time";
    }

    public void UpdateRoundCycle(int cycleIndex)
    {
        cycleText.text = "Cycle : " + cycleIndex + " Rounds";
    }

}
