namespace LoveLivePractice.Api {
	[System.Serializable]
	public class ApiLiveListResponse {
		public bool succeed;
		public ApiLiveList content;
	}

	[System.Serializable]
	public class ApiLiveList {
		public int count;
		public ApiLiveListItem[] items;
	}

	[System.Serializable]
	public class ApiLiveListItem {
		public string live_name, live_id, artist, cover_path;
		public int click_count, level;
		public bool memberonly;
		public ApiLiveListItemUser upload_user;
	}

	[System.Serializable]
	public class ApiLiveListItemUser {
		public string username;
	}
}