using System;

namespace SkypeLogBackup.Helpers
{
	public static class ProgressHelper
	{
		public static uint ComputeProgressPercentage(uint numDone, uint numTotal)
		{
			if (numDone > numTotal)
				throw new ArgumentOutOfRangeException(nameof(numDone));

			return (uint)(Math.Round((double)numDone / numTotal * 100));
		}
	}
}
