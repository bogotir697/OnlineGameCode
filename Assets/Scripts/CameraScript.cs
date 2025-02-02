using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class CameraScript : MonoBehaviour
{
    public NetworkBehaviour player;
    public NetMan netMan;
    Vector3 center;
    public float maxCamRadius = 2f;
    Vector3 mousePos;
    public float maxMouseDistance = 8f;
    public Texture2D cursor;
    public Canvas nicknameCanvas;
    public string playerNickname = "";
    private void Start()
    {
        netMan = NetMan.FindObjectOfType<NetMan>();
    }
    void Update()
    {
        Cursor.SetCursor(cursor, Vector2.zero, CursorMode.Auto);
        if (netMan.playerSpawned) 
        {
            var players = Resources.FindObjectsOfTypeAll<Player>();
            if (players.Length != 0)
            {
                foreach (var obj in players)
                {
                    if (obj.GetComponent<Player>().hasAuthority)
                    {
                        player = obj;
                        break;
                    }
                }
            }
        }
    }
    void LateUpdate()
    {
        if (netMan.playerConnected)
            nicknameCanvas.gameObject.SetActive(false);
        else
            player = null;
        if (player != null && player.GetComponent<Player>().playerSpawned)
        {
            if (playerNickname == "")
                playerNickname = $"Player{(int)player.GetComponent<Player>().netId}";
            if (player.isServer)
                player.GetComponent<Player>().ChangeNickname(playerNickname);
            else
                player.GetComponent<Player>().CmdChangeNickname(playerNickname);
            if (player.hasAuthority)
            {
                center = player.transform.position;
                mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = center.z;
                Vector3 cameraPos;
                if (Vector3.Distance(mousePos, center) / maxMouseDistance < 1)
                    cameraPos = Vector3.Distance(mousePos, center) * maxCamRadius / maxMouseDistance * (mousePos - center).normalized + center;
                else
                    cameraPos = maxCamRadius * (mousePos - center).normalized + center;
                transform.position = new Vector3(cameraPos.x, cameraPos.y, transform.position.z);

                if (!player.GetComponent<Player>().rifle.GetComponent<Rifle>().isAutomaicMode && player.GetComponent<Player>().rifleIsActive)
                {
                    gameObject.GetComponent<Camera>().orthographicSize = player.GetComponent<Player>().rifle.GetComponent<Rifle>().sniperModeCameraSize;
                }
                else
                {
                    gameObject.GetComponent<Camera>().orthographicSize = player.GetComponent<Player>().rifle.GetComponent<Rifle>().autoModeCameraSize;
                }
            }
        }
        else
        {
            nicknameCanvas.gameObject.SetActive(true);
            playerNickname = nicknameCanvas.transform.GetChild(0).GetComponent<InputField>().text;
        }
    }
}
