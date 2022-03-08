using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class EditorToolMenu
{
    [MenuItem("Tools/Hotfix/Clear Addressable Cache")]
    public static void ClearAllCache()
    {
        DeleteAllFileByDic(Application.persistentDataPath);
        var path = @"C:\Users\abcd\AppData\LocalLow\Unity\ly_Test2";
        DirectoryInfo[] dirs = new DirectoryInfo(path).GetDirectories();
        for (int i = 0; i < dirs.Length; i++)
        {
            DeleteAllFileByDic(dirs[i].FullName);
        }
    }

    public static bool DeleteAllFileByDic(string fullPath)
    {
        //获取指定路径下面的所有资源文件  然后进行删除
        if (Directory.Exists(fullPath))
        {
            DirectoryInfo direction = new DirectoryInfo(fullPath);
            direction.Delete(true);
            Debug.Log("删除" + direction.FullName + "成功");
            return true;
        }

        Debug.Log("删除" + fullPath + "失败");
        return false;
    }

    [MenuItem("Tools/Hotfix/编译 Hotfix.dll")]
    public static void BuildDll()
    {
        ScriptBuildTools.BuildDll(Application.streamingAssetsPath,Application.platform, ScriptBuildTools.BuildMode.Debug, true);
    }
}