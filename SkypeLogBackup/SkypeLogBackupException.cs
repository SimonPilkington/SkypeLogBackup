using System;

namespace SkypeLogBackup
{
	[Serializable]
	public class SkypeLogBackupException : Exception
	{
		public SkypeLogBackupException() { }
		public SkypeLogBackupException(string message) : base(message) { }
		public SkypeLogBackupException(string message, Exception inner) : base(message, inner) { }
		protected SkypeLogBackupException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
