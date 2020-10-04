using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// public class PhotonConnect : MonoBehaviour
public class PhotonConnect : MonoBehaviourPunCallbacks
{
    RoomOptions roomOptions;
    private byte maxPlayersPerRoom = 4;
    public static PhotonConnect Lobby;
    public Text progressLabel;
    public Text dataLabel;
    private string defaultRoomName = "defaultExerciseRoom";
    private int userIdCount;
    private int roomNumber = 1;
    private const byte COLOR_CHANGE_EVENT = 0;
    private const byte BODY_TRACKING_EVENT = 1;
    private bool connectionAttempted = false;

    public void onClick_test()
    {
        _print(true, "onclick test");
        SendMessage();
    }

    private void Awake()
    {
        progressLabel.text = "";
        _print(true, "Awake");
        roomOptions = new RoomOptions { IsVisible = true, IsOpen = true, MaxPlayers = maxPlayersPerRoom };
        if (Lobby == null)
        {
            Lobby = this;
        }

        if (Lobby != this)
        {
            Destroy(Lobby.gameObject);
            Lobby = this;
        }

        DontDestroyOnLoad(gameObject);
        _print(true, "Awoke");
    }

    void Start()
    {
        _print(true, "Start start");
        PhotonNetwork.ConnectUsingSettings();
        _print(true, "Start started");
    }

    void Update()
    {
        if (!connectionAttempted && SceneManager.GetActiveScene().name == "Launcher")
        {
            _print(true, "SampleScene active");
            connectionAttempted = true;
            PhotonNetwork.ConnectUsingSettings();

        }
        else if (!connectionAttempted)
        {
            _print(true, "!connectionAttempted && SampleScene inactive: " + SceneManager.GetActiveScene().name);
        }

    }
    
    public override void OnConnectedToMaster()
    {
        _print(true, "OnConnectedToMaster begin");
        var randomUserId = UnityEngine.Random.Range(0, 999999);
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.AuthValues = new AuthenticationValues();
        PhotonNetwork.AuthValues.UserId = randomUserId.ToString();
        userIdCount++;
        PhotonNetwork.NickName = PhotonNetwork.AuthValues.UserId;
        _print(true, "OnConnectedToMaster finish, joining random");
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        _print(true, "\nPhotonLobby.OnJoinRandomFailed()");
        JoinOrCreateRoom_defaultRoomName();
    }

    public void JoinOrCreateRoom_defaultRoomName()
    {
        _print(true, "JoinOrCreateRoom_" + defaultRoomName);
        PhotonNetwork.JoinOrCreateRoom(defaultRoomName, roomOptions, TypedLobby.Default);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        _print(true, "\nPhotonLobby.OnCreateRoomFailed()");
        JoinOrCreateRoom_defaultRoomName();
    }

    public override void OnCreatedRoom()
    {
        _print(true, "\nPhotonLobby.OnCreatedRoom()");
        base.OnCreatedRoom();
        roomNumber++;
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        _print(true, "\nPhotonLobby.OnJoinedRoom(): " + PhotonNetwork.CurrentRoom.Name);

        if (PhotonNetwork.CurrentRoom.Name != defaultRoomName)
        {
            _print(true, "PhotonNetwork.CurrentRoom.Name != defaultRoomName");
            _print(true, PhotonNetwork.CurrentRoom.Name + " != " + defaultRoomName);
            JoinOrCreateRoom_defaultRoomName();
        }

        networkEventsEnable();
        _print(true, "Other/Total players in room: " + PhotonNetwork.CountOfPlayersInRooms + " / " + (PhotonNetwork.CountOfPlayersInRooms + 1));
    }

#region Networking -------------------------------------------------------------------------------
    public override void OnDisconnected(DisconnectCause cause)
    {
        _print(true, $"PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {cause}");
        networkEventsDisable();
    }

    private void networkEventsEnable()
    {
        _print(true, "adding event callback");
        PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventReceived;
    }

    private void networkEventsDisable()
    {
        _print(true, "removing event callback");
        PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClient_EventReceived;
    }
    
    public void SendMessage()
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions() {
            Receivers = ReceiverGroup.All,
        };
        object[] datas = new object[] {"body tracking data from K4A app"};
        PhotonNetwork.RaiseEvent(BODY_TRACKING_EVENT, datas, raiseEventOptions, SendOptions.SendUnreliable);
    }

    private void NetworkingClient_EventReceived(EventData obj)
    {
        if (obj != null)
        {
            object[] datas = (object[])obj.CustomData;
            switch (obj.Code)
            {
                case COLOR_CHANGE_EVENT:
                    _print(true, "received COLOR_CHANGE_EVENT");
                    break;
                case BODY_TRACKING_EVENT:
                    //_print(true, "received BODY_TRACKING_EVENT");
                    DecodeCoordinateString((string)datas[0]);                    
                    break;
                default:
                    _print(true, "default unhandled obj.Code: " + obj.Code);
                    break;
            }
        }
        else
        {
            _print(true, "invalid EventData obj recieved");
            return;
        }
    }
    #endregion

    private void _print(bool shouldPrint, string msg, string data = "")
    {
        if (shouldPrint) Debug.Log(msg);
        if (shouldPrint) progressLabel.text += "\n" + msg;
        if (data.Length > 0) dataLabel.text = data;
    }


    public JointUpdater jI;
    private void DecodeCoordinateString(string input)
    {
        // _print(true, input);

        Dictionary<int, Vector3> JointData = new Dictionary<int, Vector3>();
        string[] JointString = input.Split('*');//split each entry apart
        
        // _print(true, JointString.Length.ToString());

        for(int i = 0; i < JointString.Length; i++)//iterate through each entry
        {
            // _print(true, "Joint Id: " + i);
            // _print(true, JointString[i]);

            string[] singleJoint = JointString[i].Split('(');//seperate joint index from vector3
            // _print(true, "Length of singleJoint: " + singleJoint.Length);

            //int jointIndex = int.Parse(singleJoint[0]);//get index as int
            // _print(true, "singleJoint 0: " +singleJoint[0]);
            // _print(true, "singleJoint 1: " +singleJoint[1]);

            string[] jointPos = singleJoint[1].Split(',');//split vector3 info into 3 parts
            Vector3 jointPosV3 = Vector3.zero;//create empty vector 3

            //assign float values to vector 3
            jointPosV3.x = float.Parse(jointPos[0])/-1000;
            jointPosV3.y = float.Parse(jointPos[1])/1000;
            jointPosV3.z = float.Parse(jointPos[2])/1000;
            // _print(true, jointPosV3.ToString());
            //add index to diction to apply to skeleton later
            JointData.Add(i, jointPosV3);            
        }

        jI.UpdateJointPos(JointData);
    }
}
