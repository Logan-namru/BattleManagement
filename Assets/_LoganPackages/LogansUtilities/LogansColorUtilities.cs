using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LSS
{
    public static class LogansColorUtilities
    {
		public static Color BlipColor(Color offColor, Color onColor, Color currentColor, float blipDuration, float blipCurrentProgress, float blipPeak_percentage)
		{
			Color outColor = currentColor;

			float remndr = blipDuration - blipCurrentProgress;
			float peak = blipDuration * blipPeak_percentage;
			float timeLineAmountAbovePeak = blipDuration - peak;

			if (blipCurrentProgress < peak)
			{
				outColor = new Color(currentColor.r, currentColor.g, currentColor.b,
					blipCurrentProgress / peak
				);
				//Debug.Log( $"Going up. blipping from '{currentColor.a}' to '{outColor.a}'" );

			}
			else
			{
				outColor = new Color(currentColor.r, currentColor.g, currentColor.b,
					//(blipCurrentProgress - peak) / timeLineAmountAbovePeak //wrong direction
					//timeLineAmountAbovePeak / (blipCurrentProgress - peak) //goes too high
					//(timeLineAmountAbovePeak / (blipCurrentProgress - peak)) * timeLineAmountAbovePeak //stays above 1 for a while...
					1f - ((blipCurrentProgress - peak) / timeLineAmountAbovePeak)  //wrong direction

				);
				//Debug.Log($"Going DOWN. blipping from '{currentColor.a}' to '{outColor.a}'");

			}


			return outColor;
		}
	}
}

