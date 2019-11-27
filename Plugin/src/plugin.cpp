
// update parameters
#include "Unity/IUnityGraphics.h"
#include "plugin.h"

static GameStateParameters g_parameters;

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API SetParameters (float time) { g_parameters.time = time; }

