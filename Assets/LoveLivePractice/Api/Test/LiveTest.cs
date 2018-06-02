using UnityEngine;

using LoveLivePractice.Api;

//using LoveLivePractice.Net;

public class LiveTest : MonoBehaviour {
	public ApiLiveResponse Response, ApiResponse;
	public ApiLiveMap Map;
	public Texture2D Texture;
	public AudioClip Clip;
	public AudioSource Source;
	public UnityEngine.UI.RawImage uiRawImage;

	//	public void OnValidate() {
	//		Response = JsonUtility.FromJson<ApiLiveResponse>("{\"content\": {\"bgimg_path\": \"7dXuuHqnwgcB.png\", \"bgm_path\": \"aWtJaxr7Hoqb.mp3\", \"like_count\": 2, \"live_name\": \"\\u3086\\u308b\\u3086\\u308a\\u3093\\u308a\\u3093\\u308a\\u3093\\u308a\\u3093\\u308a\\u3093\", \"added_fav\": null, \"map_file\": \"gTDZcwr6IAdk    \", \"category\": {\"id\": 5, \"name\": \"TECHNICAL\"}, \"cover_path\": \"5tXCgTRDsRrS.jpg\", \"state\": \"public\", \"bgm_file\": \"VkH6HVasChyO    \", \"update_time\": \"2017-09-08T14:00:24.300304\", \"live_info\": \"\\u662f\\u6447\\u66f3\\u767e\\u5408\\u54d2\\uff01\\n\\u67d0\\u4e00\\u5b63ova\\u7684op\", \"memberonly\": false, \"level\": 10, \"customize_path\": \"\", \"bgimg_file\": \"MwTRTTVqZYHE    \", \"upload_user\": {\"username\": \"\\u6447\\u66f3\\u7684\\u7a97\\u5e18\", \"avatar_path\": \"uV4usGve3caL.png    \", \"id\": 20319, \"post_count\": 6}, \"click_count\": 486, \"artist\": \"\\u4e03\\u68ee\\u4e2d\\u2606\\u3054\\u3089\\u304f\\u90e8\", \"map_path\": \"c0ciaJLQ4J3MXj.json\", \"assets_path\": \"\", \"live_setting\": \"\", \"added_like\": null, \"live_id\": \"OF5LhdKvkujnEWYj\"}, \"succeed\": true}");
	//
	//		string json = SyncUtil.Get(UrlBuilder.GetLiveUrl("h4SFEBaRxDXz3bzB"));
	//		Debug.Log(json);
	//
	//		ApiResponse = JsonUtility.FromJson<ApiLiveResponse>(json);
	//
	//		json = SyncUtil.Get(UrlBuilder.GetUploadUrl(ApiResponse.content.map_path));
	//		Debug.Log(ApiLiveMap.Transform(json));
	//
	//		Map = JsonUtility.FromJson<ApiLiveMap>(ApiLiveMap.Transform(json));
	//
	//		byte[] bytes = SyncUtil.GetBytes(UrlBuilder.GetUploadUrl(ApiResponse.content.cover_path));
	//		Texture = new Texture2D(4, 4);
	//		Texture.LoadImage(bytes);
	//		uiRawImage.texture = Texture;
	//	}
	//
	//	public System.Collections.IEnumerator Start() {
	//		byte[] rawData = SyncUtil.GetBytes(UrlBuilder.GetUploadUrl(ApiResponse.content.bgm_path));
	//		string tempFile = Application.persistentDataPath + "/" + ApiResponse.content.bgm_path;
	//		Debug.Log(tempFile);
	//		System.IO.File.WriteAllBytes(tempFile, rawData);
	//
	//		var loader = new WWW("file:///" + tempFile);
	//		yield return loader;
	//
	//		if (!string.IsNullOrEmpty(loader.error)) Debug.LogError(loader.error);
	//		Clip = loader.GetAudioClip();
	//
	//		Source.clip = Clip;
	//		Source.Play();
	//	}
}