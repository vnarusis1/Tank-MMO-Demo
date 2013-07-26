using UnityEngine;
using System.Collections;

public class NetworkInfo : MonoBehaviour
{
	public enum StatusType
	{
		ConnectedToServer = 0,
		FailedToConnectToServer = 1,
		DisconnectedFromServer = 2,
		JoinedLobby = 3,
		LeftLobby = 4,
		MasterClientSwitched = 5,
		CreatedRoom = 6,
		CreatingRoomFailed = 7,
		LeftRoom = 8,
		JoinedRoom = 9,
		JoinRoomFailed = 10,
		PlayerConnected = 11,
		PlayerDisconnected = 12,
		RandomJoinFailed = 13,
		ReceivedRoomList = 14,
		RoomListUpdated = 15,
		RoomFull = 16
	}
	
	#region Members
	public bool debug = false;
	
	/// <summary>
	/// The protocol version.
	/// </summary>
	public string protocolVersion = "0.1";
	
	/// <summary>
	/// Should we automatically connect to the server at runtime?
	/// </summary>
	public bool autoConnect = true;
	
	/// <summary>
	/// Should we check for disconnection and reconnect?
	/// </summary>
	public bool autoReconnect = true;
	
	/// <summary>
	/// If this is not empty, upon connection we will connect to this room.
	/// </summary>
	public string autoJoinRoomName = "dotbunny";
	public int defaultRoomSize = 32;
	public bool defaultRoomVisible = true;
	
	/// <summary>
	/// Should we reconnect to the previous room on reconnect.
	/// </summary>
	public bool autoRejoinRoom = true;	

	/// <summary>
	/// Was the latest disconnection forced? Meaning maybe we shouldn't reconnect
	/// </summary>
	private bool _forcedDisconnection = false;

	/// <summary>
	/// Our last joined room's name, useful for rejoining
	/// </summary>
	private string _joinedRoom = "";

	/// <summary>
	/// Connection Status
	/// </summary>
	private StatusType _status = StatusType.DisconnectedFromServer;
	#endregion
	
	#region Properties
	public string PlayerName
	{
		get { return PhotonNetwork.playerName; }
		set { PhotonNetwork.playerName = value; }
	}
	
	public int Ping { get { return PhotonNetwork.GetPing(); } }
	
	public string RoomName
	{
		get 
		{
			if ( PhotonNetwork.room != null ) 
			{
				return PhotonNetwork.room.name;
			}
			else
			{
				return null;
			}
		}
		set
		{
			// Add some sort of connection check
			if ( PhotonNetwork.connected)
			{
				if ( PhotonNetwork.room != null && PhotonNetwork.room.name != value) 
				{
					PhotonNetwork.LeaveRoom();
					JoinRoom(value);	
				}
			}
			
		}
	}
	
	public StatusType Status 
	{ 
		get { return _status; } 
		private set {
			if ( _status != value )
			{
				_status = value;
				if ( debug ) Debug.Log (_status.ToString());
				if ( OnStatusUpdate != null ) OnStatusUpdate (_status);
			}
		}
	}
	public string SystemStatus { get { return PhotonNetwork.connectionStateDetailed.ToString(); } }
	
	public bool Connected { get { return PhotonNetwork.connected; } }
	
	public int MaxPlayers { get { if ( PhotonNetwork.room != null ) return (int)PhotonNetwork.room.maxPlayers; else return -1; } }
	public int CurrentPlayers { get { if ( PhotonNetwork.room != null ) return PhotonNetwork.room.playerCount; else return -1; } }
	#endregion
	
	// Multithreaded Safe Singleton Pattern
    // URL: http://msdn.microsoft.com/en-us/library/ms998558.aspx
    private static readonly object _syncRoot = new Object();
	private static volatile NetworkInfo _staticInstance;	
   	public static NetworkInfo Instance 
	{
        get {
            if (_staticInstance == null) {				
                lock (_syncRoot) {
                    _staticInstance = FindObjectOfType (typeof(NetworkInfo)) as NetworkInfo;
                    if (_staticInstance == null) {
                      // Debug.LogError("The NetworkInfo instance was unable to be found, if this error persists please contact support.");						
                    }
                }
            }
            return _staticInstance;
        }
    }
	
