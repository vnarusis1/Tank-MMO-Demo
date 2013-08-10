using UnityEngine;
using System.Collections;

/// <summary>
/// Tank Tower Controller.
/// </summary>
/// <remarks>
/// Towers always must be modelled to rotate on the Y axis, if they can.
/// </remarks>
public class TankTowerController : MonoBehaviour {
	
	#region Members
	/// <summary>
	/// The tower's transform
	/// </summary>
	public Transform tower;
	
	/// <summary>
	/// The horizontal adjustment speed of the tower
	/// </summary>
	/// <remarks>
	/// Setting this to 0 means that the tower will not rotate
	/// </remarks>
	public float adjustmentSpeed = 30f;
	
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
	
	#region Properties
	/// <summary>
	/// Gets a value indicating whether this <see cref="TankTowerController"/> is on target.
	/// </summary>
	/// <value>
	/// <c>true</c> if is on target; otherwise, <c>false</c>.
	/// </value>
	public bool OnTarget
	{
		get { 
			if ( Mathf.Round(Hydrogen.Math.NeutralizeAngle(TowerRotation)) == 
				 Mathf.Round(TargetRotation) ) 
			{	
				return true;
			}
			return false;
		}
	}
	
	/// <summary>
	/// Gets the targeting difference in degrees.
	/// </summary>
	/// <value>
	/// The targeting difference.
	/// </value>
	public float TargetingDifference
	{
		get { return (Hydrogen.Math.NeutralizeAngle(TargetRotation) - TowerRotation); }
	}
	
	/// <summary>
	/// Gets or sets the parent tank.
	/// </summary>
	/// <value>
	/// The parent tank.
	/// </value>
	public Tank ParentTank { get; set; }
	
	/// <summary>
	/// Gets the current tower rotation in degrees.
	/// </summary>
	/// <value>
	/// The tower rotation in degrees, based on 0 degrees looking straight forward.
	/// </value>
	public float TowerRotation
	{
		get { return tower.localEulerAngles.y; }
	}
	
	/// <summary>
	/// Gets or sets the target rotation in degrees.
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
		get {  return tower.rotation * Vector3.forward; }
	}
	#endregion
	
	#region Unity Functions
	/// <summary>
	/// Unity's Awake Method - Initialization
	/// </summary>
	public void Awake () 
	{
		// Failsafe check, if no turret is set, use the object the script is on
		if ( tower == null ) tower = gameObject.transform;
		
		// Make the targets the current rotations (just makes life easier) 
		_workingRotation = tower.localRotation;
		TargetRotation = tower.localEulerAngles.y;
	}
	
	/// <summary>
	/// Unity's LateUpdate Method
	/// </summary>
	public void LateUpdate()
	{
		
		// No sense moving the tower if it's already where we need it now is there?
		if ( OnTarget ) return;
		
		// Slight variations per the mode the tank is operating in
		if ( ParentTank.mode == Tank.TankMode.LocalPlayer )
		{
			// Rotate the tower to the target rotation (interpolated) based on the adjustment speed
			tower.localEulerAngles = new Vector3(0f, Mathf.MoveTowardsAngle (TowerRotation, TargetRotation, Time.deltaTime * adjustmentSpeed),0f);
		}
		else
		{
			// Rotate the tower to the target rotation (interpolated), smoothing out network jitter
			tower.localEulerAngles = new Vector3(0f, Mathf.LerpAngle(TowerRotation, TargetRotation, Time.deltaTime * 5), 0f);
		}
	}
	#endregion
	
	#region Meat & Potatoes 
	/// <summary>
	/// Updates the TargetRotation to face a designated world position
	/// </summary>
	/// <param name='worldPosition'>
	/// World position.
	/// </param>
	public void UpdateTargetRotationFromWorldPosition(Vector3 worldPosition)
	{
		// Calculate direction from tower to the worldPosition
		_workingVector = worldPosition - tower.position;
		
		// Create a rotation to represent that direction
		_workingRotation = Quaternion.LookRotation(_workingVector, Vector3.up);
		
		// Now that we've created a rotation, throw that to our other function to handle from there
		UpdateTargetRotationFromDirectionalRotation(_workingRotation);
	}
	
	/// <summary>
	/// Updates the TargetRotation from a directional rotation.
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
		
		// Unsigned Difference ( Rotating around Y )
		TargetRotation = Hydrogen.Math.UnsignedAngle(Mathf.DeltaAngle( 
			Mathf.Atan2(ParentTank.ForwardVector.x, ParentTank.ForwardVector.z) * Mathf.Rad2Deg,
			Mathf.Atan2(_workingVector.x, _workingVector.z) * Mathf.Rad2Deg));
	}
	
	/// <summary>
	/// Updates the TargetRotation by making adjustments based on the specified degrees.
	/// </summary>
	/// <param name='adjustment'>
	/// Degree adjustment
	/// </param>
	public void UpdateTargetRotationByDegrees(float adjustment)
	{
		TargetRotation = Hydrogen.Math.UnsignedAngle(TargetRotation + adjustment);
	}
	#endregion
}

