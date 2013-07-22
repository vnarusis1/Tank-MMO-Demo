using UnityEngine;
using System.Collections;

public class TrackDamageRegion : DamageRegion {
	public enum Side
	{
		Left = 0,
		Right = 1
	}
	public Side trackSide;
	public TankTracksController controller;
	
}