	#region Unity Methods
	
	public void Awake ()
	{
		// Create a random player name for you, probably want to set this later
		PlayerName = "Player #" + Random.Range(1, 9999);

	}
	
	
	// Use this for initialization
	public void Start () 
	{
		PhotonNetwork.ConnectUsingSettings(protocolVersion);
	}
	
	public void OnDisable()
	{
		// Let our script know that this was manually done and we should ignore reconnecting
		_forcedDisconnection = true;
		
		// Forcibly disconnect from the server
		PhotonNetwork.Disconnect();
	}
	
	#endregion
	
	
	#region Delegates

	public delegate void StatusUpdateEventHandler (StatusType status);
	public event StatusUpdateEventHandler OnStatusUpdate;
	
	#endregion
	
	private void JoinRoom(string name)
	{
		bool found = false;
		bool full = false;
		foreach (Room r in PhotonNetwork.GetRoomList())
		{
			if ( r.name == name ) 
			{
				found = true;
				if ( r.maxPlayers <= r.playerCount ) full = true;
			}
		}
	
		if ( !full )
		{
			if ( found ) PhotonNetwork.JoinRoom(name);
			else PhotonNetwork.CreateRoom(name, defaultRoomVisible, true, (byte)defaultRoomSize);
			_joinedRoom = name;
		}
		else
		{
			if ( OnStatusUpdate != null ) OnStatusUpdate (StatusType.RoomFull);
		}
	}
		
	#region Photon Callbacks (Evil SendMessage?)
	public void OnConnectedToPhoton() 
	{ 
		Status = StatusType.ConnectedToServer; 
	}	
	public void OnFailedToConnectToPhoton()
	{
		Status = StatusType.FailedToConnectToServer;
		
		// Should we auto reconnect / retry?
		if ( autoReconnect && !_forcedDisconnection ) Start ();			
	}	
	public void OnDisconnectedFromPhoton ()
	{
		Status = StatusType.DisconnectedFromServer;
		if ( autoReconnect && !_forcedDisconnection ) Start ();
	}
	
	public void OnJoinedLobby ()
	{
		Status = StatusType.JoinedLobby;
	}
	public void OnLeftLobby ()
	{
		Status = StatusType.LeftLobby;
	}
	public void OnMasterClientSwitched ()
	{
		Status = StatusType.MasterClientSwitched;
	}
	
	public void OnCreatedRoom ()
	{
		Status = StatusType.CreatedRoom;
	}
	public void OnPhotonCreateRoomFailed ()
	{
		Status = StatusType.CreatingRoomFailed;
	}
	public void OnLeftRoom ()
	{
		Status = StatusType.LeftRoom;
	}
	public void OnJoinedRoom ()
	{
		Status = StatusType.JoinedRoom;
	}
	public void OnPhotonJoinRoomFailed ()
	{
		Status = StatusType.JoinRoomFailed; 
		_joinedRoom = "";
	}
	public void OnPhotonRandomJoinFailed ()
	{
		Status = StatusType.RandomJoinFailed;
	}
	
	public void OnPhotonPlayerConnected ()
	{
		Status = StatusType.PlayerConnected;
	}
	public void OnPhotonPlayerDisconnected ()
	{
		Status = StatusType.PlayerDisconnected;
	}
	
	public void OnReceivedRoomList ()
	{
		Status =  StatusType.ReceivedRoomList;
		if ( debug )
		{
			foreach(Room r in PhotonNetwork.GetRoomList())
			{
				Debug.Log (r.name + " [" + r.playerCount.ToString() + "/" + r.maxPlayers.ToString() + "]");
			}
		}
		if ( autoRejoinRoom && _joinedRoom != "" ) JoinRoom(_joinedRoom);
		if ( autoJoinRoomName != null && autoJoinRoomName != "" ) JoinRoom(autoJoinRoomName);
	}
	public void OnReceivedRoomListUpdate ()
	{
		Status = StatusType.RoomListUpdated;
	}
	#endregion
}
