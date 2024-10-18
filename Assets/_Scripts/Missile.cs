using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LSS;
using NAMRU_data;

public class Missile : BM_Object_mono
{
    Stats_Missiles myStats => AircraftManager.Instance.Stats_FriendlyMissiles;
    public FriendlyAircraft myOwner;
    Transform myTransform;
    private EnemyAircraft targetAircraft;
    public EnemyAircraft TargetAircraft => targetAircraft;
    [SerializeField] private float distance_toTarget;

    public Missile()
    {
        NAMRU_Debug.Log( "Missile constructor", BM_Enums.LogDestination.none );
    }

    private void Awake()
	{
        myTransform = GetComponent<Transform>();

	}

	void Start()
    {
        
    }

    public SpatialOrientation FwdOrientation = SpatialOrientation.Y;

    public Vector3 vUp;

    void Update()
    {
		if (GameManager.AmPaused)
		{
			return;
		}

		LogansTransformUtilities.LookAt_2D( myTransform, SpatialOrientation.Y, targetAircraft.transform, myStats.Speed_turn * Time.deltaTime );

        myTransform.Translate( myTransform.up * myStats.Speed_move * Time.deltaTime, Space.World );

        distance_toTarget = Vector2.Distance( transform.position, targetAircraft.transform.position );
        if( distance_toTarget <= myStats.Distance_explode )
        {
            TargetHit();
        }
    }

	public void Init( FriendlyAircraft owner_passed )
	{
        NAMRU_Debug.Log( $"{name} > {nameof(Missile)}.{nameof(Init)}", BM_Enums.LogDestination.none );

		 myOwner = owner_passed;
	}

    public void SetTargetAircraft( EnemyAircraft craft )
    {
		NAMRU_Debug.Log( $"{gameObject} > {nameof(Missile)}.{nameof(SetTargetAircraft)}({craft.name})" );

		targetAircraft = craft;
	}

	public void FireMe(EnemyAircraft craft )
    {
		LogansTransformUtilities.LookAt_2D(
			myTransform.transform, SpatialOrientation.Y, -myOwner.transform.right
			);

		SetTargetAircraft( craft );
		transform.position = myOwner.transform.position + (myOwner.transform.up * 0.5f);
        transform.rotation = myOwner.transform.rotation;
	}

    public void TargetHit()
    {
		NAMRU_Debug.Log( $"{nameof(TargetHit)}" );
		gameObject.SetActive( false );
		targetAircraft.GetComponent<EnemyAircraft>().DamageMe();
        myOwner.TargetHit_action();
        GameManager.Instance.TargetHit_action();
	}



	public override bool CheckIfKosher()
	{
		bool amKosher = true;

		return amKosher;
	}
}
