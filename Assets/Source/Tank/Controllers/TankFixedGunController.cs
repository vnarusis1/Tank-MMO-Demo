using UnityEngine;
using System.Collections;

public class TankFixedGunController : MonoBehaviour {
	
	#region Members
	public bool canRotateVertical;
	public float verticalAdjustmentSpeed = 30f;
	public float verticallMinimumAngle = -180f;
	public float verticalMaximumAngle = 180f;
	public float verticalBoost = 10f;
	
	public bool canRotateHorizontal;
	public float horizontalAdjustmentSpeed = 30f;
	public float horizontalMinimumAngle = -180f;
	public float horizontalMaximumAngle = 180f;
	
	/// <summary>
	/// Internal storage for calculations using a quaternion.
	/// </summary>
	/// <remarks>
	/// Helps with GC management
	/// </remarks>
	private Quaternion _workingRotation;
	
	/// <summary>
	/// Internal storage for calculations using a vector3.
	/// </summary>
	/// <remarks>
	/// Helps with GC management
	/// </remarks>
	private Vector3 _workingVector;
	
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
			if ( canRotateHorizontal)
			{
				if ( Mathf.Round(Hydrogen.Math.NeutralizeAngle(GunHorizontalRotation)) != 
						Mathf.Round(Hydrogen.Math.UnsignAngle(TargetHorizontalRotation) ))
				{
					return false;
				}
			}
			
