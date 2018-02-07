using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.UI;

namespace Monopoly
{
	public class JoinGame : MonoBehaviour
	{
		List<GameObject> roomList = new List<GameObject>();

		[SerializeField]
		private Text status;

		[SerializeField]
		private GameObject roomListItemPrefab;

		[SerializeField]
		private Transform roomListParent;

		private NetworkManager networkManager;

		private void Start()
		{
			networkManager = NetworkManager.singleton;

			if (networkManager.matchMaker == null)
			{
				networkManager.StartMatchMaker();
			}

			RefreshRoomList();
		}

		public void RefreshRoomList()
		{
			ClearRoomList();
			networkManager.matchMaker.ListMatches(0, 20, "", true, 0, 0, OnMatchList);
			status.text = "Loading...";
		}

		public void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matchList)
		{
			status.text = "";

			if (!success || matchList == null)
			{
				status.text = "Couldn't get room list.";
				return;
			}

			foreach (MatchInfoSnapshot match in matchList)
			{
				GameObject _roomListItemGO = Instantiate(roomListItemPrefab);
				_roomListItemGO.transform.SetParent(roomListParent);
				_roomListItemGO.transform.localScale = Vector3.one;

				RoomListItem _roomListItem = _roomListItemGO.GetComponent<RoomListItem>();
				if (_roomListItem != null)
				{
					_roomListItem.Setup(match, JoinRoom);
				}

				roomList.Add(_roomListItemGO);
			}

			if (roomList.Count == 0)
			{
				status.text = "No rooms";
			}
		}

		private void ClearRoomList()
		{
			foreach (GameObject room in roomList)
			{
				Destroy(room);
			}

			roomList.Clear();
		}

		public void JoinRoom(MatchInfoSnapshot _match)
		{
			networkManager.matchMaker.JoinMatch(_match.networkId, "", "", "", 0, 0, networkManager.OnMatchJoined);
			ClearRoomList();
			status.text = "Joining...";
		}
	}
}
