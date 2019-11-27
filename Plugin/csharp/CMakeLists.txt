cmake_minimum_required(VERSION 3.8)
project("ImGui.NET" CSharp)

set(IMGUI_NET_DIR "${CMAKE_CURRENT_SOURCE_DIR}/../external/ImGui.NET/src/ImGui.NET")


add_library(ImGui.NET SHARED
  ${IMGUI_NET_DIR}/ImDrawData.Manual.cs    
  ${IMGUI_NET_DIR}/ImDrawList.Manual.cs    
  ${IMGUI_NET_DIR}/ImGui.Manual.cs
  ${IMGUI_NET_DIR}/ImGuiSizeCallback.cs    
  ${IMGUI_NET_DIR}/ImGuiTextEditCallback.cs
  ${IMGUI_NET_DIR}/ImVector.cs
  ${IMGUI_NET_DIR}/NullTerminatedString.cs
  ${IMGUI_NET_DIR}/Pair.cs
  ${IMGUI_NET_DIR}/RangeAccessor.cs
  ${IMGUI_NET_DIR}/Util.cs

  ${IMGUI_NET_DIR}/Generated/ImColor.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImDrawChannel.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImDrawCmd.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImDrawCornerFlags.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImDrawData.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImDrawList.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImDrawListFlags.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImDrawListSplitter.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImDrawVert.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImFont.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImFontAtlas.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImFontAtlasCustomRect.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImFontAtlasFlags.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImFontConfig.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImFontGlyph.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImFontGlyphRangesBuilder.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImGui.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImGuiBackendFlags.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImGuiCol.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImGuiColorEditFlags.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImGuiComboFlags.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImGuiCond.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImGuiConfigFlags.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImGuiDataType.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImGuiDir.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImGuiDragDropFlags.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImGuiFocusedFlags.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImGuiHoveredFlags.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImGuiInputTextCallbackData.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImGuiInputTextFlags.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImGuiIO.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImGuiKey.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImGuiListClipper.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImGuiMouseCursor.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImGuiNative.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImGuiNavInput.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImGuiOnceUponAFrame.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImGuiPayload.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImGuiSelectableFlags.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImGuiSizeCallbackData.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImGuiStorage.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImGuiStyle.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImGuiStyleVar.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImGuiTabBarFlags.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImGuiTabItemFlags.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImGuiTextBuffer.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImGuiTextFilter.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImGuiTextRange.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImGuiTreeNodeFlags.gen.cs
  ${IMGUI_NET_DIR}/Generated/ImGuiWindowFlags.gen.cs
)

set_property(TARGET ImGui.NET PROPERTY VS_DOTNET_REFERENCES
  "Microsoft.CSharp"
  "System"
  "System.Numerics"
  "System.Numerics.Vectors"
  "System.Core"
)

set(COMPILER_SERVICES_DLL "${CMAKE_CURRENT_SOURCE_DIR}/dll/System.Runtime.CompilerServices.Unsafe.dll")
set_property(TARGET ImGui.NET PROPERTY VS_DOTNET_REFERENCE_System.Runtime.CompilerServices.Unsafe ${COMPILER_SERVICES_DLL})

target_compile_options(ImGui.NET PUBLIC "/unsafe")

add_library(ImGuiUnityIL SHARED 
  UnityScripts/ImGuiRenderer.cs
  UnityScripts/ComputeBufferPinned.cs
)

set_property(TARGET ImGuiUnityIL PROPERTY VS_DOTNET_REFERENCES
  "Microsoft.CSharp"
  "System"
  "System.Collections"
  "System.Runtime.InteropServices"
  "System.Numerics"
  "System.Core"
)
set_property(TARGET ImGuiUnityIL PROPERTY VS_DOTNET_REFERENCE_UnityEngine ${UNITY_DIR}/Editor/Data/Managed/UnityEngine.dll)
set_property(TARGET ImGuiUnityIL PROPERTY VS_DOTNET_REFERENCE_ImGui.NET ${CMAKE_CURRENT_BINARY_DIR}/ImGui.NET.dll)

target_compile_options(ImGuiUnityIL PUBLIC "/unsafe")
set(CMAKE_CSharp_FLAGS "/langversion:7")

add_custom_command(TARGET ImGuiUnityIL POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E remove -f
        UnityEngine.dll
        $<TARGET_FILE_DIR:ImGuiUnityIL>)
add_custom_command(TARGET ImGuiUnityIL POST_BUILD
  COMMAND ${CMAKE_COMMAND} -E remove -f
      UnityEngine.xml
      $<TARGET_FILE_DIR:ImGuiUnityIL>)

set(FRAMEWORK_VER "v4.7.2")
set_property(TARGET ImGui.NET PROPERTY DOTNET_TARGET_FRAMEWORK_VERSION ${FRAMEWORK_VER})
set_property(TARGET ImGuiUnityIL PROPERTY DOTNET_TARGET_FRAMEWORK_VERSION ${FRAMEWORK_VER})
