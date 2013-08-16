using UnityEngine;
using System.Collections;

public class DemoManager : Photon.MonoBehaviour 
{
	public Transform tankPrefab;
	public Transform[] spawnPoints;
	public Transform targetPoint;
	
	public bool invertMouseLook = true;
	
	public UILabel statusLabel;
	public UILabel healthLabel;
	public UISprite crosshair;
	public CameraManager cameraManager;
	
	
	[System.NonSerialized]
	public GameObject playerTank;
	public Tank playerScript;
	
	
	
	public float pingTimer = 5f;
	private float _nextPing = 0f;
	private int _ping = 0;
	
	
	private Vector3 currentMousePosition;
	private Ray targetRay;
	private RaycastHit targetRayHit;
	public LayerMask targetableLayers;
	public Material willHit;
	public Material wontHit;

	public void Start () 
	{
		_nextPing = pingTimer;
#if !UNITY_EDITOR
		Screen.showCursor = false;
		Screen.lockCursor = true;
#endif
		crosshair.enabled = false;	
	}
	
	public void Update()
	{
		
		#region Text Updates
		_nextPing -= Time.deltaTime;
		if ( _nextPing <= 0 )
		{
			_ping = NetworkInfo.Instance.Ping;
			_nextPing = pingTimer;
			
			statusLabel.text = _ping.ToString() + " ms" + "\n\r" + 
			NetworkInfo.Instance.CurrentPlayers.ToString() + "/" + NetworkInfo.Instance.MaxPlayers.ToString() + " Players";
		}
		
		
		if ( healthLabel.gameObject.activeSelf && playerScript != null) 
		{
			string newText = "";
			for ( int x = 0; x < playerScript.damageRegions.Length; x++ )
			{
				newText += playerScript.damageRegions[x].statusName + ": " + 
					playerScript.damageRegions[x].health.ToString() + "%";
				if ( x < playerScript.damageRegions.Length - 1 )
				{
					newText += "\n\r"; 
				}
			}
	
			if ( healthLabel.text != newText ) 
			{
				healthLabel.text = newText;
			}
		}
		#endregion
		
		if ( playerScript != null ) 
		{
			if ( cameraManager.mode == CameraManager.Mode.LookingDownSights )
			{
				crosshair.enabled = true;
				targetPoint.gameObject.SetActive(false);
			}
			else
			{
				
				crosshair.enabled = false;
				targetPoint.gameObject.SetActive(true);
				
				// Targeting Update
				if ( playerScript != null )
				{
		
					targetRay = cameraManager.TargetCamera.ScreenPointToRay(Input.mousePosition);
		
					if (Physics.Raycast(targetRay, out targetRayHit, 
						float.PositiveInfinity,
						//playerScript.primaryFire.maximumTargetDistance + playerScript.cameraController.targetDistance, 
						targetableLayers))
					{
						targetPoint.position = targetRayHit.point;
						
					}
					
					playerScript.TargetedWorldPosition = targetPoint.position;
				}
			}
		}
		
		
		// Demonstration of adjusting aiming color based on aim
		if ( !crosshair.enabled )
		{
			if ( playerScript != null )
			{
				
				
				if ( playerScript.towerController.OnTarget && playerScript.fixedGunsControllers[0].OnTarget )
				{
					targetPoint.renderer.material = willHit;
				}
				else
				{
					targetPoint.renderer.material = wontHit;
				}
					
			}
			
		}
		
		#region User Input
		
		// Show Server Status
		if ( Input.GetKeyDown(KeyCode.Tab) ) 
		{
			healthLabel.gameObject.SetActive(true);
			statusLabel.gameObject.SetActive(true);
		}
		
		// Hide Server Status
		if ( Input.GetKeyUp(KeyCode.Tab ) ) 
		{
			healthLabel.gameObject.SetActive(false);
			statusLabel.gameObject.SetActive(false);
		}
		
		// Fullscreen Check
		if ( Input.GetKeyDown (KeyCode.F11) )
		{
			if ( Screen.fullScreen ) 
			{
				Screen.fullScreen = false;
				
			} else {
				Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
			}
		}
		
		// Lock that cursor!
		if ( Input.GetKeyDown(KeyCode.L) )
		{
			if ( Screen.lockCursor )
			{
				Screen.showCursor = true;
				Screen.lockCursor = false;
			}
			else
			{
				Screen.showCursor = false;
				Screen.lockCursor = true;
			}
		}
		
		if ( Input.GetMouseButtonDown(1) )
		{
			if ( playerScript != null )
			{
				if ( cameraManager.mode == CameraManager.Mode.LookingDownSights )
				{
					cameraManager.mode = CameraManager.Mode.MouseOrbitTarget;
					cameraManager.TargetCamera.transform.parent = null;
				}
				else
				{
					cameraManager.mode = CameraManager.Mode.LookingDownSights;	
					cameraManager.TargetCamera.transform.position = playerScript.manualView.transform.position;
					cameraManager.TargetCamera.transform.rotation = playerScript.manualView.transform.rotation;
					cameraManager.TargetCamera.transform.parent = playerScript.manualView;
				}
			}
		}
		
		
		// Respawn
		if ( Input.GetKeyDown(KeyCode.R) )
		{
			Respawn();
		}
		
		#endregion
				
	}
	public void LateUpdate()	
	{
		cameraManager.UpdateCamera(Input.GetAxis("Mouse X"),Input.GetAxis("Mouse Y"));
		if ( cameraManager.mode == CameraManager.Mode.LookingDownSights )
		{
			
			playerScript.towerController.UpdateTargetRotationByDegrees(Input.GetAxis("Mouse X") * 0.75f);
			
			foreach(TankFixedGunController g in playerScript.fixedGunsControllers)
			{
				if ( invertMouseLook ) 
				{
					g.UpdateTargetRotationByDegrees(Input.GetAxis("Mouse Y") * 0.75f);
				}
				else
				{
					g.UpdateTargetRotationByDegrees(Input.GetAxis("Mouse Y") * -0.75f);
				}
				
				
			}
		}
	}
	
