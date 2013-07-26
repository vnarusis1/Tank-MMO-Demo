using UnityEngine;
using System.Collections;

public class TankCameraController : MonoBehaviour {
	
	
	public enum CameraMode
	{
		MouseOrbitTarget = 0,
		LookingDownSights = 1
	}
	
	public CameraMode mode = CameraMode.MouseOrbitTarget;
	
	public Transform orbit;
	public float targetDistance = 10.0f;
	
	public float horizontalSpeed = 250f;
	public float verticalSpeed = 120f;
	public float verticalMinimum = -20f;
	public float verticalMaximum = 80f;
	
	
	/// <summary>
	/// Gets or sets the target camera.
	/// </summary>
	/// <value>
	/// The target camera used in calculating the direction to rotate the turret as well as elevate the barrel.
	/// </value>
	public Camera TargetCamera
	{
		get { return _targetCamera; }
		set { _targetCamera = value; }
	}
	
	private float _x = 0.0f;
	private float _y = 0.0f;
	private Camera _targetCamera;
	private Quaternion _rotation;
	private Vector3 _position;
	
	
	private Tank _parentTank;
	public Tank ParentTank
	{
		get { return _parentTank; }
		set { _parentTank = value; }
	}

	public void Start() {
		
		// Failsafe check against having no camera to use for moving the turrent;
		if ( _targetCamera == null ) _targetCamera = Camera.main;
		
		_x = _targetCamera.transform.eulerAngles.x;
		_y = _targetCamera.transform.eulerAngles.y;	
		
	}
	
	public void UpdateCamera(float horizontalAdjustment, float verticalAdjustment)
	{
		if ( mode == CameraMode.MouseOrbitTarget && orbit) 
		{
			_x += horizontalAdjustment * horizontalSpeed * 0.02f;
        	_y -= verticalAdjustment * verticalSpeed * 0.02f;
 		
 			_y = ClampAngle(_y, verticalMinimum, verticalMaximum);
			
       		_rotation =  Quaternion.Euler(_y, _x, 0);
			_position = _rotation * new Vector3(0.0f, 0.0f, -targetDistance) + orbit.position;
			
			_targetCamera.transform.rotation = _rotation;
			_targetCamera.transform.position = _position;
		}
	}
	
	public void ManualAim(float horizontalAdjustment, float verticalAdjustment)
	{
		_targetCamera.transform.position = _parentTank.manualView.transform.position;
		_targetCamera.transform.rotation = _parentTank.manualView.transform.rotation;
		
		//ParentTank.towerController.TargetTowerAngle += (horizontalAdjustment * horizontalSpeed * 0.01f);
	}
	
	private float ClampAngle(float angle, float min, float max) {
		if (angle < -360) angle += 360;
		if (angle > 360) angle -= 360;
		return Mathf.Clamp (angle, min, max);
	}
}