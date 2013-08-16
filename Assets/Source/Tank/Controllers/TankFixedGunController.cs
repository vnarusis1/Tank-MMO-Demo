using UnityEngine;
using System.Collections;

public class TankFixedGunController : MonoBehaviour {
	
	#region Members
	public float adjustmentSpeed = 30f;
	public float minimumAngle = -180f;
	public float maximumAngle = 180f;
	
	/// <summary>
	/// Internal storage for calculations using a vector3.
	/// </summary>
	/// <remarks>
	/// Helps with GC management
	/// </remarks>
	private Vector3 _workingVector;
	private Quaternion _workingRotation;
	
	
	#endregion
	
	
	public Transform gun;

	public float maximumTargetDistance = 20f;
	public GameObject activeOrdanance;
	public Transform spawnLocation;
	public float reloadTime = 1f;
	private float _reloadTime = 1f;
	
	#region Properties
	public bool CanFire 
	{ 
		get 
		{ 
			if ( _reloadTime <= 0 ) { return true; }
			else { return false; }
		} 
	}
	
	/// <summary>
	/// Gets a value indicating whether this <see cref="TankFixedGunController"/> is on target.
	/// </summary>
	/// <value>
	/// <c>true</c> if is on target; otherwise, <c>false</c>.
	/// </value>
	public bool OnTarget
	{
		get { 
			
			if ( Mathf.Round(Hydrogen.Math.NeutralizeAngle(GunRotation)) != Mathf.Round(TargetRotation))
			{
					return false;
			}
			return true;
		}
	}
	
	/// <summary>
	/// Gets the targeting vertical difference in degrees.
	/// </summary>
	/// <value>
	/// The targeting vertical difference.
	/// </value>
	public float TargetingDifference
	{
		get 
		{ 
			return (TargetRotation - Hydrogen.Math.NeutralizeAngle(GunRotation)); 
		}
	}
	
	/// <summary>
	/// Gets or sets the parent tank.
	/// </summary>
	/// <value>
	/// The parent tank.
	/// </value>
	public Tank ParentTank { get; set; }
	
	/// <summary>
	/// Gets the current gun vertical rotation in degrees.
	/// </summary>
	/// <value>
	/// The gun vertical rotation in degrees, based on 0 degrees looking straight forward.
	/// </value>
	public float GunRotation
	{
		get { return gun.localEulerAngles.x; }
	}
	
	/// <summary>
	/// Gets or sets the target vertical rotation in degrees.
	/// </summary>
	/// <value>
	/// The target rotation in degrees.
	/// </value>
	/// <remarks>
	/// 0 degrees is looking straight forward
	/// </remarks>
	public float TargetRotation { get; set; }

	public Vector3 ForwardVector
	{
		get {  return gun.rotation * Vector3.forward; }
	}
	#endregion

	#region Unity Functions
	/// <summary>
	/// Unity's Awake Method - Initialization
	/// </summary>
	public void Awake () 
	{
		// Failsafe check, if no gun is set, use the object the script is on
		if ( gun == null ) gun = gameObject.transform;
		
		// Establish base rotation to calculate angles off of
		TargetRotation = gun.localEulerAngles.x;
	}
	
	/// <summary>
	/// Unity's LateUpdate Method
	/// </summary>
	public void LateUpdate()
	{
		// Reload Timer
		if ( _reloadTime > 0 )
		{
			_reloadTime -= Time.deltaTime;
		}

		// No sense moving the gun if it's already where we need it now is there?
		if ( OnTarget ) return;
		
		// Slight variations per the mode the tank is operating in
		if ( ParentTank.mode == Tank.TankMode.LocalPlayer )
		{
			// Rotate the gun to the target rotation (interpolated) based on the adjustment speed
			gun.localEulerAngles = new Vector3(Mathf.MoveTowardsAngle(GunRotation, TargetRotation, Time.deltaTime * adjustmentSpeed), 0f, 0f);
		}
		else
		{
			// Rotate the gun to the target rotation (interpolated), smoothing out network jitter
			gun.localEulerAngles = new Vector3(Mathf.LerpAngle(GunRotation, TargetRotation, Time.deltaTime * 5), 0f, 0f);
		}
	}
	#endregion
	
	#region Meat & Potatoes 
	public void Fire()
	{
		// Reset Reload Time
		_reloadTime = reloadTime;
		GameObject shell = hObjectPool.Instance.Spawn(activeOrdanance, spawnLocation.position, spawnLocation.rotation);
		shell.GetComponent<Ordnance>().SourceTank = ParentTank;
		shell.GetComponent<Ordnance>().Launch();
	}
	
	/// <summary>
	/// Updates the TargetRotatiob to face a designated world position
	/// </summary>
	/// <param name='worldPosition'>
	/// World position.
	/// </param>
	public void UpdateTargetRotationFromWorldPosition(Vector3 worldPosition)
	{
		//find the vector pointing from our position to the target
	    _workingVector = (worldPosition - gun.position);
	 
		TargetRotation = 
			Quaternion.LookRotation(_workingVector, Vector3.up).eulerAngles.x +
			Quaternion.LookRotation(worldPosition - ParentTank.transform.position, Vector3.up).eulerAngles.x;
	}

	
	
	/// <summary>
	/// Updates the TargetHorizontalRotation and TargetVerticalRotation by making adjustments based
	/// on the specified degrees.
	/// </summary>
	/// <param name='adjustment'>
	/// Degree adjustment
	/// </param>
	public void UpdateTargetRotationByDegrees(float verticalAdjustment)
	{
		TargetRotation = Mathf.Clamp (TargetRotation + verticalAdjustment, minimumAngle, maximumAngle);
	}
	
	#endregion	
	
	#if UNITY_EDITOR
	public void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawRay(gun.position, _workingVector * 100f);
		
		
        Gizmos.color = Color.white;
        Gizmos.DrawRay(
			spawnLocation.position, 
			spawnLocation.TransformDirection(-Vector3.up) * 100f
		);
	}
	#endif
}
