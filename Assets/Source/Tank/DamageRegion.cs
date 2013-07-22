using UnityEngine;
using System.Collections;

public class DamageRegion : MonoBehaviour {
	
	public float damageReduction = 0f;
	public float health = 100f;
	public string statusName = "";
	
	public Tank ParentTank
	{
		get { return _parentTank; }
		set { _parentTank = value; }
	}
	private Tank _parentTank;
	
	public virtual void ApplyDamage(float damage, float distance)
	{
		health -= damage;
	}	
}