	/// <summary>
	/// An extremely simple network setup
	/// </summary>
	void NetworkUpdate (NetworkInfo.StatusType status)
	{
		switch (status)
		{	
		case NetworkInfo.StatusType.JoinedRoom:
			statusLabel.text = "Joined Room";
			statusLabel.gameObject.SetActive(false);
			SpawnPlayer();
			break;
		default:
			statusLabel.gameObject.SetActive(true);
			statusLabel.text = status.ToString();
			break;
			
		}
    }
	
	public void SpawnPlayer()
	{
		int randomSpawnLocation = Random.Range (0, spawnPoints.Length);
		
		//Manually allocate PhotonViewID
    	PhotonViewID id1 = PhotonNetwork.AllocateViewID();
		if ( photonView == null ) { Debug.Log ("VIEW IS NULL"); }
		photonView.RPC("SpawnNetworkedPlayer", PhotonTargets.AllBuffered, 
			spawnPoints[randomSpawnLocation].transform.position, 
			spawnPoints[randomSpawnLocation].transform.rotation, id1, PhotonNetwork.player);
	}
	
	[RPC]
	public void SpawnNetworkedPlayer(Vector3 pos, Quaternion rot, PhotonViewID id1, PhotonPlayer np)
	{ 
    	Transform newPlayer = Instantiate(tankPrefab, pos, rot) as Transform;
    	//Set the PhotonView
    	PhotonView[] nViews = newPlayer.GetComponentsInChildren<PhotonView>();
    	nViews[0].viewID = id1;
		
		// Name the gameObject for cleanliness
		newPlayer.gameObject.name = np.name;
		
		// Set tank mode
		if (np.isLocal) 
		{
			playerTank = newPlayer.gameObject;
			playerScript = newPlayer.GetComponent<Tank>();
			playerScript.SetTankType(Tank.TankMode.LocalPlayer);
			cameraManager.orbit = newPlayer.gameObject.transform;
		}
		else
		{
			newPlayer.GetComponent<Tank>().SetTankType(Tank.TankMode.RemotePlayer);
		}
	}
	
	public void Respawn()
	{
		int randomSpawnLocation = Random.Range (0, spawnPoints.Length);
		playerTank.transform.position = spawnPoints[randomSpawnLocation].transform.position;
		playerTank.transform.rotation = spawnPoints[randomSpawnLocation].transform.rotation;
		
		playerScript.Reset();
	}
	
	public void OnEnable()
	{
		NetworkInfo.Instance.OnStatusUpdate += NetworkUpdate;
	}
	public void OnDisable()
	{
		if ( NetworkInfo.Instance != null ) NetworkInfo.Instance.OnStatusUpdate -= NetworkUpdate;;
	}
}
