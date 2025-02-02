using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class Team : MonoBehaviour
{
    [SyncVar(hook = nameof(SyncTeam))]
    GameTeam _SyncTeam;
    GameTeam team;
    public GameObject parent;
    public Text teamText;
    public void TeamData(GameTeam teamData)
    {
        _SyncTeam = teamData;
    }
    //private void Update()
    //{
    //    teamText.text = $"{team.name}\n{team.playerCount}/{team.maxPlayerCount}";
    //    gameObject.GetComponent<Image>().color = team.color;
    //    gameObject.transform.SetParent(parent.transform, false);
    //}
    void SyncTeam(GameTeam oldValue, GameTeam newValue)
    {
        team = newValue;
        teamText.text = $"{team.name}\n{team.playerCount}/{team.maxPlayerCount}";
        gameObject.GetComponent<Image>().color = team.color;
        gameObject.transform.SetParent(parent.transform, false);
    }
}
