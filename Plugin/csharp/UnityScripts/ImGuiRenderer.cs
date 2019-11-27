using System;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace ImGuiNET.Unity
{

  public class ImGuiRenderer
  {
    public void Finish()
    {
      _vertexBufferPinned.Release();
      _indexBufferPinned.Release();
    }

    private MonoBehaviour _game;

    ComputeBufferPinned _vertexBufferPinned;
    ComputeBufferPinned _indexBufferPinned;

    // Textures
    private Dictionary<IntPtr, Texture2D> _loadedTextures;

    private int _textureId;
    private IntPtr? _fontTextureId;

    // Input
    private List<int> _keys = new List<int>();

    Material _material;
    MaterialPropertyBlock _props;

    public ImGuiRenderer(MonoBehaviour game)
    {


      var context = ImGui.CreateContext();
      ImGui.SetCurrentContext(context);


      var io = ImGui.GetIO();
      io.ConfigFlags |= ImGuiConfigFlags.NavEnableSetMousePos;
      io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;

      io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors;         // We can honor GetMouseCursor() values (optional)
      io.BackendFlags |= ImGuiBackendFlags.HasSetMousePos;          // We can honor io.WantSetMousePos requests (optional, rarely used)


      _game = game ?? throw new ArgumentNullException(nameof(game));

      _material = new Material(Shader.Find("UI/ImGui"));
      _props = new MaterialPropertyBlock();

      _loadedTextures = new Dictionary<IntPtr, Texture2D>();

      SetupInput();
    }

    #region ImGuiRenderer

    /// <summary>
    /// Creates a texture and loads the font data from ImGui. Should be called when the <see cref="GraphicsDevice" /> is initialized but before any rendering is done
    /// </summary>
    public unsafe void RebuildFontAtlas()
    {
      // Get font texture from ImGui
      var io = ImGui.GetIO();
      io.Fonts.GetTexDataAsRGBA32(out byte* pixelData, out int width, out int height, out int bytesPerPixel);

      // Copy the data to a managed array
      var pixels = new byte[width * height * bytesPerPixel];
      unsafe { Marshal.Copy(new IntPtr(pixelData), pixels, 0, pixels.Length); }

      // Create and register the texture as an XNA texture
      var tex2d = new Texture2D(width, height, TextureFormat.ARGB32, false);
      tex2d.SetPixelData(pixels, 0);
      // tex2d.filterMode = FilterMode.Point;
      tex2d.Apply();

      // Should a texture already have been build previously, unbind it first so it can be deallocated
      if (_fontTextureId.HasValue) UnbindTexture(_fontTextureId.Value);

      // Bind the new texture to an ImGui-friendly id
      _fontTextureId = BindTexture(tex2d);

      // Let ImGui know where to find the texture
      io.Fonts.SetTexID(_fontTextureId.Value);
      io.Fonts.ClearTexData(); // Clears CPU side texture data
    }

    /// <summary>
    /// Creates a pointer to a texture, which can be passed through ImGui calls such as <see cref="ImGui.Image" />. That pointer is then used by ImGui to let us know what texture to draw
    /// </summary>
    public IntPtr BindTexture(Texture2D texture)
    {
      var id = new IntPtr(_textureId++);

      _loadedTextures.Add(id, texture);

      return id;
    }

    /// <summary>
    /// Removes a previously created texture pointer, releasing its reference and allowing it to be deallocated
    /// </summary>
    public void UnbindTexture(IntPtr textureId)
    {
      _loadedTextures.Remove(textureId);
    }

    /// <summary>
    /// Sets up ImGui for a new frame, should be called at frame start
    /// </summary>
    public void BeforeLayout()
    {
      ImGui.GetIO().DeltaTime = Time.deltaTime;
      // ImGui.GetIO().BackendFlags = ImGuiBackendFlags.HasSetMousePos;


      UpdateInput();

      ImGui.NewFrame();
    }

    /// <summary>
    /// Asks ImGui for the generated geometry data and sends it to the graphics pipeline, should be called after the UI is drawn using ImGui.** calls
    /// </summary>
    public void AfterLayout()
    {
      ImGui.Render();

      unsafe { RenderDrawData(ImGui.GetDrawData()); }
    }

    #endregion ImGuiRenderer

    #region Setup & Update

    /// <summary>
    /// Maps ImGui keys to XNA keys. We use this later on to tell ImGui what keys were pressed
    /// </summary>
    void SetupInput()
    {
      var io = ImGui.GetIO();

      _keys.Add(io.KeyMap[(int)ImGuiKey.Tab] = (int)KeyCode.Tab);
      _keys.Add(io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)KeyCode.LeftArrow);
      _keys.Add(io.KeyMap[(int)ImGuiKey.RightArrow] = (int)KeyCode.RightArrow);
      _keys.Add(io.KeyMap[(int)ImGuiKey.UpArrow] = (int)KeyCode.UpArrow);
      _keys.Add(io.KeyMap[(int)ImGuiKey.DownArrow] = (int)KeyCode.DownArrow);
      _keys.Add(io.KeyMap[(int)ImGuiKey.PageUp] = (int)KeyCode.PageUp);
      _keys.Add(io.KeyMap[(int)ImGuiKey.PageDown] = (int)KeyCode.PageDown);
      _keys.Add(io.KeyMap[(int)ImGuiKey.Home] = (int)KeyCode.Home);
      _keys.Add(io.KeyMap[(int)ImGuiKey.End] = (int)KeyCode.End);
      _keys.Add(io.KeyMap[(int)ImGuiKey.Delete] = (int)KeyCode.Delete);
      _keys.Add(io.KeyMap[(int)ImGuiKey.Backspace] = (int)KeyCode.Backspace);
      _keys.Add(io.KeyMap[(int)ImGuiKey.Enter] = (int)KeyCode.Return);
      _keys.Add(io.KeyMap[(int)ImGuiKey.Escape] = (int)KeyCode.Escape);
      _keys.Add(io.KeyMap[(int)ImGuiKey.A] = (int)KeyCode.A);
      _keys.Add(io.KeyMap[(int)ImGuiKey.C] = (int)KeyCode.C);
      _keys.Add(io.KeyMap[(int)ImGuiKey.V] = (int)KeyCode.V);
      _keys.Add(io.KeyMap[(int)ImGuiKey.X] = (int)KeyCode.X);
      _keys.Add(io.KeyMap[(int)ImGuiKey.Y] = (int)KeyCode.Y);
      _keys.Add(io.KeyMap[(int)ImGuiKey.Z] = (int)KeyCode.Z);


      ImGui.GetIO().Fonts.AddFontDefault();
    }

    /// <summary>
    /// Updates the <see cref="Effect" /> to the current matrices and texture
    /// </summary>
    void UpdateInput()
    {
      var io = ImGui.GetIO();

      for (int i = 0; i < _keys.Count; i++)
      {
        io.KeysDown[_keys[i]] = Input.GetKeyDown((KeyCode)_keys[i]);
      }

      io.KeyShift = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);
      io.KeyCtrl = Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl);
      io.KeyAlt = Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt);
      io.KeySuper = Input.GetKeyDown(KeyCode.LeftWindows) || Input.GetKeyDown(KeyCode.RightWindows);

      io.DisplaySize = new System.Numerics.Vector2(Screen.width, Screen.height);

      io.DisplayFramebufferScale = new System.Numerics.Vector2(2f, 2f);

      io.MousePos = new System.Numerics.Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);

      io.MouseDown[0] = Input.GetMouseButton(0);
      io.MouseDown[1] = Input.GetMouseButton(1);
      io.MouseDown[2] = Input.GetMouseButton(2);

      if (io.MouseDown[0])
      {
        Debug.Log($"{io.MousePos.X} : {io.MousePos.Y} ({Screen.width} : {Screen.height})");
      }

      var scrollDelta = Input.mouseScrollDelta.y;// mouse.ScrollWheelValue - _scrollWheelValue;
      io.MouseWheel = scrollDelta > 0 ? 1 : scrollDelta < 0 ? -1 : 0;
    }

    #endregion Setup & Update

    #region Internals

    /// <summary>
    /// Gets the geometry as set up by ImGui and sends it to the graphics device
    /// </summary>
    private void RenderDrawData(ImDrawDataPtr drawData)
    {
      // Setup render state: alpha-blending enabled, no face culling, no depth testing, scissor enabled, vertex/texcoord/color pointers
      // Handle cases of screen coordinates != from framebuffer coordinates (e.g. retina displays)
      drawData.ScaleClipRects(ImGui.GetIO().DisplayFramebufferScale);

      UpdateBuffers(drawData);

      RenderCommandLists(drawData);
    }

    int DrawVertDeclarationSize { get { return /* sizeof(Vector2) */ 8 + /* sizeof(Vector2) */ 8 + /* sizeof(Vector4) */ 4; } }

    private unsafe void UpdateBuffers(ImDrawDataPtr drawData)
    {
      if (drawData.TotalVtxCount == 0)
      {
        return;
      }

      // Expand buffers if we need more room
      if (_vertexBufferPinned == null || drawData.TotalVtxCount > _vertexBufferPinned.Size)
      {
        _vertexBufferPinned?.Release();
        _vertexBufferPinned = new ComputeBufferPinned((int)(drawData.TotalVtxCount * 1.5f), DrawVertDeclarationSize);
      }

      if (_indexBufferPinned == null || drawData.TotalIdxCount > _indexBufferPinned.Size)
      {
        _indexBufferPinned?.Release();
        _indexBufferPinned = new ComputeBufferPinned((int)(drawData.TotalIdxCount * 1.5f), 4);
      }

      // Copy ImGui's vertices and indices to a set of managed byte arrays
      int vtxOffset = 0;
      int idxOffset = 0;

      for (int n = 0; n < drawData.CmdListsCount; n++)
      {
        ImDrawListPtr cmdList = drawData.CmdListsRange[n];

        fixed (void* vtxDstPtr = &_vertexBufferPinned.Array[vtxOffset * _vertexBufferPinned.Stride])
        fixed (void* idxDstPtr = &_indexBufferPinned.Array[idxOffset * _indexBufferPinned.Stride])
        {
          Buffer.MemoryCopy((void*)cmdList.VtxBuffer.Data, vtxDstPtr, _vertexBufferPinned.Array.Length, cmdList.VtxBuffer.Size * _vertexBufferPinned.Stride);
          Buffer.MemoryCopy((void*)cmdList.IdxBuffer.Data, idxDstPtr, _indexBufferPinned.Array.Length, cmdList.IdxBuffer.Size * _indexBufferPinned.Stride);
        }

        vtxOffset += cmdList.VtxBuffer.Size;
        idxOffset += cmdList.IdxBuffer.Size;
      }

      // Copy the managed byte arrays to the gpu vertex- and index buffers
      _vertexBufferPinned.UpdateData();
      _indexBufferPinned.UpdateData();
    }

    private unsafe void RenderCommandLists(ImDrawDataPtr drawData)
    {
      var io = ImGui.GetIO();

      _props.SetBuffer("_indexBuffer", _indexBufferPinned.ComputeBuffer);
      _props.SetBuffer("_vertexBuffer", _vertexBufferPinned.ComputeBuffer);


      int vtxOffset = 0;
      int idxOffset = 0;


      for (int n = 0; n < drawData.CmdListsCount; n++)
      {
        ImDrawListPtr cmdList = drawData.CmdListsRange[n];

        for (int cmdi = 0; cmdi < cmdList.CmdBuffer.Size; cmdi++)
        {
          ImDrawCmdPtr drawCmd = cmdList.CmdBuffer[cmdi];

          if (!_loadedTextures.ContainsKey(drawCmd.TextureId))
          {
            throw new InvalidOperationException($"Could not find a texture with id '{drawCmd.TextureId}', please check your bindings");
          }

          var offset = 0f;
          _props.SetTexture("_MainTex", _loadedTextures[drawCmd.TextureId]);
          _props.SetInt("idxOffset", idxOffset);
          _props.SetInt("vtxOffset", vtxOffset);

          _props.SetMatrix("projection", Matrix4x4.Ortho(offset, io.DisplaySize.X + offset, offset, io.DisplaySize.Y + offset, -1f, 1f));


          Graphics.DrawProcedural(_material, new Bounds(Vector2.zero, Vector3.one * 1000.0f), MeshTopology.Triangles, (int)drawCmd.ElemCount, 1, Camera.main, _props);

          idxOffset += (int)drawCmd.ElemCount;
        }
        vtxOffset += cmdList.VtxBuffer.Size;
      }
    }
    #endregion Internals
  }
}
