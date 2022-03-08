﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using BDFramework.Core.Tools;
using BDFramework.ResourceMgr.V2;
using LitJson;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;


/// <summary>
/// AssetBundle测试
/// </summary>
public class AssetBundleBenchmark01 : MonoBehaviour
{
    /// <summary>
    /// 资源分组
    /// </summary>
    public Dictionary<string, List<string>> AssetGroup = new Dictionary<string, List<string>>();


    static private Transform        UI_ROOT;
    static private Transform        SCENE_ROOT;
    // static private DevResourceMgr   DevLoder;
    static private AssetBundleMgrV2 AssetBundleLoader;

    static private Camera Camera;

    // static private EditorWindow     GameView;
    private static string BenchmarkResultPath;
    //
    static private Image          imageNode;
    static private SpriteRenderer spriteRendererNode;

    private void Start()
    {
        BenchmarkResultPath = Application.persistentDataPath + "/Benchmark/AssetBundleTest01.json";
        this.Init();
        this.StartCoroutine(IE_01_LoadAll());
    }


    /// <summary>
    /// 初始化
    /// </summary>
    private void Init()
    {
        //初始化加载环境
        UnityEngine.AssetBundle.UnloadAllAssetBundles(true);
        //dev加载器
        // DevLoder = new DevResourceMgr();
        // DevLoder.Init("");
        AssetBundleLoader = new AssetBundleMgrV2();
        var abPath = Application.isEditor ? BDApplication.DevOpsPublishAssetsPath : Application.persistentDataPath;
        AssetBundleLoader.Init(abPath);
        AssetBundleLoader.WarmUpShaders();
        //节点
        UI_ROOT            = GameObject.Find("UIRoot").transform;
        SCENE_ROOT         = GameObject.Find("3dRoot").transform;
        imageNode          = UI_ROOT.transform.GetComponentInChildren<Image>();
        spriteRendererNode = SCENE_ROOT.transform.GetComponentInChildren<SpriteRenderer>();
        imageNode.gameObject.SetActive(false);
        spriteRendererNode.gameObject.SetActive(false);
        //相机
        Camera                      = GameObject.Find("Camera").GetComponent<Camera>();
        Camera.cullingMask          = -1;
        Camera.gameObject.hideFlags = HideFlags.DontSave;
        //获取gameview
        //  var assembly = typeof(UnityEditor.EditorWindow).Assembly;
        // System.Type GameViewType = assembly.GetType("UnityEditor.GameView");
    }

    /// <summary>
    /// 加载消耗数据
    /// </summary>
    public class LoadTimeData
    {
        public string LoadPath;

        /// <summary>
        /// 加载时长
        /// </summary>
        public float LoadTime;

        /// <summary>
        /// 初始化时长
        /// </summary>
        public float InstanceTime;
    }

    /// <summary>
    /// 加载数据
    /// </summary>
    private static Dictionary<string, List<LoadTimeData>> loadDataMap = new Dictionary<string, List<LoadTimeData>>();

