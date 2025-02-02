using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetMan : NetworkManager
{
    public NetworkIdentity TeamCreator;
    public NetworkIdentity TeamCenter;

    public struct PosMessage : NetworkMessage //наследуемся от интерфейса NetworkMessage, чтобы система поняла какие данные упаковывать
    {
        public Vector2 vector2; //нельзя использовать Property
    }
    public void OnCreateCharacter(NetworkConnection conn, PosMessage message)
    {
        GameObject go = Instantiate(playerPrefab, message.vector2, Quaternion.identity); //локально на сервере создаем gameObject
        NetworkServer.AddPlayerForConnection(conn, go); //присоеднияем gameObject к пулу сетевых объектов и отправляем информацию об этом остальным игрокам
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        playerSpawned = false;
        NetworkServer.RegisterHandler<PosMessage>(OnCreateCharacter); //указываем, какой struct должен прийти на сервер, чтобы выполнился свапн
    }
    public bool playerSpawned;

    public void ActivatePlayerSpawn()
    {
        PosMessage m = new PosMessage() { vector2 = new Vector2(1000, 1000) }; //создаем struct определенного типа, чтобы сервер понял к чему эти данные относятся
        connection.Send(m); //отправка сообщения на сервер с координатами спавна
        playerSpawned = true;
    }
    NetworkConnection connection;
    public bool playerConnected;

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        playerSpawned = false;
        connection = conn;
        playerConnected = true;
        TeamCreator.AssignClientAuthority(conn);
        TeamCenter.AssignClientAuthority(conn);
    }
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        playerConnected = false;
    }

    private void Update()
    {
        if (!playerSpawned && playerConnected)
        {
            ActivatePlayerSpawn();
        }
    }
}
