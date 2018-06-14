using UnityEngine;

public sealed class FResource<T> : System.IDisposable where T : Object {
	T _asset;

	public T asset {
		get {
			if (_asset != null)
				return _asset;
			throw new System.NullReferenceException();
		}
	}

	public FResource(string path) {
		_asset = Resources.Load<T>(path);
		if (_asset == null)
			throw new System.NullReferenceException(path);
	}

	public void Dispose() {
		Resources.UnloadAsset(_asset);
		_asset = null;
	}
}