using LootLocker.Requests;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Events;

// Lootlocker source: https://docs.lootlocker.com/players/authentication/guest-login
// Script execution order modified.

namespace Utility
{
    public class LootLockerTool : Singleton<LootLockerTool>
    {
        [Title("Settings")]
        [SerializeField] private string _leaderboardKey = "blobs_leaderboard";
        [SerializeField] private int _downloadCount = 11;
        [SerializeField, ReadOnly] private string _memberID;
        [SerializeField] private string _playerName;
        [Space]
        [Header("Data")]
        [SerializeField] private LootLockerLeaderboardMember[] _members;
        [SerializeField] private bool _showDebug;
        
        [Header("Events")]
        public UnityEvent<string> OnPlayerNameSet;
        public UnityEvent OnSessionSetupCompleted;
        public UnityEvent OnTopScoresDownloadComplete;
        public UnityEvent OnNearbyScoresDownloadComplete;

        // Public properties
        public LootLockerLeaderboardMember[] Members => _members;

        public string PlayerName => _playerName;

        #region Lifecycle
        private void Start()
        {
            SetupLootLockerGuestSession();
        }

        private void OnEnable()
        {
            // Grab new scores when page appears. 
            GetTopScores();
        }
        #endregion

        #region Setup
        private void SetupLootLockerGuestSession()
        {
            LootLockerSDKManager.StartGuestSession(response =>
            {
                if (!response.success)
                {
                    Debug.LogWarning("Error starting Lootlocker session");
                    return;
                }

                GuestSessionDidStart();

                if (_showDebug) Debug.Log("Successfully started LootLocker session");
            });
        }

        private void GuestSessionDidStart()
        {
            GetPlayerName();
            GetTopScores();

            OnSessionSetupCompleted?.Invoke();
        }
        #endregion

        public void SubmitScore(int score)
        {
            string memberID = _memberID.IsNullOrWhitespace() ? "Test" : _memberID;

            LootLockerSDKManager.SubmitScore(memberID, score, _leaderboardKey, response =>
            {
                if (response.statusCode == 200)
                {
                    if (_showDebug) Debug.Log($"Submit Score Successful – memberID: {_memberID} | score: {score} | leaderboardKey: {_leaderboardKey}");
                }
                else
                {
                    Debug.LogWarning("Submit Score Failed: " + response.Error);
                }
            });
        }

        private void GetTopScores()
        {
            LootLockerSDKManager.GetScoreList(_leaderboardKey, _downloadCount, 0, response =>
            {
                if (response.statusCode == 200) {
                    if (_showDebug)
                    {
                        Debug.Log($"Leaderboard Get Top Scores Successful – {response.items.Length} leaderboard member(s) downloaded.");
                        foreach (LootLockerLeaderboardMember member in response.items)
                            Debug.Log(member.ToString());
                    }

                    _members = response.items;

                    OnTopScoresDownloadComplete?.Invoke();
                } else {
                    Debug.LogWarning("Leaderboard Get Top Scores Failed: " + response.Error);
                }
            });
        }

        private void GetScoresAroundMember()
        {
            LootLockerSDKManager.GetMemberRank(_leaderboardKey, _memberID, response =>
            {
                if (response.statusCode == 200)
                {
                    int rank = response.rank;
                    int count = _downloadCount;
                    int after = rank < 6 ? 0 : rank - 5;

                    LootLockerSDKManager.GetScoreList(_leaderboardKey, count, after, scoreResponse =>
                    {
                        if (scoreResponse.statusCode == 200)
                        {
                            if (_showDebug)
                            {
                                Debug.Log($"Leaderboard Get Scores Around Member Successful – {scoreResponse.items.Length} leaderboard member(s) downloaded.");
                                foreach (LootLockerLeaderboardMember member in scoreResponse.items)
                                    Debug.Log(member.ToString());
                            }

                            _members = scoreResponse.items;

                            OnNearbyScoresDownloadComplete?.Invoke();
                        }

                        Debug.LogWarning("Leaderboard Get Scores Around Member Failed: " + scoreResponse.Error);
                    });
                }
                else
                {
                    Debug.LogWarning("Leaderboard Get Member Rank Failed: " + response.Error);
                }
            });
        }

        public void GetPlayerName()
        {
            LootLockerSDKManager.GetPlayerName(response =>
            {
                if (response.success)
                {
                    Debug.Log("Successfully retrieved player name: " + response.name);

                    _playerName = response.name;

                    OnPlayerNameSet?.Invoke(_playerName);
                }
                else
                {
                    Debug.Log("Error getting player name");
                }
            });
        }

        public void UpdatePlayerName(string playerName)
        {
            LootLockerSDKManager.SetPlayerName(playerName, (response) =>
            {
                if (response.success)
                {
                    Debug.Log($"Successfully set player name:{_playerName}({_memberID})");

                    _playerName = playerName;

                    OnPlayerNameSet?.Invoke(playerName);
                }
                else
                {
                    Debug.Log("Error setting player name");
                }
            });
        }
    }
}