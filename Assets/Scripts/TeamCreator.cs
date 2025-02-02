using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public struct GameTeam
{
    public int id;
    public string name;
    public Color color;
    public int maxPlayerCount;
    public int playerCount;
    public uint[] players;
    public GameTeam(int teamId, string teamName, Color teamColor, int teamMaxPlayerCount, uint player)
    {
        id = teamId;
        name = teamName;
        color = teamColor;
        maxPlayerCount = teamMaxPlayerCount;
        players = new uint[teamMaxPlayerCount];
        for (playerCount = 0; playerCount < teamMaxPlayerCount; playerCount++)
        {
            if (players[playerCount] == uint.MinValue)
            {
                players[playerCount] = player;
                break;
            }
        }
        playerCount++;
    }
}
public class TeamCreator : NetworkBehaviour
{
    public NetMan netMan;
    public readonly SyncList<GameTeam> teams = new SyncList<GameTeam>();
    public SyncList<GameObject> teamButtons = new SyncList<GameObject>();
    Player player = null;
    [Header("Color Picker")]
    public Image image;
    public Slider hSlider;
    public Slider sSlider;
    public Slider vSlider;
    public int startingHValue;
    public int startingSValue;
    public int startingVValue;
    float hPValue = 0;
    float sPValue = 0;
    float vPValue = 0;
    [Header("Team Creating")]
    public GameObject newTeamButton;
    public GameObject teamChoosingCanvas;
    public GameObject teamCreatingCanvas;
    public GameObject teamButtonPrefab;
    public InputField teamNameIF;
    public Slider countOfPlayersSlider;
    public int startingCountOfPlayers;
    public Button CreateTeamButton;
    Color teamColor;
    string teamName;
    int maxPlayerCount;
    int playerCount;
    int teamId;
    float previousCountOfPlayers;
    [SyncVar(hook = nameof(SyncNewTeamButtonTransform))]
    Transform _SyncNewTeamButtonTransform;
    Transform NewTeamButtonTransform;

