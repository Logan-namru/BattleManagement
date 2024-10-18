using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LSS
{
    /// <summary>
    /// Describes a spatial orientation. Can be used for both 2D and 3D
    /// </summary>
    public enum SpatialOrientation
    {
        Z,
        PositiveZ,
        NegativeZ,
        X,
        PositiveX,
        NegativeX,
        Y,
        PositiveY,
        NegativeY,
    }

    public static class LogansTransformUtilities
    {
		public static void LookAt_2D( Transform perspectiveTrans, SpatialOrientation fwdOrientation, Vector3 v_goal )
        {
			Quaternion qRot = Quaternion.LookRotation( Vector3.forward, v_goal.normalized );

			perspectiveTrans.rotation = qRot;
			//Debug.Log($"vGenerated: '{vGenerated}', qRot: '{qRot}'");
		}

		public static void LookAt_2D( Transform perspectiveTrans, SpatialOrientation fwdOrientation, Transform targetTrans )
        {
            LookAt_2D( perspectiveTrans, fwdOrientation, targetTrans.position );
        }

		public static void LookAt_2D( Transform perspectiveTrans, SpatialOrientation fwdOrientation, Transform targetTrans, float amount )
		{
			Vector3 vTo = Vector3.Normalize( ShallowVector(targetTrans.position - perspectiveTrans.position) );
            Vector3 vGoal = vTo;

            if( fwdOrientation == SpatialOrientation.Y )
            {
                vGoal = Vector3.RotateTowards( perspectiveTrans.up, vTo, amount, 0f );
            }

			LookAt_2D( perspectiveTrans, fwdOrientation, vGoal );
		}

        public static Vector3 FlatVector( Vector3 v )
        {
            return new Vector3( v.x, 0f, v.z );
        }

        public static Vector3 SkinnyVector( Vector3 v )
        {
            return new Vector3( 0f, v.y, v.z );
        }

        public static Vector3 ShallowVector( Vector3 v )
        {
            return new Vector3( v.x, v.y, 0f );
        }

        public static Vector3 SquishedVect( Vector3 v, bool squishX = false, bool squishY = false, bool squishZ = false )
        {
            return new Vector3( squishX ? 0f : v.x, squishY ? 0f : v.y, squishZ ? 0f : v.z );
        }

        public static float ShallowDot( Vector3 v1, Vector3 v2 )
        {
            return Vector3.Dot( Vector3.Normalize(ShallowVector(v1)), Vector3.Normalize(ShallowVector(v2)) );
        }
	}
}

