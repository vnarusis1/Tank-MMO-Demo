using UnityEngine;
using System.Collections;

// Serves as the hull reference
public class Tank : Photon.MonoBehaviour  {
	
	public enum TankMode {
		LocalPlayer = 0,
		RemotePlayer = 1,
		AIPlayer = 2 // This could be fun to have some ai players on a local machine broadcasting to server
	}
	
	public bool canDrive = false;
	public bool canFire = false;

	public Transform centerOfMass;
	
	
	
	public Transform manualView;
	
	
	
	/// <summary>
	/// This is the tanks mode of operation, LocalPlayer vs NetworkPlayer
	/// </summary> 
	public TankMode mode = TankMode.LocalPlayer;
	
	public TankTracksController tracksController;
	public TankTowerController towerController;
	public TankFixedGunController[] fixedGunsControllers;
	
	public TankFixedGunController primaryFire;
	public TankFixedGunController secondaryFire;
	
	public DamageRegion[] damageRegions;
	
	private float _accelerationForce = 0.0f;
	private float _steeringForce = 0.0f;
	
	
	private Vector3 _networkedPosition;
	private Quaternion _networkedRotation;
	
	
	/// <summary>
	/// Gets or sets the targeted world position.
	/// </summary>
	/// <value>
	/// The targeted world position.
	/// </value>
	public Vector3 TargetedWorldPosition
	{
		get
		{
			return _targetedWorldPosition;
		}
		set
		{
			// Set local referrence just incase we need to reference this later
			_targetedWorldPosition = value;
			
			
			towerController.UpdateTargetRotationFromWorldPosition(value);
			
			if ( fixedGunsControllers.Length > 0 )
			{
				for ( int x = 0; x < fixedGunsControllers.Length; x++ )
				{
					fixedGunsControllers[x].UpdateTarget(value);
				}
			}
			
		}
	}
	private Vector3 _targetedWorldPosition;
	
	public Vector3 ForwardVector
	{
		get {  return transform.rotation * Vector3.forward; }
	}
	
	
	public void Awake()
	{
		// Force assignment of base level controllers
		if ( tracksController == null ) Debug.LogError("No Track Controller was set on Tank script.");
		if ( towerController == null ) Debug.LogError("No Tower Controller was set on Tank script.");
		
		rigidbody.centerOfMass = centerOfMass.localPosition;
		
		_networkedPosition = transform.position;
		_networkedRotation = transform.rotation;
	}
	
	
	public void Start()
	{
		// Configure sub controllers for reference back to parent
		// - this is handy if they are not on the same gameObject
		towerController.ParentTank = this;
		
		if ( fixedGunsControllers.Length > 0 )
		{
			for ( int x = 0; x < fixedGunsControllers.Length; x++ )
			{
				fixedGunsControllers[x].ParentTank = this;
			}
		}
		
		if ( damageRegions.Length > 0 )
		{
			for ( int y = 0; y < damageRegions.Length; y++ )
			{
				damageRegions[y].ParentTank = this;
			}
		}
		
		
	}
	
	public void Reset()
	{
		rigidbody.velocity = Vector3.zero;
		rigidbody.angularVelocity = Vector3.zero;
		
		if ( damageRegions.Length > 0 )
		{
			for ( int y = 0; y < damageRegions.Length; y++ )
			{
				damageRegions[y].health = 100f;
			}
		}
	}
	
	public void SetTankType(TankMode type)
	{
		if ( type == TankMode.LocalPlayer )
		{
			canDrive = true;
			
			canFire = true;
		}
		else if ( type == TankMode.RemotePlayer )
		{
			canDrive = false;
			canFire = false;
		}
		mode = type;
	}
	
	public void Update()
	{	
		
		if ( mode == TankMode.RemotePlayer && !photonView.isMine) 
		{
            transform.position = Vector3.Lerp(transform.position, _networkedPosition, Time.deltaTime * 5);
            transform.rotation = Quaternion.Lerp(transform.rotation, _networkedRotation, Time.deltaTime * 5);
		}
			
		if(canDrive)
		{
			_accelerationForce = Input.GetAxis("Vertical");
			_steeringForce = Input.GetAxis("Horizontal");
		}
		
		if(canFire)
		{
			if ( Input.GetButtonDown("Fire1") && primaryFire != null && primaryFire.canFire )
			{
				this.photonView.RPC ("RPC_FirePrimaryWeapon", PhotonTargets.All);
			}
		}
	}
	
	
	void FixedUpdate(){
		// Update our tracks
		tracksController.UpdateWheels(_accelerationForce,_steeringForce);
	}
	
