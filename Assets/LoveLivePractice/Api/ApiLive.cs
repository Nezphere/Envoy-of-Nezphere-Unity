namespace LoveLivePractice.Api {
	[System.Serializable]
	public class ApiLiveResponse {
		public bool succeed;
		public ApiLive content;
	}

	[System.Serializable]
	public class ApiLive {
		public string live_name, live_info, live_id, update_time, state;
		public string artist, bgimg_path, bgm_path, cover_path, map_path;
		public int level, like_count, click_count;
		public bool memberonly;

		public ApiLiveCategory category;
		public ApiLiveUser upload_user;
	}

	[System.Serializable]
	public class ApiLiveCategory {
		public string name;
		public int id;
	}

	[System.Serializable]
	public class ApiLiveUser {
		public string username, avatar_path;
		public int post_count;
	}
}