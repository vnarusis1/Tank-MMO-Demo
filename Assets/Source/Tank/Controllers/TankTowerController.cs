using UnityEngine;
using System.Collections;

public class TankTowerController : MonoBehaviour {
	
	
	public Transform tower;
	
	/// <summary>
	/// The horizontal adjustment speed of the tower
	/// </summary>
	public float adjustmentSpeed = 30f;
	
	public Tank ParentTank
	{
		get { return _parentTank; }
		set { _parentTank = value; }
	}
	
	
	private Tank _parentTank;
	private Quaternion _workingRotation;
	private Vector3 _workingPosition;
	public float NetworkRotation
	{
		get { return tower.localEulerAngles.y; }
		set { _targetRotation = value; }
	}
	private float _targetRotation;
	

	
	
	void Awake () 
	{
		// Failsafe check, if no turret is set, use the object the script is on
		if ( tower == null ) tower = gameObject.transform;
		
		// Make the targets the current rotations (just makes life easier) 
		_workingRotation = tower.localRotation;
	}
	
	void LateUpdate()
	{
		if ( _parentTank.mode == Tank.TankMode.LocalPlayer )
		{
			tower.rotation = Quaternion.RotateTowards(tower.rotation, _workingRotation, adjustmentSpeed * Time.deltaTime);
			
			// Restrict Rotation to Y axis
			tower.localEulerAngles = new Vector3(0,tower.localEulerAngles.y,0);
		}
		else
		{
			tower.localEulerAngles = new Vector3(0f, Mathf.Lerp(tower.localEulerAngles.y, _targetRotation, Time.deltaTime * 5), 0f);
		}
	}
	

	public void UpdateTarget(Vector3 position)
	{
		_workingPosition = position - tower.position;
		//_workingPosition = new Vector3(_workingPosition.x, tower.position.y, _workingPosition.z);
		_workingRotation = Quaternion.LookRotation(_workingPosition, Vector3.up);
		
	}
	
	// USEFUL FOR SOMETHING ELSE MAYBE
	public void UpdateTargetRotation(Transform camera)
	{
		// Establish forward facing vectors
		Vector3 tankForward = _parentTank.transform.rotation * Vector3.forward;
		Vector3 cameraForward = camera.rotation * Vector3.forward;
		float tankAngle = Mathf.Atan2(tankForward.x, tankForward.z) * Mathf.Rad2Deg;
		float cameraAngle = Mathf.Atan2(cameraForward.x, cameraForward.z) * Mathf.Rad2Deg;
		float angleDiff = Mathf.DeltaAngle( tankAngle, cameraAngle );
		
		// Target Angle (Signed)
//		_targetTowerAngle = angleDiff;
	}
	
	/*
	 * 	if ( tower.localEulerAngles.y != _targetTowerAngle )
		{
			tower.localEulerAngles = new Vector3(
				0f, Mathf.LerpAngle(tower.localEulerAngles.y, _targetTowerAngle, adjustmentSpeed * Time.deltaTime), 0f);
		}
		*/
	public float TowerAngle
	{
		get { return tower.localEulerAngles.y; }
	}
	
}

