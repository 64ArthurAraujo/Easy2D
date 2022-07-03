using Silk.NET.OpenGLES;
using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Easy2D.OpenGL;

// gl controller may not be the best name but works for now
public static class GLController
{
    public static GL Instance { get; private set; }

    public static string Vendor { get; private set; }
    public static string Renderer { get; private set; }
    public static string ShadingVersion { get; private set; }
    public static string Version { get; private set; }
    
    public static string GLExtensions  { get; private set; }

    public static ulong DrawCalls { get; private set; }

    public static int MaxTextureSlots { get; private set; } = -1;

    public unsafe static void SetGL(GL gl)
    {
        Instance = gl;

        Vendor = Marshal.PtrToStringAnsi(new IntPtr(Instance.GetString(StringName.Vendor)));
        Renderer = Marshal.PtrToStringAnsi(new IntPtr(Instance.GetString(StringName.Renderer)));
        ShadingVersion = Marshal.PtrToStringAnsi(new IntPtr(Instance.GetString(StringName.ShadingLanguageVersion)));
        Version = Marshal.PtrToStringAnsi(new IntPtr(Instance.GetString(StringName.Version)));
        GLExtensions = Marshal.PtrToStringAnsi(new IntPtr(Instance.GetString(StringName.Extensions)));

        Utils.Log($"Vendor: {Vendor}", LogLevel.Important);
        Utils.Log($"Renderer: {Renderer}", LogLevel.Important);
        Utils.Log($"Version: {Version}", LogLevel.Important);
        Utils.Log($"GLSL Version: {ShadingVersion}", LogLevel.Important);

        Instance.GetInteger(GetPName.MaxTextureImageUnits, out int slotCount);
        MaxTextureSlots = slotCount;

#if DEBUG
        Instance.Enable(GLEnum.DebugOutput);
        Instance.Enable(GLEnum.DebugOutputSynchronous);
        Instance.DebugMessageCallback(OnDebug, null);
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void DrawElements(PrimitiveType mode, uint count, DrawElementsType elementsType, void* indices = null)
    {
        Instance.DrawElements(mode, count, elementsType, indices);
        DrawCalls++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void DrawArrays(PrimitiveType mode, int first, uint count)
    {
        Instance.DrawArrays(mode, first, count);
        DrawCalls++;
    }

    public static void ResetStatistics()
    {
        DrawCalls = 0;
    }

    private static void OnDebug(GLEnum source, GLEnum type, int id, GLEnum severity, int length, nint message, nint userparam)
    {
        LogLevel level = (DebugSeverity)severity == DebugSeverity.DebugSeverityNotification ? LogLevel.Debug : LogLevel.Warning;
        Utils.Log
        (
            $"[{severity.ToString().Substring(13)}] {type.ToString().Substring(9)}/{id}: {System.Runtime.InteropServices.Marshal.PtrToStringAnsi(message)}"
        , level);
    }
}
