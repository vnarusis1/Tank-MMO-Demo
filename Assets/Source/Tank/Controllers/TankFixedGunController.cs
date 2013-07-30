using UnityEngine;
using System.Collections;

public class TankFixedGunController : MonoBehaviour {
	
	public Transform gun;
	
	public float maximumAngle = 60f;
	public float minimumAngle = -5f;
	public float maximumTargetDistance = 20f;
	
	public GameObject activeOrdanance;
	public Transform spawnLocation;
	
	/// <summary>
	/// The vertical adjustment speed of the barrel;
	/// </summary>
	public float adjustmentSpeed = 0.5f;
	
	public float reloadTime = 1f;
	private float _reloadTime = 1f;
	
	
	public Tank ParentTank
	{
		get { return _parentTank; }
		set { _parentTank = value; }
	}
	private Tank _parentTank;
	
	public bool canFire 
	{ 
		get 
		{ 
			if ( _reloadTime <= 0 ) { return true; }
			else { return false; }
		} 
	}
	
	
	private Quaternion _workingRotation;
	private Vector3 _workingPosition;
	public float NetworkRotation
	{
		get { return gun.localEulerAngles.x; }
		set { _targetRotation = value; }
	}
	private float _targetRotation;
	
	
	
	public void Awake () 
	{
		// Failsafe check, if no gun is set, use the object the script is on
		if ( gun == null ) gun = gameObject.transform;
		
		// Establish base rotation to calculate angles off of
		// This is a safety check incase the part of the tanks rotation isn't zero'd out 
		//(as was the case for the stock tank)
		_workingRotation = gun.localRotation;
	}
	
	public void Update ()
	{
		// Reload Timer
		if ( _reloadTime > 0 )
		{
			_reloadTime -= Time.deltaTime;
		}
	}
	
	public void LateUpdate()
	{
		if ( _parentTank.mode == Tank.TankMode.LocalPlayer )
		{
			//gun.rotation = Quaternion.RotateTowards(gun.rotation, _workingRotation, adjustmentSpeed * Time.deltaTime);
			
			// Restrict Rotation to X axis
			//gun.localEulerAngles = new Vector3(gun.localEulerAngles.x,0f,0f);
		}
		else
		{
			//gun.localEulerAngles = new Vector3(Mathf.Lerp(gun.localEulerAngles.x, _targetRotation, Time.deltaTime * 5),0f, 0f);
		}
	}
	
	public void UpdateTarget(Vector3 position)
	{
		
		_workingPosition = position - gun.position;
		Vector3 aim = new Vector3(0f, _workingPosition.y, 0f);
		_workingPosition.Set (_workingPosition.x, 0f, _workingPosition.z);
		aim.Set(aim.x, aim.y, _workingPosition.magnitude);

		
		//_workingPosition = new Vector3(_workingPosition.x, tower.position.y, _workingPosition.z);
		_workingRotation = Quaternion.LookRotation(aim, Vector3.up);
		
	}
	
	public void Fire()
	{
		// Reset Reload Time
		_reloadTime = reloadTime;
		GameObject shell = hObjectPool.Instance.Spawn(activeOrdanance, spawnLocation.position, spawnLocation.rotation);
		//Transform shell = PoolManager.Pools["Weapons"].Spawn(activeOrdanance, spawnLocation.position, spawnLocation.rotation);
		shell.GetComponent<Ordnance>().Launch();
		shell.GetComponent<Ordnance>().SourceTank = _parentTank;
	}
	
	
		#region Meat & Potatoes 
	/// <summary>
	/// Updates the TargetRotation to face a designated world position
	/// </summary>
	/// <param name='worldPosition'>
	/// World position.
	/// </param>
	public void UpdateTargetRotationFromWorldPosition(Vector3 worldPosition)
	{

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
	
	}
	
	/// <summary>
	/// Updates the TargetRotation by making adjustments based on the specified degrees.
	/// </summary>
	/// <param name='adjustment'>
	/// Degree adjustment
	/// </param>
	public void UpdateTargetRotationByDegrees(float adjustment)
	{
	
	}
	#endregion
	
	
}
