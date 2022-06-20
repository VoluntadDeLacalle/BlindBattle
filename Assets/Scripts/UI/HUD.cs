using Fusion;
using LincolnCpp.HUDIndicator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HUD : SingletonMonoBehaviour<HUD>
{
    public TMP_Text team1ScoreText;
    public TMP_Text team2ScoreText;
    public GameObject team1PlayerIndicator;
    public GameObject team2PlayerIndicator;

    public TMP_Text roundNameText;
    public TMP_Text timerText;

    public string mainMenuName;
    public string loadingSceneName;
    public string endRoundSFXName;

    public GameObject pauseScreen;
    public GameObject endScreen;

    public GameObject nextRoundPanel;
    public GameObject finalResultPanel;
    public TMP_Text finalResultText;

    public List<GameObject> HostOnlyItems;
    public List<GameObject> ClientOnlyItems;

    public IndicatorRenderer indicatorRenderer;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (NetworkManager.Instance.networkRunner && NetworkGameState.Instance)
        {
            team1ScoreText.text = $"${NetworkGameState.Instance.team1Score}";
            team2ScoreText.text = $"${NetworkGameState.Instance.team2Score}";
            roundNameText.text = $"Round {NetworkGameState.Instance.currentRound}";

            var remainingTime = NetworkGameState.Instance.gameTimer.RemainingTime(NetworkManager.Instance.networkRunner) ?? 0;
            timerText.text = $"{HelperUtilities.GetTimeDisplay(remainingTime)}";


            var teamNum = NetworkGameState.Instance.GetPlayerTeamNumber(NetworkManager.Instance.networkRunner.LocalPlayer);
            team1PlayerIndicator.SetActive(teamNum == 1);
            team2PlayerIndicator.SetActive(teamNum == 2);

            if (NetworkGameState.Instance.gameTimer.Expired(NetworkManager.Instance.networkRunner))
            {
                if (!endScreen.activeSelf)
                {
                    ShowEndScreen();
                }
            }
            else
            {
                if (endScreen.activeSelf)
                {
                    endScreen.SetActive(false);
                }
            }
        }

        if (Input.GetButtonDown("Pause"))
        {
            TogglePause();
        }
    }

    public void ToggleIndicators(bool show)
    {
        indicatorRenderer.gameObject.SetActive(show);
    }

    public void SetCameraForIndicator(Camera camera)
    {
        indicatorRenderer.camera = camera;
    }

    public void Pause()
    {
        HelperUtilities.UpdateCursorLock(false);
        pauseScreen.SetActive(true);
    }

    public void Unpause()
    {
        HelperUtilities.UpdateCursorLock(true);
        pauseScreen.SetActive(false);
    }

    public void TogglePause()
    {
        if (pauseScreen.activeSelf)
        {
            Unpause();
        }
        else
        {
            Pause();
        }
    }

    public void ShowEndScreen()
    {
        pauseScreen.SetActive(false);

        nextRoundPanel.SetActive(!NetworkGameState.Instance.IsLastRound());
        finalResultPanel.SetActive(NetworkGameState.Instance.IsLastRound());

        var networkRunner = NetworkManager.Instance.networkRunner;
        for (int i = 0; i < HostOnlyItems.Count; i++)
        {
            HostOnlyItems[i].SetActive(networkRunner?.GameMode == GameMode.Host);
        }
        for (int i = 0; i < ClientOnlyItems.Count; i++)
        {
            ClientOnlyItems[i].SetActive(networkRunner?.GameMode == GameMode.Client);
        }

        if (NetworkGameState.Instance.team1Score > NetworkGameState.Instance.team2Score)
        {
            finalResultText.text = "Team 1 wins!!";
        }
        else if (NetworkGameState.Instance.team1Score < NetworkGameState.Instance.team2Score)
        {
            finalResultText.text = "Team 2 wins!!";
        }
        else
        {
            finalResultText.text = "It's a Tie!!";
        }

        HelperUtilities.UpdateCursorLock(false);
        endScreen.SetActive(true);

        NetworkGameState.Instance.RPC_PlayAudio(endRoundSFXName);
    }

    public void NextRound()
    {
        if (!NetworkManager.Instance.IsHost)
        {
            return;
        }

        NetworkGameState.Instance.IncrementRound();
        ReloadNetworkedScene();
    }

    private async void ReloadNetworkedScene()
    {
        if (!NetworkManager.Instance.IsHost)
        {
            return;
        }

        var currentSceneName = SceneManager.GetActiveScene().name;

        NetworkManager.Instance.networkRunner.SetActiveScene(loadingSceneName);
        await Task.Delay(TimeSpan.FromSeconds(3));
        NetworkGameState.Instance.ResetTimer(RoundManager.Instance.roundDuration);
        NetworkManager.Instance.networkRunner.SetActiveScene(currentSceneName);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(mainMenuName);
        NetworkManager.Instance.networkRunner.Shutdown();
    }
}