	public void CreateExplosion(Vector3 worldPosition, Vector3 worldNormal, string explosionEffect, float radius, float damage)
	{
		// Only have local tanks send out their explosions
		if ( mode == TankMode.LocalPlayer )
		{
			
			this.photonView.RPC  ("RPC_CreateExplosion",PhotonTargets.All, worldPosition, worldNormal, explosionEffect, radius, damage);
		}
	}
	public void SpawnEffect(Transform parent, Vector3 position, Vector3 normal, Transform effect)
	{
		if ( mode == TankMode.LocalPlayer )
		{
		}
	}
	
	// PHOTON RELATED
	[RPC]
	public void RPC_FirePrimaryWeapon()
	{
		primaryFire.Fire();
	}
	
	
	[RPC]
	public void RPC_CreateExplosion(Vector3 worldPosition, Vector3 worldNormal, string explosionEffect, float radius, float damage)
	{
		
		
		GameObject explosion = hObjectPool.Instance.Spawn(
			hObjectPool.Instance.GetPoolID(explosionEffect), 
			worldPosition, Quaternion.LookRotation(worldNormal));
		
		if ( explosion.GetComponent<HTSpriteSheet>() )
		{
			explosion.GetComponent<HTSpriteSheet>().OnSpawned();
		}
		
		// Add Damage Radius Check for LOCAL only (this is the only fair way to do this to accomodate for lag) - though this seems
		// like a point where the client could cheat
		
		// Explosion Radius
		RaycastHit[] hit = Physics.SphereCastAll(worldPosition, radius, Vector3.up);
		
		
		// What did we hit? ONLY based on my position
		for(int x = 0; x < hit.Length; x++ )
		{
			// Handle Player Damage
			if ( hit[x].collider.CompareTag("Player") )
			{
				// Only if its you!
				if ( hit[x].collider.gameObject.GetComponent<DamageRegion>().ParentTank.mode == TankMode.LocalPlayer )
				{
					
					hit[x].collider.gameObject.GetComponent<DamageRegion>().ApplyDamage(damage, hit[x].distance);
				}
			}
			
			// Object Destruction
		}

	}
	
	[RPC]
	public void RPC_SpawnEffect(Transform parent, Vector3 position, Vector3 normal, string hitEffect)
	{
		GameObject effect = hObjectPool.Instance.Spawn(
			hObjectPool.Instance.GetPoolID(hitEffect), 
			position, Quaternion.LookRotation(normal));
		
		
		
		if ( parent != null ) effect.transform.parent = parent;
	}
	
	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
			
			stream.SendNext(towerController.TowerRotation);
		
			if ( fixedGunsControllers.Length > 0 )
			{
				for ( int x = 0; x < fixedGunsControllers.Length; x++ )
				{
					stream.SendNext(fixedGunsControllers[x].NetworkRotation);
				}
			}
			
			
			if ( damageRegions.Length > 0 )
			{
				for ( int y = 0; y < damageRegions.Length; y++ )
				{
					stream.SendNext(damageRegions[y].health);
				}
			}
        }
        else
        {
            // Network player, receive data
			_networkedPosition = (Vector3) stream.ReceiveNext();
            _networkedRotation = (Quaternion) stream.ReceiveNext();
			
			towerController.TargetRotation = (float)stream.ReceiveNext();
			
			if ( fixedGunsControllers.Length > 0 )
			{
				for ( int x = 0; x < fixedGunsControllers.Length; x++ )
				{
					fixedGunsControllers[x].NetworkRotation = (float)stream.ReceiveNext();
				}
			}
			
			if ( damageRegions.Length > 0 )
			{
				for ( int y = 0; y < damageRegions.Length; y++ )
				{
					damageRegions[y].health = (float)stream.ReceiveNext();
				}
			}
        }
    }
	
	
}
