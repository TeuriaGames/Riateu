using System;
using System.Runtime.InteropServices;
using ImGuiNET;

namespace Riateu.ImGuiRend;

public static class DockNative 
{
    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    public static extern uint igDockBuilderDockWindow([MarshalAs(UnmanagedType.LPUTF8Str)] string window_name, uint node_id);

    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    public static extern uint igDockBuilderAddNode(uint node_id = 0, ImGuiDockNodeFlags flags = ImGuiDockNodeFlags.None);

    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    public static extern uint igDockBuilderRemoveNode(uint node_id);

    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr igDockBuilderGetNode(uint node_id = 0);

    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    public static extern uint igDockBuilderSetNodePos(uint node_id, System.Numerics.Vector2 pos);

    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    public static extern uint igDockBuilderSetNodeSize(uint node_id, System.Numerics.Vector2 size);

    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    public static extern uint igDockBuilderSplitNode(uint node_id, ImGuiNET.ImGuiDir split_dir, float size_ratio_for_node_at_dir, out uint out_id_at_dir, out uint out_id_at_opposite_dir);

    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe uint igDockBuilderCopyDockSpace(uint src_dockspace_id, uint dst_dockspace_id, IntPtr in_window_remap_pairs);

    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    public static extern uint igDockBuilderCopyNode(uint src_node_id, uint dst_node_id, out ImVector<uint> out_node_remap_pairs);

    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe uint igDockBuilderCopyWindowSettings(byte *src_name, byte *dst_name);

    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    public static extern uint igDockBuilderFinish(uint node_id);
}