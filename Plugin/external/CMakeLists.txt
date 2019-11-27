find_program(git_executable NAMES git git.exe git.cmd)
if(NOT git_executable)
    message(FATAL_ERROR "Failed to find git.")
endif()

function(UpdateExternalLib name url rev)
    set(need_checkout FALSE)
    set(external_folder "${MYROOTDIR}/external")
    set(external_lib_folder "${external_folder}/${name}")
    if(EXISTS ${external_lib_folder})
        message(STATUS "Updating ${name} to revision ${rev}...")
        execute_process(COMMAND "${git_executable}" "fetch" "origin" WORKING_DIRECTORY "${external_lib_folder}")
        execute_process(COMMAND "${git_executable}" "rev-parse" "HEAD" WORKING_DIRECTORY "${external_lib_folder}" OUTPUT_VARIABLE head_rev)
        string(STRIP ${head_rev} head_rev)
        if (${head_rev} STREQUAL ${rev})
            set(need_checkout FALSE)
        else()
            set(need_checkout TRUE)
        endif()
    else()
        message(STATUS "Cloning ${name} revision...")
        execute_process(COMMAND "${git_executable}" "clone" ${url} "-n" WORKING_DIRECTORY "${external_folder}")
        set(need_checkout TRUE)
    endif()
    if(need_checkout)
        message(STATUS "Checking out to revision ${rev}...")
        execute_process(COMMAND "${git_executable}" "checkout" "-q" ${rev} WORKING_DIRECTORY "${external_lib_folder}")
    endif()
endfunction()

function(ApplyPatch name patch)
    set(external_folder "${MYROOTDIR}/External")
    set(external_lib_folder "${external_folder}/${name}")

    execute_process(COMMAND "${git_executable}" "apply" "--check" "--ignore-space-change" "${patch}" WORKING_DIRECTORY ${external_lib_folder} RESULT_VARIABLE checkout_err)
    if(NOT checkout_err)
        message(STATUS "Applying ${patch}...")
        execute_process(COMMAND "${git_executable}" "am" "--ignore-space-change" "${patch}" WORKING_DIRECTORY ${external_lib_folder})
    endif()
endfunction()

function(LoadSubmodules name) 
    set(external_folder "${MYROOTDIR}/external")
    set(external_lib_folder "${external_folder}/${name}")
    if(EXISTS ${external_lib_folder})
        execute_process(COMMAND "${git_executable}" "submodule" "update" "--init" WORKING_DIRECTORY "${external_lib_folder}")
    endif()
endfunction()

UpdateExternalLib("cimgui" "https://github.com/cimgui/cimgui.git" "8096fb69fe992804afca97984d33453ea10dd653")
LoadSubmodules("cimgui")

UpdateExternalLib("ImGui.NET" "https://github.com/mellinoe/ImGui.NET.git" "35aabdc9b898cea2f645f9985a40488e9a265c13")
ApplyPatch("ImGui.NET" ${MYROOTDIR}/external/imgui_dot_net.patch)

add_subdirectory(cimgui)
set(IMGUI_DIR "${MYROOTDIR}/external/cimgui/imgui")

add_library(imgui STATIC
  ${IMGUI_DIR}/imconfig.h
  ${IMGUI_DIR}/imgui_draw.cpp
  ${IMGUI_DIR}/imgui_demo.cpp
  ${IMGUI_DIR}/imgui_internal.h
  ${IMGUI_DIR}/imgui_widgets.cpp
  ${IMGUI_DIR}/imgui.cpp
  ${IMGUI_DIR}/imgui.h
  ${IMGUI_DIR}/imstb_rectpack.h
  ${IMGUI_DIR}/imstb_textedit.h
  ${IMGUI_DIR}/imstb_truetype.h
)
target_compile_definitions(imgui PUBLIC "ImDrawIdx=int")
target_compile_definitions(cimgui PUBLIC "ImDrawIdx=int")
