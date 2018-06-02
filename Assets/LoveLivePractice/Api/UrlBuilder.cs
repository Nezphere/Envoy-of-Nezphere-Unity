namespace LoveLivePractice.Api {
	public static class UrlBuilder {
		public const int ApiLimit = 24;

		public const string ApiBaseUrl = "https://m.tianyi9.com/API/";
		public const string UploadBaseUrl = "https://m.tianyi9.com/upload/";

		public static string GetLiveListUrl(int offset, int limit) {
			return ApiBaseUrl + string.Format("getlivelist?type=public&offset={0}&limit={1}", offset, limit);
		}

		public static string GetLiveListUrl(int offset, int limit, int category) {
			return ApiBaseUrl + string.Format("getlivelist?type=category&offset={0}&limit={1}&category={2}", offset, limit, category);
		}

		public static string GetLiveUrl(string id) {
			return ApiBaseUrl + "getlive?live_id=" + id;
		}

		public static string GetUploadUrl(string path) {
			return UploadBaseUrl + path;
		}

		public static string GetCachedUploadUrl(string path) {
			if (System.IO.File.Exists(UnityEngine.Application.persistentDataPath + "/" + path)) {
				return "file:///" + UnityEngine.Application.persistentDataPath + "/" + path;
			}

			return GetUploadUrl(path);
		}
	}
}

