﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Bridge
{
    [StructLayout(LayoutKind.Sequential,CharSet = CharSet.Unicode)]
    struct csObject
    {
        public int key;
        public int value;
        public float fvalue;
    }
#if UNITY_IOS    
    private const string dllName = "__Internal";
#else
    private const string dllName = "cppLibs";
#endif
    public delegate void CSUpdateCallback(int tick);
    public delegate void Log(string content);
    public delegate void LogWarning(string content);
    public delegate void LogError(string content);
    [DllImport(dllName)]
    extern static void InitCppEngine();
    [DllImport(dllName)]
    extern static void HandleSetInt(int key, int val);
    [DllImport(dllName)]
    extern static void HandleSetObject(int key, csObject val);
    [DllImport(dllName)]
    extern static void Update(int time_diff);
    [DllImport(dllName)]
    extern static void RegisterLog(Log callback);
    [DllImport(dllName)]
    extern static void RegisterLogWarning(LogWarning callback);
    [DllImport(dllName)]
    extern static void RegisterLogError(LogError callback);
    [DllImport(dllName)]
    extern static void DestroyCPP();

    private static int mainThreadId;
    public static void Init()
    {
        mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        UnityEngine.Profiling.Profiler.BeginSample("CppEngine Initilize");
        RegisterLog(CSLog);
        RegisterLogWarning(CSLogWarning);
        RegisterLogError(CSLogError);
        InitCppEngine();
        HandleSetInt(0, 666);
        HandleSetObject(0, new csObject() { fvalue = 0.58f, key = 666, value= 1246});
        UnityEngine.Profiling.Profiler.EndSample();
    }
    public static void Destroy()
    {
#if UNITY_EDITOR
        if (mainThreadId != System.Threading.Thread.CurrentThread.ManagedThreadId)
        {
            Debug.LogError("Destroy Is Running not in main thread");
            return;
        }
#endif
        DestroyCPP();
        //System.Runtime.InteropServices.
    }
    public static void CSUpdate(int time_diff)
    {
#if UNITY_EDITOR
        if (mainThreadId != System.Threading.Thread.CurrentThread.ManagedThreadId)
        {
            Debug.LogError("CSUpdate Is Running not in main thread");
            return;
        }
#endif
        UnityEngine.Profiling.Profiler.BeginSample("CppUpdate");
        //cost 4 ms (average) ms on my book
        Update(time_diff);
        UnityEngine.Profiling.Profiler.EndSample();
       /* UnityEngine.Profiling.Profiler.BeginSample("CSUpdate");
        //cost 95 ms (average) on my book
        long ret = 0;
        for (int i = 0; i < 10000000; i++)
        {
            ret += i;
        }
        CSLog("ret is " + ret);
        UnityEngine.Profiling.Profiler.EndSample();*/
    }
    private static void CSLog(string content)
    {
        UnityEngine.Debug.Log(content);
    }
    private static void CSLogWarning(string content)
    {
        UnityEngine.Debug.LogWarning(content);
    }
    private static void CSLogError(string content)
    {
        UnityEngine.Debug.LogError(content);
    }
}
