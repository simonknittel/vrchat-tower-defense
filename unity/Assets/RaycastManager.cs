
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace SimonKnittel.TowerDefense
{
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class RaycastManager : UdonSharpBehaviour
	{
		VRCPlayerApi _localPlayer;
		GameObject _currentTarget;
		bool _isUserInVR = true;

		void Start()
		{
			_localPlayer = Networking.LocalPlayer;
			_isUserInVR = _localPlayer.IsUserInVR();
		}

		void Update()
		{
			if (_currentTarget != null)
			{
				_currentTarget.GetComponent<Renderer>().enabled = false;
				_currentTarget = null;
			}

			VRCPlayerApi.TrackingData trackingData;
			if (_isUserInVR)
			{
				trackingData = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand);
			}
			else
			{
				trackingData = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
			}
			var origin = trackingData.position;
			var direction = trackingData.rotation * Vector3.forward;
			var ray = new Ray(origin, direction);

			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, 5f))
			{
				if (hit.transform && hit.transform.gameObject.name.Contains("RaycastTarget"))
				{
					_currentTarget = hit.transform.gameObject;
					_currentTarget.GetComponent<Renderer>().enabled = true;
				}
			}
		}
	}
}