    /// <summary>
    /// 加载所有assetbundle
    /// </summary>
    /// <returns></returns>
    static IEnumerator IE_01_LoadAll()
    {
        var outpath = BDApplication.BDEditorCachePath + "/AssetBundle";
        if (!Directory.Exists(outpath))
        {
            Directory.CreateDirectory(outpath);
        }

        loadDataMap.Clear();
        //加载
        foreach (var assetdata in AssetBundleLoader.AssetConfigLoder.AssetbundleItemList)
        {
            if (string.IsNullOrEmpty(assetdata.LoadPath))
            {
                continue;
            }

            var typeName    = AssetBundleLoader.AssetConfigLoder.AssetTypes.AssetTypeList[assetdata.AssetType];
            var runtimePath = assetdata.LoadPath;
            //加载
            //Debug.Log("【LoadTest】:" + runtimePath);
            if (!loadDataMap.ContainsKey(typeName))
            {
                loadDataMap[typeName] = new List<LoadTimeData>();
            }

            var loadList = loadDataMap[typeName];
            //
            var loadData = new LoadTimeData();
            loadData.LoadPath = runtimePath;
            loadList.Add(loadData);
            //计时器
            Stopwatch sw = new Stopwatch();
            if (typeName == typeof(GameObject).FullName)
            {
                //加载
                sw.Start();
                var obj = AssetBundleLoader.Load<GameObject>(runtimePath);
                sw.Stop();
                loadData.LoadTime = sw.ElapsedTicks;
                //实例化
                if (obj != null)
                {
                    sw.Restart();
                    var gobj = GameObject.Instantiate(obj);
                    sw.Stop();
                    loadData.InstanceTime = sw.ElapsedTicks;
                    //UI
                    var rectTransform = gobj.GetComponentInChildren<RectTransform>();
                    if (rectTransform != null)
                    {
                        gobj.transform.SetParent(UI_ROOT, false);
                    }
                    else
                    {
                        gobj.transform.SetParent(SCENE_ROOT);
                    }

                    //抓屏 保存
                    var outpng = string.Format("{0}/{1}_ab.png", outpath, runtimePath.Replace("/", "_"));
                    yield return null;
                    //渲染
                    // GameView.Repaint();
                    // GameView.Focus();

                    yield return null;
                    //抓屏 
                    //TODO 这里有时候能抓到 有时候抓不到

                    ScreenCapture.CaptureScreenshot(outpng);
                    //删除
                    GameObject.DestroyImmediate(gobj);
                }
                else
                {
                    UnityEngine.Debug.LogError("【Prefab】加载失败:" + runtimePath);
                }
            }
            else if (typeName == typeof(TextAsset).FullName)
            {
                //测试打印AssetText资源
                sw.Start();
                var textAsset = AssetBundleLoader.Load<TextAsset>(runtimePath);
                sw.Stop();
                loadData.LoadTime = sw.ElapsedTicks;
                if (!textAsset)
                {
                    UnityEngine.Debug.LogError("【TextAsset】加载失败:" + runtimePath);
                }
                else
                {
                    UnityEngine.Debug.Log(textAsset.text);
                }
            }
            else if (typeName == typeof(Texture).FullName)
            {
                sw.Start();
                var tex = AssetBundleLoader.Load<Texture>(runtimePath);
                sw.Stop();
                loadData.LoadTime = sw.ElapsedTicks;
                if (!tex)
                {
                    UnityEngine.Debug.LogError("【Texture】加载失败:" + runtimePath);
                }

                break;
            }
            else if (typeName == typeof(Texture2D).FullName)
            {
                sw.Start();
                var tex = AssetBundleLoader.Load<Texture2D>(runtimePath);
                sw.Stop();
                loadData.LoadTime = sw.ElapsedTicks;
                if (!tex)
                {
                    UnityEngine.Debug.LogError("【Texture2D】加载失败:" + runtimePath);
                }
                else
                {
                    spriteRendererNode.gameObject.SetActive(true);
                    spriteRendererNode.sprite = Sprite.Create(tex, new Rect(Vector2.zero, tex.texelSize), new Vector2(0.5f, 0.5f), 128);
                    yield return null;
                    spriteRendererNode.gameObject.SetActive(false);
                }
            }
            else if (typeName == typeof(Sprite).FullName)
            {
                sw.Start();
                var sp = AssetBundleLoader.Load<Sprite>(runtimePath);
                sw.Stop();
                loadData.LoadTime = sw.ElapsedTicks;
                if (!sp)
                {
                    UnityEngine.Debug.LogError("【Sprite】加载失败:" + runtimePath);
                }
                else
                {
                    imageNode.gameObject.SetActive(true);
                    imageNode.overrideSprite = sp;
                    imageNode.SetNativeSize();
                    yield return null;
                    imageNode.gameObject.SetActive(false);
                }
            }
            else if (typeName == typeof(Material).FullName)
            {
                sw.Start();
                var mat = AssetBundleLoader.Load<Material>(runtimePath);
                sw.Stop();
                loadData.LoadTime = sw.ElapsedTicks;
                if (!mat)
                {
                    UnityEngine.Debug.LogError("【Material】加载失败:" + runtimePath);
                }
            }
            else if (typeName == typeof(Shader).FullName)
            {
                sw.Start();
                var shader = AssetBundleLoader.Load<Shader>(runtimePath);
                sw.Stop();
                loadData.LoadTime = sw.ElapsedTicks;
                if (!shader)
                {
                    UnityEngine.Debug.LogError("【Shader】加载失败:" + runtimePath);
                }
            }
            else if (typeName == typeof(AudioClip).FullName)
            {
                sw.Start();
                var ac = AssetBundleLoader.Load<AudioClip>(runtimePath);
                sw.Stop();
                loadData.LoadTime = sw.ElapsedTicks;
                if (!ac)
                {
                    UnityEngine.Debug.LogError("【AudioClip】加载失败:" + runtimePath);
                }
            }
            else if (typeName == typeof(AnimationClip).FullName)
            {
                sw.Start();
                var anic = AssetBundleLoader.Load<AnimationClip>(runtimePath);
                sw.Stop();
                loadData.LoadTime = sw.ElapsedTicks;
                if (!anic)
                {
                    UnityEngine.Debug.LogError("【AnimationClip】加载失败:" + runtimePath);
                }
            }
            else if (typeName == typeof(Mesh).FullName)
            {
                sw.Start();
                var mesh = AssetBundleLoader.Load<Mesh>(runtimePath);
                sw.Stop();
                loadData.LoadTime = sw.ElapsedTicks;
                if (!mesh)
                {
                    UnityEngine.Debug.LogError("【Mesh】加载失败:" + runtimePath);
                }
            }
            else if (typeName == typeof(Font).FullName)
            {
                sw.Start();

                var font = AssetBundleLoader.Load<Font>(runtimePath);

                sw.Stop();
                loadData.LoadTime = sw.ElapsedTicks;
                if (!font)
                {
                    UnityEngine.Debug.LogError("【Font】加载失败:" + runtimePath);
                }
            }
            else if (typeName == typeof(SpriteAtlas).FullName)
            {
                sw.Start();
                var sa = AssetBundleLoader.Load<SpriteAtlas>(runtimePath);
                sw.Stop();
                if (!sa)
                {
                    UnityEngine.Debug.LogError("【SpriteAtlas】加载失败:" + runtimePath);
                }

                loadData.LoadTime = sw.ElapsedTicks;
            }
            else if (typeName == typeof(ShaderVariantCollection).FullName)
            {
                sw.Start();
                var svc = AssetBundleLoader.Load<ShaderVariantCollection>(runtimePath);
                svc?.WarmUp();
                sw.Stop();
                if (!svc)
                {
                    UnityEngine.Debug.LogError("【ShaderVariantCollection】加载失败:" + runtimePath);
                }

                loadData.LoadTime = sw.ElapsedTicks;
            }
            // else if (typeName == typeof(anima).FullName)
            // {
            //     sw.Start();
            //     var aniCtrl = AssetBundleLoader.Load<AnimatorController>(runtimePath);
            //     sw.Stop();
            //     if (!aniCtrl)
            //     {
            //         UnityEngine.Debug.LogError("【AnimatorController】加载失败:" + runtimePath);
            //     }
            //
            //     loadData.LoadTime = sw.ElapsedTicks;
            // }
            // else if (typeName == typeof(timeline).FullName)
            // {
            //     sw.Start();
            //     var aniCtrl = AssetBundleLoader.Load<AnimatorController>(runtimePath);
            //     sw.Stop();
            //     if (!aniCtrl)
            //     {
            //         UnityEngine.Debug.LogError("【AnimatorController】加载失败:" + runtimePath);
            //     }
            //
            //     loadData.LoadTime = sw.ElapsedTicks;
            // }
            else
            {
                sw.Start();
                var gobj = AssetBundleLoader.Load<Object>(runtimePath);
                sw.Stop();
                if (!gobj)
                {
                    UnityEngine.Debug.LogError("【Object】加载失败:" + runtimePath);
                }

                UnityEngine.Debug.LogError("待编写测试! -" + typeName);
            }

            //打印

            Debug.LogFormat("<color=yellow>{0}</color> <color=green>【加载】:<color=yellow>{1}ms</color>;【初始化】:<color=yellow>{2}ms</color> </color>", loadData.LoadPath, loadData.LoadTime / 10000f, loadData.InstanceTime / 10000f);
            yield return null;
        }

        yield return null;

        // foreach (var item in loadDataMap)
        // {
        //     Debug.Log("<color=red>【" + item.Key + "】</color>");
        //     foreach (var ld in item.Value)
        //     {
        //         Debug.LogFormat("<color=yellow>{0}</color> <color=green>【加载】:<color=yellow>{1}ms</color>;【初始化】:<color=yellow>{2}ms</color> </color>", ld.LoadPath, ld.LoadTime / 10000f, ld.InstanceTime / 10000f);
        //     }
        // }

        //
        var content = JsonMapper.ToJson(loadDataMap);
        FileHelper.WriteAllText(BenchmarkResultPath, content);

        yield return null;

// #if UNITY_EDITOR
//         EditorUtility.RevealInFinder(outpath);
// #endif
    }
}