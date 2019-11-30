#include "Unity/IUnityInterface.h"
#include "imgui.h"

typedef struct IOState {
  float Time;
  int KeyShift;
  int KeyCtrl;
  int KeyAlt;
  int KeySuper;
  float DisplaySizeX;
  float DisplaySizeY;
  float DisplayFramebufferSizeX;
  float DisplayFramebufferSizeY;
  float MousePosX;
  float MousePosY;
  int MouseDown0;
  int MouseDown1;
  int MouseDown2;
  float MouseWheel;
} _IOState;

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UploadKeyMap(int *keyMap, int length) {
  auto &io = ImGui::GetIO();
  int minLen = length > 22 ? 22 : length;
  for (int x = 0; x < minLen; x++) {
    io.KeyMap[x] = keyMap[x];
  }
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UploadKeyDownStates(int *keyDown, int length) {
  auto &io = ImGui::GetIO();
  int minLen = length > 512 ? 512 : length;
  for (int x = 0; x < minLen; x++) {
    io.KeysDown[x] = keyDown[x];
  }
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UpdateImGuiIO(IOState *statePtr) {
  auto &io = ImGui::GetIO();
  auto &state = *statePtr;

  io.DeltaTime = state.Time;

  io.KeyAlt = state.KeyAlt;
  io.KeyCtrl = state.KeyCtrl;
  io.KeyShift = state.KeyShift;
  io.KeySuper = state.KeySuper;

  io.DisplaySize.x = state.DisplaySizeX;
  io.DisplaySize.y = state.DisplaySizeY;

  io.DisplayFramebufferScale.x = state.DisplayFramebufferSizeX;
  io.DisplayFramebufferScale.y = state.DisplayFramebufferSizeY;

  io.MousePos = {state.MousePosX, state.MousePosY};

  io.MouseDown[0] = state.MouseDown0;
  io.MouseDown[1] = state.MouseDown1;
  io.MouseDown[2] = state.MouseDown2;

  io.MouseWheel = state.MouseWheel;
}
