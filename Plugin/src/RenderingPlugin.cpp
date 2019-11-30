// Example low level rendering Unity plugin

#include "PlatformBase.h"
#include "RenderAPI.h"

#include <assert.h>
#include <math.h>
#include <vector>

// --------------------------------------------------------------------------
// UnitySetInterfaces

static void UNITY_INTERFACE_API OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType);

static IUnityInterfaces *s_UnityInterfaces = NULL;
static IUnityGraphics *s_Graphics = NULL;

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces *unityInterfaces) {
  s_UnityInterfaces = unityInterfaces;
  s_Graphics = s_UnityInterfaces->Get<IUnityGraphics>();
  s_Graphics->RegisterDeviceEventCallback(OnGraphicsDeviceEvent);

#if SUPPORT_VULKAN
  if (s_Graphics->GetRenderer() == kUnityGfxRendererNull) {
    extern void RenderAPI_Vulkan_OnPluginLoad(IUnityInterfaces *);
    RenderAPI_Vulkan_OnPluginLoad(unityInterfaces);
  }
#endif // SUPPORT_VULKAN

  // Run OnGraphicsDeviceEvent(initialize) manually on plugin load
  OnGraphicsDeviceEvent(kUnityGfxDeviceEventInitialize);
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload() {
  s_Graphics->UnregisterDeviceEventCallback(OnGraphicsDeviceEvent);
}

#if UNITY_WEBGL
typedef void(UNITY_INTERFACE_API *PluginLoadFunc)(IUnityInterfaces *unityInterfaces);
typedef void(UNITY_INTERFACE_API *PluginUnloadFunc)();

extern "C" void UnityRegisterRenderingPlugin(PluginLoadFunc loadPlugin, PluginUnloadFunc unloadPlugin);

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API RegisterPlugin() {
  UnityRegisterRenderingPlugin(UnityPluginLoad, UnityPluginUnload);
}
#endif

static RenderAPI *s_CurrentAPI = NULL;
static UnityGfxRenderer s_DeviceType = kUnityGfxRendererNull;

static void UNITY_INTERFACE_API OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType) {
  if (eventType == kUnityGfxDeviceEventInitialize) {
    assert(s_CurrentAPI == NULL);
    s_DeviceType = s_Graphics->GetRenderer();
    s_CurrentAPI = CreateRenderAPI(s_DeviceType);
  }

  if (s_CurrentAPI) {
    s_CurrentAPI->ProcessDeviceEvent(eventType, s_UnityInterfaces);
  }

  if (eventType == kUnityGfxDeviceEventShutdown) {
    delete s_CurrentAPI;
    s_CurrentAPI = NULL;
    s_DeviceType = kUnityGfxRendererNull;
  }
}

enum class RenderingEventId {
  NewFrame = 0,
  Render = 1,
  Init = 2,
  Shutdown = 3,
};


void NullFunction(int eventID) {
}

static void UNITY_INTERFACE_API OnRenderEvent(int eventID) {
  // Unknown / unsupported graphics device type? Do nothing
  if (s_CurrentAPI == NULL) {
    return;
  }

  switch ((RenderingEventId)eventID) {
  case RenderingEventId::Render:
    s_CurrentAPI->ImGuiDraw();
    break;
  case RenderingEventId::NewFrame:
    s_CurrentAPI->ImGuiNewFrame();
    break;
  case RenderingEventId::Init:
    s_CurrentAPI->ImGuiInit();
    break;
  case RenderingEventId::Shutdown:
    s_CurrentAPI->ImGuiShutdown();
    break;
  default:
    NullFunction(eventID);
    break;
  }
}

// --------------------------------------------------------------------------
// GetRenderEventFunc, an example function we export which is used to get a rendering event callback function.

extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetRenderEventFunc() {
  return OnRenderEvent;
}
