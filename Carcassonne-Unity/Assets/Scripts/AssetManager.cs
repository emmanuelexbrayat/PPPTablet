using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;

public class AssetManager : MonoBehaviour {

    public static AssetManager instance;

    public Texture2D noTex;

    static string basePath;
    static bool isInit;
    
    void Awake()
    {
        instance = this;
    }
	// Use this for initialization
	public static void init () {
#if UNITY_EDITOR
        basePath = Directory.GetCurrentDirectory().Replace('\\','/') + "/../Sync/";
#else
        basePath = "/storage/emulated/0/PPP/"; //Android sync folder
        
#endif
        Debug.Log("Base Path :" + basePath + ": exists ? " + Directory.Exists(basePath));

        isInit = true;
	}

    
    public static string getFileData(string filePath)
    {
        if (!isInit) init();
        if (!File.Exists(basePath + filePath)) return "";
        StreamReader s = new StreamReader(basePath + filePath , Encoding.Unicode);
        string data = s.ReadToEnd();
        s.Close();
        return data;
    }

    public static void writeFileData(string dir, string file, string data)
    {
        if (!isInit) init();

        Directory.CreateDirectory(basePath+dir);

        if (!File.Exists(basePath + dir+"/"+file))
        {
            File.Create(basePath + dir+"/"+file);
        }

        StreamWriter s = new StreamWriter(basePath + dir + "/" + file, false, Encoding.Unicode);
        s.Write(data);
        s.Close();
    }

    public static string getGameConfig(string gameID)
    {
        if (!isInit) init();
        return getGameAssetAsText(gameID,"config.json");
    }

    public static string getGameAssetAsText(string gameID, string assetPath)
    {
        if (!isInit) init();
        return getFileData("jeux/" + gameID + "/" + assetPath);
    }
        
    public static IEnumerator loadGameTexture(string gameID, string assetPath, string texID, ITextureReceiver receiver)
    {
        if (!isInit) init();
        string path = basePath + "jeux/" + gameID + "/" + assetPath;
        WWW www = null;
        if (File.Exists(path)) www = new WWW("file:///" + path);

        yield return www;

        if (www != null) receiver.textureReady(texID, www.texture);
        else receiver.textureReady(texID, instance.noTex); 
       
    }

    public static IEnumerator loadTexture(string assetPath, string texID, ITextureReceiver receiver)
    {
        if (!isInit) init();
        string path = basePath + assetPath;
        WWW www = null;
        if (File.Exists(path)) www = new WWW("file:///" + path);

        yield return www;

        if (www != null) receiver.textureReady(texID, www.texture);
        else receiver.textureReady(texID, instance.noTex);

    }


    public static string getGameMediaFile(string gameID, string mediaFile)
    {
        if (!isInit) init();
        string path = basePath + "jeux/" + gameID + "/" + mediaFile;
        if (!File.Exists(path))
        {
            //Debug.LogWarning("File does not exist : " + path);
            return "";
        }
        return "file:///" + path;
    }

    public static string getMediaPath(string mediaFile)
    {
        if (!isInit) init();
        if (!File.Exists(basePath + "medias/" + mediaFile)) return "";
        return "file:///" + basePath + "medias/" + mediaFile;
    }

    public static string getFolderPath(string path)
    {
        if (!isInit) init();
        return basePath + path;
    }
}
