using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class TeamSpawnCenter : NetworkBehaviour
{
    TeamCreator teamCreator;
    public SyncList<GameTeam> teams = new SyncList<GameTeam>();
    public List<GameObject> teamSpawns = new List<GameObject>();
    public GameObject teamSpawnPrefab;
    private void Start()
    {
        teamCreator = TeamCreator.FindObjectOfType<TeamCreator>();
    }
    private void Update()
    {
        teams = teamCreator.teams;
        if (teams.Count != teamSpawns.Count)
        {
            for (int i = 0; i < teams.Count - teamSpawns.Count; i++)
            {
                teamSpawns.Add(null);
            }
        }
        if (isServer)
            Teams();
        else
            CmdTeams();
    }
    [Server]
    void Teams()
    {
        for (int i = 0; i < teams.Count; i++)
        {
            if (teamSpawns[i] == null)
            {
                var teamSpawn = Instantiate(teamSpawnPrefab, gameObject.transform);
                NetworkServer.Spawn(teamSpawn);
                teamSpawns[i] = teamSpawn;
            }
        }
        for (int i = 1; i <= System.Math.Min(teamSpawns.Count, teams.Count); i++)
        {
            teamSpawns[i - 1].transform.localEulerAngles = new Vector3(0, 0, i * 360 * 1f / teamSpawns.Count);
            teamSpawns[i - 1].transform.GetChild(0).GetChild(0).rotation = Quaternion.Euler(0, 0, 0);
            teamSpawns[i - 1].transform.GetComponentInChildren<Image>().color = teams[i - 1].color;
        }
    }
    [Command]
    void CmdTeams()
    {
        Teams();
    }
}
