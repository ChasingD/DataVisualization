using UnityEngine;

namespace Module.Session
{
	/// <summary>
	/// This script exists as a stub to allow other scripts to find
	/// the shared world anchor transform.
	/// </summary>
	public class SharedCollection : Singleton<SharedCollection>
	{
		public Transform root;
	}
}