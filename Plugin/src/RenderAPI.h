#pragma once

#include "Unity/IUnityGraphics.h"

#include <stddef.h>

struct IUnityInterfaces;
class RenderAPI {
public:
  virtual ~RenderAPI() {
  }

  virtual void ProcessDeviceEvent(UnityGfxDeviceEventType type, IUnityInterfaces *interfaces) = 0;

  virtual bool GetUsesReverseZ() = 0;

  virtual void ImGuiInit() = 0;
  virtual void ImGuiShutdown() = 0;
  virtual void ImGuiDraw() = 0;
  virtual void ImGuiNewFrame() = 0;
};

// Create a graphics API implementation instance for the given API type.
RenderAPI *CreateRenderAPI(UnityGfxRenderer apiType);