    void SyncNewTeamButtonTransform(Transform oldValue, Transform newValue)
    {
        NewTeamButtonTransform = newValue;
    }
    private void Start()
    {
        hSlider.value = (float)startingHValue;
        sSlider.value = (float)startingSValue;
        vSlider.value = (float)startingVValue;
        countOfPlayersSlider.value = (float)startingCountOfPlayers;
        netMan = Resources.FindObjectsOfTypeAll<NetMan>()[0];
        if (isServer)
            teamButtons.Add(newTeamButton.gameObject);
        teamButtonPrefab.GetComponent<Team>().parent = gameObject.transform.GetChild(0).GetChild(0).gameObject;
    }
    private void Update()
    {
        if (netMan == null)
            netMan = Resources.FindObjectsOfTypeAll<NetMan>()[0];
        if (player == null && netMan.playerSpawned)
        {
            var players = Resources.FindObjectsOfTypeAll<Player>();
            if (players.Length != 0) 
            { 
                foreach (var obj in players)
                {
                    if (obj.hasAuthority)
                    {
                        player = obj;
                        break;
                    }
                }
            }
        }
        if (netMan.playerConnected && !player.playerSpawned)
        {
            if (teamCreatingCanvas.activeSelf)
            {
                {
                    float hValue = hSlider.value / 255;
                    float sValue = sSlider.value / 255;
                    float vValue = vSlider.value / 255;
                    teamColor = Color.HSVToRGB(hValue, sValue, vValue);

                    Color32[] hColors = new Color32[256];
                    for (int i = 0; i < 255; i++)
                    {
                        hColors[i] = Color.HSVToRGB(i * 1f / 255, 1, 1);
                    }
                    Texture2D hTexture = new Texture2D(256, 1);
                    hTexture.SetPixels32(hColors);
                    hTexture.Apply();
                    hSlider.transform.GetChild(0).GetComponent<RawImage>().texture = hTexture;

                    Color32[] sColors = new Color32[256];
                    for (int i = 0; i < 255; i++)
                    {
                        sColors[i] = Color.HSVToRGB(hValue, i * 1f / 255, 1);
                    }
                    Texture2D sTexture = new Texture2D(256, 1);
                    sTexture.SetPixels32(sColors);
                    sTexture.Apply();
                    sSlider.transform.GetChild(0).GetComponent<RawImage>().texture = sTexture;

                    Color32[] vColors = new Color32[256];
                    for (int i = 0; i < 255; i++)
                    {
                        vColors[i] = Color.HSVToRGB(hValue, sValue, i * 1f / 255);
                    }
                    Texture2D vTexture = new Texture2D(256, 1);
                    vTexture.SetPixels32(vColors);
                    vTexture.Apply();
                    vSlider.transform.GetChild(0).GetComponent<RawImage>().texture = vTexture;

                    if (hSlider.value != hPValue)
                        hSlider.transform.GetChild(1).GetComponent<InputField>().text = hSlider.value.ToString();
                    if (sSlider.value != sPValue)
                        sSlider.transform.GetChild(1).GetComponent<InputField>().text = sSlider.value.ToString();
                    if (vSlider.value != vPValue)
                        vSlider.transform.GetChild(1).GetComponent<InputField>().text = vSlider.value.ToString();
                    hSlider.value = float.Parse(hSlider.transform.GetChild(1).GetComponent<InputField>().text);
                    sSlider.value = float.Parse(sSlider.transform.GetChild(1).GetComponent<InputField>().text);
                    vSlider.value = float.Parse(vSlider.transform.GetChild(1).GetComponent<InputField>().text);
                    hPValue = float.Parse(hSlider.transform.GetChild(1).GetComponent<InputField>().text);
                    sPValue = float.Parse(sSlider.transform.GetChild(1).GetComponent<InputField>().text);
                    vPValue = float.Parse(vSlider.transform.GetChild(1).GetComponent<InputField>().text);
                } //Color Picker
                {
                    
                    if (countOfPlayersSlider.value != previousCountOfPlayers)
                        countOfPlayersSlider.transform.GetComponentInChildren<InputField>().text = countOfPlayersSlider.value.ToString();
                    
                    countOfPlayersSlider.value = float.Parse(countOfPlayersSlider.transform.GetComponentInChildren<InputField>().text);
                    previousCountOfPlayers = int.Parse(countOfPlayersSlider.transform.GetComponentInChildren<InputField>().text);
                }
                {
                    bool nameExist = false;
                    bool colorExist = false;
                    if (teams.Count != 0)
                    {
                        foreach(var team in teams)
                        {
                            if (teamNameIF.text == team.name)
                            {
                                nameExist = true;
                                break;
                            }
                            if (teamColor == team.color)
                            {
                                colorExist = true;
                                break;
                            }
                        }
                    }
                    CreateTeamButton.interactable = !(teamNameIF.text == "") && !nameExist && !colorExist;
                    image.color = teamColor;
                    maxPlayerCount = int.Parse(countOfPlayersSlider.transform.GetComponentInChildren<InputField>().text);
                    teamName = teamNameIF.text;
                }
            }
        }
        else if (player != null && player.playerSpawned)
        {
            teamChoosingCanvas.SetActive(false);
            teamCreatingCanvas.SetActive(false);
        }
    }
    [Server]
    void AddTeamToList(GameTeam team)
    {
        teams.Add(team);
    }
    [Command]
    void CmdAddTeamToList(GameTeam team)
    {
        AddTeamToList(team);
    }

