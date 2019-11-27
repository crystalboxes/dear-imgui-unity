using System.Collections;
using System.Collections.Generic;
using ImGuiNET;
using UnityEngine;
using ImGuiNET.Unity;

using Num = System.Numerics;
public class ImguTester : MonoBehaviour
{
  ImGuiRenderer _imGuiRenderer;

  // Direct port of the example at https://github.com/ocornut/imgui/blob/master/examples/sdl_opengl2_example/main.cpp
  private float f = 0.0f;

  private bool show_test_window = false;
  private bool show_another_window = false;
  private System.Numerics.Vector3 clear_color = new System.Numerics.Vector3(114f / 255f, 144f / 255f, 154f / 255f);
  private byte[] _textBuffer = new byte[100];

  // Start is called before the first frame update
  void Start()
  {
    _imGuiRenderer = new ImGuiRenderer(this);
    _imGuiRenderer.RebuildFontAtlas();
  }

  // Update is called once per frame
  void Update()
  {
    // GraphicsDevice.Clear(new Color(clear_color.X, clear_color.Y, clear_color.Z));

    // Call BeforeLayout first to set things up
    _imGuiRenderer.BeforeLayout();

    // Draw our UI
    ImGuiLayout();

    // Call AfterLayout now to finish up and draw all the things
    _imGuiRenderer.AfterLayout();
  }

  void OnDisable()
  {
    _imGuiRenderer.Finish();
  }

  void ImGuiLayout()
  {
    // 1. Show a simple window
    // Tip: if we don't call ImGui.Begin()/ImGui.End() the widgets appears in a window automatically called "Debug"
    {
      if (ImGui.IsMouseClicked(0))
        ImGui.Text("Hello, world!");
      else
        ImGui.Text($"X: {ImGui.GetMousePos().X} Y: {ImGui.GetMousePos().X}");

      ImGui.SliderFloat("float", ref f, 0.0f, 1.0f, string.Empty, 1f);
      ImGui.ColorEdit3("clear color", ref clear_color);
      if (ImGui.Button("Test Window")) show_test_window = !show_test_window;
      if (ImGui.Button("Another Window")) show_another_window = !show_another_window;
      ImGui.Text(string.Format("Application average {0:F3} ms/frame ({1:F1} FPS)", 1000f / ImGui.GetIO().Framerate, ImGui.GetIO().Framerate));

      ImGui.InputText("Text input", _textBuffer, 100);

      ImGui.Text($"Texture sample {Time.time}");
      // ImGui.Image(_imGuiTexture, new Num.Vector2(300, 150), Num.Vector2.Zero, Num.Vector2.One, Num.Vector4.One, Num.Vector4.One); 
      // Here, the previously loaded texture is used
    }

    // 2. Show another simple window, this time using an explicit Begin/End pair
    if (show_another_window)
    {
      ImGui.SetNextWindowSize(new Num.Vector2(200, 100), ImGuiCond.FirstUseEver);
      ImGui.Begin("Another Window", ref show_another_window);
      ImGui.Text("Hello");
      ImGui.End();
    }

    // 3. Show the ImGui test window. Most of the sample code is in ImGui.ShowTestWindow()
    if (show_test_window)
    {
      ImGui.SetNextWindowPos(new Num.Vector2(650, 20), ImGuiCond.FirstUseEver);
      ImGui.ShowDemoWindow(ref show_test_window);
    }

  }

}
