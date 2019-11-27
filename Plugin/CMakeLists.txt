cmake_minimum_required(VERSION 3.12)
project(ImGuiUnity)

if (NOT UNITY_DIR)
  message( FATAL_ERROR "Please specify -DUNITY_DIR='unity installation dir' in cmake args." )
endif()

set(BIN_OUT_DEBUG ${CMAKE_BINARY_DIR}/BinDebug)
set(BIN_OUT_RELEASE ${CMAKE_SOURCE_DIR}/../ImGuiUnityProject/Assets/ImGui/Plugins)

set(MYROOTDIR ${CMAKE_CURRENT_SOURCE_DIR})
add_subdirectory(external)

add_library(ImGuiUnity SHARED
  src/plugin.cpp
)

target_include_directories(ImGuiUnity PUBLIC include)
target_link_libraries(ImGuiUnity PUBLIC imgui)

add_subdirectory(csharp)

function (ConfigOutput target_name) 
  set_target_properties(${target_name} PROPERTIES
                        RUNTIME_OUTPUT_DIRECTORY_DEBUG ${BIN_OUT_DEBUG}
                        RUNTIME_OUTPUT_DIRECTORY_RELEASE ${BIN_OUT_RELEASE})
  set_target_properties(${target_name} PROPERTIES
                        LIBRARY_OUTPUT_DIRECTORY_DEBUG ${BIN_OUT_DEBUG}
                        LIBRARY_OUTPUT_DIRECTORY_RELEASE ${BIN_OUT_RELEASE})
endfunction()

ConfigOutput(imgui) 
ConfigOutput(ImGuiUnity) 
ConfigOutput(cimgui)
ConfigOutput(ImGui.NET)
ConfigOutput(ImGuiUnityIL)