    public void CreateTeam()
    {
        teamId = teams.Count;
        GameTeam team = new GameTeam(teamId, teamName, teamColor, maxPlayerCount, player.netId);
        if (isServer)
            AddTeamToList(team);
        else
            CmdAddTeamToList(team);
        //if (isServer)
        //    SpawnNewTeam(team);
        //else
        //    CmdSpawnNewTeam(team);
        Menu();
        //if (player.isServer)
        //    player.SpawnPlayer(team);
        //else
        //    player.CmdSpawnPlayer(team);
    }
    void SpawnNewTeam(GameTeam team)
    {
        GameObject newTeam = Instantiate(teamButtonPrefab, gameObject.transform.GetChild(0).GetChild(0));
        newTeam.GetComponent<Team>().TeamData(team);
        teamButtons.RemoveAt(teamButtons.Count - 1);
        teamButtons.Add(newTeam);
        if (teamButtons.Count < 28)
        {
            teamButtons.Add(newTeamButton);
            newTeamButton.SetActive(true);
        }
        else
            newTeamButton.SetActive(false);
        TeamButtonsPos();
    }
    public void Menu()
    {
        teamChoosingCanvas.SetActive(!teamChoosingCanvas.activeSelf);
        teamCreatingCanvas.SetActive(!teamCreatingCanvas.activeSelf);
        if (!teamCreatingCanvas.activeSelf)
        {
            hSlider.value = (float)startingHValue;
            sSlider.value = (float)startingSValue;
            vSlider.value = (float)startingVValue;
            countOfPlayersSlider.value = (float)startingCountOfPlayers;
            teamNameIF.text = "";
        }
    }
    void TeamButtonsPos()
    {
        int count = teamButtons.Count;
        int sCount = (int)System.Math.Ceiling(count * 1f / 7);
        List<float> xCord = new List<float>();
        List<float> yCord = new List<float>();

        for (int i = sCount - 1; i >= 0; i--)
        {
            int t;
            if (i == 0 && count % 7 != 0)
                t = count - count / 7 * 7;
            else
                t = 7;
            if (sCount % 2 != 0)
            {
                for (int j = (sCount / 2); j > -(sCount / 2 + 1); j--)
                {
                    yCord.Add(j);
                }
            }
            else
            {
                for (float j = (sCount / 2 - 0.5f); j > -(sCount / 2); j--)
                {
                    yCord.Add(j);
                }
            }
            if (t % 2 != 0)
            {
                for (int k = -(t / 2); k < (t / 2 + 1); k++)
                {
                    xCord.Add(k);
                }
            }
            else
            {
                for (float k = -(t / 2 - 0.5f); k < (t / 2); k += 1)
                {
                    xCord.Add(k);
                }
            }
        }

        int m = 0;
        for (int l = 0; l < count; l++)
        {
            if (l % 7 == 0 && l != 0)
                m++;
            teamButtons[l].transform.localPosition = new Vector3(xCord[l] * 100, yCord[m] * 100, 0);
        }
    }

    public override void OnStartClient()
    {
        teams.Callback += OnTeamsUpdated;

        // Process initial SyncList payload
        for (int index = 0; index < teams.Count; index++)
            OnTeamsUpdated(SyncList<GameTeam>.Operation.OP_ADD, index, new GameTeam(), teams[index]);
    }
    void OnTeamsUpdated(SyncList<GameTeam>.Operation op, int index, GameTeam oldTeam, GameTeam newTeam)
    {
        switch (op)
        {
            case SyncList<GameTeam>.Operation.OP_ADD:
                SpawnNewTeam(newTeam);
                break;
            case SyncList<GameTeam>.Operation.OP_INSERT:
                // index is where it was inserted into the list
                // newItem is the new item
                break;
            case SyncList<GameTeam>.Operation.OP_REMOVEAT:
                // index is where it was removed from the list
                // oldItem is the item that was removed
                break;
            case SyncList<GameTeam>.Operation.OP_SET:
                // index is of the item that was changed
                // oldItem is the previous value for the item at the index
                // newItem is the new value for the item at the index
                break;
            case SyncList<GameTeam>.Operation.OP_CLEAR:
                // list got cleared
                break;
        }
    }
}