			if ( canRotateVertical )
			{
				if ( Mathf.Round(Hydrogen.Math.NeutralizeAngle(GunVerticalRotation)) != 
						Mathf.Round(Hydrogen.Math.UnsignAngle(TargetVerticalRotation) ))
				{
					return false;
				}
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
	public float TargetingVerticalDifference
	{
		get { return (TargetVerticalRotation - GunVerticalRotation); }
	}
	
	
	/// <summary>
	/// Gets the targeting horizontal difference in degrees.
	/// </summary>
	/// <value>
	/// The targeting horizontal difference.
	/// </value>
	public float TargetingHorizontalDifference
	{
		get { return (TargetHorizontalRotation - GunHorizontalRotation); }
	}
	
	/// <summary>
	/// Gets or sets the parent tank.
	/// </summary>
	/// <value>
	/// The parent tank.
	/// </value>
	public Tank ParentTank { get; set; }
	
	/// <summary>
	/// Gets the current gun horizontal rotation in degrees.
	/// </summary>
	/// <value>
	/// The gun horizontal rotation in degrees, based on 0 degrees looking straight forward.
	/// </value>
	public float GunHorizontalRotation
	{
		get { return gun.localEulerAngles.y; }
	}
	
	/// <summary>
	/// Gets the current gun vertical rotation in degrees.
	/// </summary>
	/// <value>
	/// The gun vertical rotation in degrees, based on 0 degrees looking straight forward.
	/// </value>
	public float GunVerticalRotation
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
	public float TargetVerticalRotation { get; set; }
	
	/// <summary>
	/// Gets or sets the target horizontal rotation in degrees.
	/// </summary>
	/// <value>
	/// The target rotation in degrees.
	/// </value>
	/// <remarks>
	/// 0 degrees is looking straight forward
	/// </remarks>
	public float TargetHorizontalRotation { get; set; }
	
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
		_workingRotation = gun.localRotation;
		TargetHorizontalRotation = gun.localEulerAngles.y;
		TargetVerticalRotation = gun.localEulerAngles.x;
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
			gun.localEulerAngles = new Vector3(
				Mathf.MoveTowardsAngle (GunVerticalRotation, TargetVerticalRotation, Time.deltaTime * verticalAdjustmentSpeed), 
				Mathf.MoveTowardsAngle (GunHorizontalRotation, TargetHorizontalRotation, Time.deltaTime * horizontalAdjustmentSpeed),
				0f);
		}
		else
		{
			// Rotate the gun to the target rotation (interpolated), smoothing out network jitter
			gun.localEulerAngles = new Vector3(
				Mathf.LerpAngle(GunVerticalRotation, TargetVerticalRotation, Time.deltaTime * 5),
				Mathf.LerpAngle(GunHorizontalRotation, TargetHorizontalRotation, Time.deltaTime * 5),
				0f);
		}
	}
	#endregion
	
	
	
	#region Meat & Potatoes 
	public void Fire()
	{
		// Reset Reload Time
		_reloadTime = reloadTime;
		GameObject shell = hObjectPool.Instance.Spawn(activeOrdanance, spawnLocation.position, spawnLocation.rotation);
		shell.GetComponent<Ordnance>().Launch();
		shell.GetComponent<Ordnance>().SourceTank = ParentTank;
	}
	
	
	/// <summary>
	/// Updates the TargetHorizontalRotation and TargetVerticalRotation to face a designated world position
	/// </summary>
	/// <param name='worldPosition'>
	/// World position.
	/// </param>
	public void UpdateTargetRotationFromWorldPosition(Vector3 worldPosition)
	{
		// Calculate direction from tower to the worldPosition
		_workingVector = worldPosition - gun.position;
		
		// Create a rotation to represent that direction
		_workingRotation = Quaternion.LookRotation(_workingVector, Vector3.up);
		
		// Now that we've created a rotation, throw that to our other function to handle from there
		UpdateTargetRotationFromDirectionalRotation(_workingRotation);
	}
	
	/// <summary>
	/// Updates the TargetHorizontalRotation and TargetVerticalRotation from a directional rotation.
	/// </summary>
	/// <remarks>
	/// A camera's transform's rotation for example
	/// </remarks>
	/// <param name='rotation'>
	/// Rotation.
	/// </param>
	public void UpdateTargetRotationFromDirectionalRotation(Quaternion rotation)
	{
		// Determine rotations's forward vector
		_workingVector = rotation * Vector3.forward;
		
		// Signed Difference
		if ( canRotateVertical )
		{
			TargetVerticalRotation = 
				Hydrogen.Math.ClampAngle(
					Mathf.DeltaAngle(
						Mathf.Atan2(ParentTank.towerController.ForwardVector.y, ParentTank.towerController.ForwardVector.z) * Mathf.Rad2Deg,
						Mathf.Atan2(_workingVector.y, _workingVector.z) * Mathf.Rad2Deg)  * -1f,
					verticallMinimumAngle,
					verticalMaximumAngle);
		}
			
		
		if ( canRotateHorizontal )
		{
			TargetHorizontalRotation = 
				Hydrogen.Math.ClampAngle(
					Mathf.DeltaAngle( 
						Mathf.Atan2(ParentTank.towerController.ForwardVector.x, ParentTank.towerController.ForwardVector.z) * Mathf.Rad2Deg, 
						Mathf.Atan2(_workingVector.x, _workingVector.z) * Mathf.Rad2Deg),
					horizontalMinimumAngle,
					horizontalMaximumAngle);
		}
	}
	
	/// <summary>
	/// Updates the TargetHorizontalRotation and TargetVerticalRotation by making adjustments based
	/// on the specified degrees.
	/// </summary>
	/// <param name='adjustment'>
	/// Degree adjustment
	/// </param>
	public void UpdateTargetRotationByDegrees(float horizontalAdjustment, float verticalAdjustment)
	{
		if ( canRotateHorizontal )
		{
			TargetHorizontalRotation = Hydrogen.Math.ClampAngle(
				TargetHorizontalRotation + horizontalAdjustment,
				horizontalMinimumAngle,
				horizontalMaximumAngle);
		}

		if ( canRotateVertical )  
		{
			TargetVerticalRotation = Hydrogen.Math.ClampAngle(
				TargetVerticalRotation + verticalAdjustment,
				verticallMinimumAngle,
				verticalMaximumAngle);
		}
	}
	#endregion
	
	
}
