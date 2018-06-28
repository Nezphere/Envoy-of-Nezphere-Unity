using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using ICSharpCode.SharpZipLib.Zip;

public class SongUiController : MonoBehaviour {
	public RectTransform uiScrollTrans;
	public GameObject uiScrollItemProto;

	void Start() {
		string path = Application.persistentDataPath;

		var dirs = Directory.GetDirectories(path);

		var rootFiles = Directory.GetFiles(path);
		foreach (var file in rootFiles) {
			if (file.Substring(file.Length - 4, 4).ToLower() == ".osz") {
				print(file);
				string name = Path.GetFileNameWithoutExtension(file);
				if (!System.Array.Exists(dirs, e => e == name)) {  // New .osz
					var resultPath = Path.Combine(path, name.Substring(0, 6));
					Directory.CreateDirectory(resultPath);
					Debug.Log(file + " => " + resultPath);
					ExtractZipFile(file, resultPath);
				}
			}
		}

		dirs = Directory.GetDirectories(path);

		OsuFile osuFile = null;
		foreach (var dir in dirs) {
			print(dir);
			var files = Directory.GetFiles(dir);
			foreach (var file in files) {
				if (file.Substring(file.Length - 4, 4).ToLower() == ".osu") {
					print(file);
					var text = File.ReadAllText(file);
					osuFile = new OsuFile(text, dir);

					var go = Instantiate(uiScrollItemProto, uiScrollTrans);
					var item = go.GetComponent<SongItemUiController>();
					item.Init(osuFile);
				}
			}
		}
	}

	public void ExtractZipFile(string archiveFilenameIn, string outFolder) {
		ZipFile zf = null;
		try {
			FileStream fs = File.OpenRead(archiveFilenameIn);
			zf = new ZipFile(fs);
			foreach (ZipEntry zipEntry in zf) {
				if (!zipEntry.IsFile) {
					continue;			// Ignore directories
				}

				try {
					string entryFileName = zipEntry.Name;
					// to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
					// Optionally match entrynames against a selection list here to skip as desired.
					// The unpacked length is available in the zipEntry.Size property.

					byte[] buffer = new byte[4096];		// 4K is optimum
					Stream zipStream = zf.GetInputStream(zipEntry);

					// Manipulate the output filename here as desired.
					string fullZipToPath = Path.Combine(outFolder, entryFileName);
					string directoryName = Path.GetDirectoryName(fullZipToPath);
					if (directoryName.Length > 0)
						Directory.CreateDirectory(directoryName);

					// Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
					// of the file, but does not waste memory.
					// The "using" will close the stream even if an exception occurs.
					using (FileStream streamWriter = File.Create(fullZipToPath)) {
						ICSharpCode.SharpZipLib.Core.StreamUtils.Copy(zipStream, streamWriter, buffer);
					}
				} catch (System.Exception e) {
					Debug.Log(e);
				}
			}
		} finally {
			if (zf != null) {
				zf.IsStreamOwner = true; // Makes close also shut the underlying stream
				zf.Close(); // Ensure we release resources
			}
		}
	}
}
