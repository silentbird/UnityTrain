using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BDFramework.Core.Tools;
using UnityEngine;

namespace BDFramework
{
    static public class ScriptLoder
    {
        static readonly public string DLL_PATH = "Hotfix/hotfix.dll";

        /// <summary>
        /// 反射注册
        /// </summary>
        private static Action<bool> CLRBindAction { get; set; }

        /// <summary>
        /// 脚本加载入口
        /// </summary>
        /// <param name="loadPathTypeType"></param>
        /// <param name="runMode"></param>
        /// <param name="mainProjectTypes">UPM隔离了dll,需要手动传入</param>
        static public void Load(AssetLoadPathType loadPathTypeType,
            HotfixCodeRunMode runMode,
            Type[] mainProjectTypes,
            Action<bool> clrBindingAction)
        {
            CLRBindAction = clrBindingAction;

            if (loadPathTypeType == AssetLoadPathType.Editor)
            {
                BDebug.Log("【ScriptLaunch】Editor(非热更)模式!");
                //反射调用，防止编译报错
                var assembly = Assembly.GetExecutingAssembly();
                var type = assembly.GetType("BDLauncherBridge");
                var method = type.GetMethod("Start", BindingFlags.Public | BindingFlags.Static);
                //添加框架部分的type，热更下不需要，打包会把框架的部分打进去
                var list = new List<Type>();
                list.AddRange(mainProjectTypes);
                list.AddRange(typeof(BDLauncher).Assembly.GetTypes());
                method.Invoke(null, new object[] {list.ToArray(),null});
            }
            else
            {
                BDebug.Log("【ScriptLaunch】热更模式!");
                var path = GameConfig.GetLoadPath(loadPathTypeType);
                path = Path.Combine(path, BDApplication.GetPlatformPath(Application.platform));
                //加载dll
                var dllPath = Path.Combine(path, DLL_PATH);
                LoadHotfixDLL(dllPath, runMode, mainProjectTypes);
            }
        }


        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="source"></param>
        /// <param name="copyto"></param>
        /// <returns></returns>
        static void LoadHotfixDLL(string dllPath, HotfixCodeRunMode mode, Type[] mainProjecTypes)
        {
            //反射执行
            if (mode == HotfixCodeRunMode.ByReflection)
            {
                BDebug.Log("【ScriptLaunch】反射Dll路径:" + dllPath, "red");
                Assembly Assembly;
                var dllBytes = File.ReadAllBytes(dllPath);
                var pdbPath = dllPath + ".pdb";
                if (File.Exists(pdbPath))
                {
                    var pdbBytes = File.ReadAllBytes(pdbPath);
                    Assembly = Assembly.Load(dllBytes, pdbBytes);
                }
                else
                {
                    Assembly = Assembly.Load(dllBytes);
                }

                BDebug.Log("【ScriptLaunch】反射加载成功,开始执行Start");
                var type = typeof(ScriptLoder).Assembly.GetType("BDLauncherBridge");
                var method = type.GetMethod("Start", BindingFlags.Public | BindingFlags.Static);

                method.Invoke(null, new object[] {mainProjecTypes, Assembly.GetTypes()});
            }
            //解释执行
            else if (mode == HotfixCodeRunMode.ByILRuntime)
            {
                BDebug.Log("【ScriptLaunch】热更Dll路径:" + dllPath, "red");
                //解释执行模式
                ILRuntimeHelper.LoadHotfix(dllPath, CLRBindAction);
                var hotfixTypes = ILRuntimeHelper.GetHotfixTypes().ToArray();
                ILRuntimeHelper.AppDomain.Invoke("BDLauncherBridge", "Start", null,
                    new object[] {mainProjecTypes, hotfixTypes});
            }
            else
            {
                BDebug.Log("【ScriptLaunch】Dll路径:内置", "red");
            }
        }
    }
}