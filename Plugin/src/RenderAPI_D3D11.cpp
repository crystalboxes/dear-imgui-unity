#include "RenderAPI.h"
#include "PlatformBase.h"

// Direct3D 11 implementation of RenderAPI.

#if SUPPORT_D3D11

#include <assert.h>
#include <d3d11.h>
#include "Unity/IUnityGraphicsD3D11.h"

#include "imgui_impl_dx11.h"
#include "imgui.h"

class RenderAPI_D3D11 : public RenderAPI {
public:
  RenderAPI_D3D11();
  virtual ~RenderAPI_D3D11() {
  }

  virtual void ProcessDeviceEvent(UnityGfxDeviceEventType type, IUnityInterfaces *interfaces);

  virtual bool GetUsesReverseZ() {
    return (int)m_Device->GetFeatureLevel() >= (int)D3D_FEATURE_LEVEL_10_0;
  }

  virtual void ImGuiInit() override;
  virtual void ImGuiShutdown() override;
  virtual void ImGuiNewFrame() override;
  virtual void ImGuiDraw() override;

private:
  void CreateResources();
  void ReleaseResources();

private:
  ID3D11Device *m_Device;
};

RenderAPI *CreateRenderAPI_D3D11() {
  return new RenderAPI_D3D11();
}

RenderAPI_D3D11::RenderAPI_D3D11()
    : m_Device(NULL) {
}

void RenderAPI_D3D11::ProcessDeviceEvent(UnityGfxDeviceEventType type, IUnityInterfaces *interfaces) {
  switch (type) {
  case kUnityGfxDeviceEventInitialize: {
    IUnityGraphicsD3D11 *d3d = interfaces->Get<IUnityGraphicsD3D11>();
    m_Device = d3d->GetDevice();
    CreateResources();
    break;
  }
  case kUnityGfxDeviceEventShutdown:
    ReleaseResources();
    break;
  }
}

void RenderAPI_D3D11::CreateResources() {
}

void RenderAPI_D3D11::ReleaseResources() {
}

void RenderAPI_D3D11::ImGuiInit() {
  ImGui_ImplDX11_Init(m_Device);
}

void RenderAPI_D3D11::ImGuiDraw() {
  ImGui_ImplDX11_RenderDrawData(ImGui::GetDrawData());
}

void RenderAPI_D3D11::ImGuiShutdown() {
  ImGui_ImplDX11_Shutdown();
}

void RenderAPI_D3D11::ImGuiNewFrame() {
  ImGui_ImplDX11_NewFrame();
}

#endif // #if SUPPORT_D3D11
