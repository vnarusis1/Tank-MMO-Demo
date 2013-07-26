using UnityEngine;
using System.Collections;

public class Ordnance : MonoBehaviour {
	
	public Vector3 shellForce;
	public Vector3 shellTorque;
	public Transform[] explosionEffects;
	public float explosionRadius;
	public float damage;
	
	public Tank SourceTank
	{
		get { return _sourceTank; }
		set { _sourceTank = value; }
	}
	private Tank _sourceTank;
	
	public void Launch()
	{
		rigidbody.AddRelativeForce(shellForce);
		rigidbody.AddRelativeTorque(shellTorque);
	}
	
	public void OnCollisionEnter(Collision collision)
	{	
		if (SourceTank != null )
		{
			_sourceTank.CreateExplosion(collision.contacts[0].point, collision.contacts[0].normal, 
				explosionEffects[Random.Range(0, explosionEffects.Length)].gameObject.name, explosionRadius, damage);
			
			// Remove all forces on shell
			//rigidbody.velocity = Vector3.zero;
			//rigidbody.angularVelocity = Vector3.zero;
			
			// Remove the shell - system auto handles removing forces
			hObjectPool.Instance.Despawn(this.gameObject);
			//PoolManager.Pools["Weapons"].Despawn(this.transform);
		}
    }
		
}