using System;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections;

namespace ImGuiNET.Unity
{
  public static class ImGuiPlugin
  {
    public struct IOState
    {
      public float Time;
      public int KeyShift;
      public int KeyCtrl;
      public int KeyAlt;
      public int KeySuper;
      public float DisplaySizeX;
      public float DisplaySizeY;
      public float DisplayFramebufferSizeX;
      public float DisplayFramebufferSizeY;
      public float MousePosX;
      public float MousePosY;
      public int MouseDown0;
      public int MouseDown1;
      public int MouseDown2;
      public float MouseWheel;
    }

    [DllImport("ImGuiUnity")]
    public static extern void UpdateImGuiIO(ref IOState state);

    [DllImport("ImGuiUnity")]
    public static extern void UploadKeyMap(int[] keyMap, int length);

    [DllImport("ImGuiUnity")]
    public static extern void UploadKeyDownStates(int[] keyDown, int length);

    [DllImport("ImGuiUnity")]
    public static extern IntPtr GetRenderEventFunc();
  }

  public class ImGuiIOController
  {
    PinnedArray<int> _keyMap;
    PinnedArray<int> _keyDown;
    int[] _unityKeyMap;
    public ImGuiIOController()
    {
      var io = ImGui.GetIO();
      io.ConfigFlags |= ImGuiConfigFlags.NavEnableSetMousePos;
      io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;

      io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors;
      io.BackendFlags |= ImGuiBackendFlags.HasSetMousePos;


      var iterator = Enum.GetValues(typeof(KeyCode));
      var totalKeyCodes = iterator.Length;

      _keyDown = new PinnedArray<int>(totalKeyCodes);
      _unityKeyMap = new int[totalKeyCodes];
      _keyMap = new PinnedArray<int>((int)ImGuiKey.COUNT);

      var index = 0;
      foreach (var key in iterator)
      {
        _unityKeyMap[index++] = (int)key;
      }

      _keyMap[(int)ImGuiKey.Tab] = (int)KeyCode.Tab;
      _keyMap[(int)ImGuiKey.LeftArrow] = (int)KeyCode.LeftArrow;
      _keyMap[(int)ImGuiKey.RightArrow] = (int)KeyCode.RightArrow;
      _keyMap[(int)ImGuiKey.UpArrow] = (int)KeyCode.UpArrow;
      _keyMap[(int)ImGuiKey.DownArrow] = (int)KeyCode.DownArrow;
      _keyMap[(int)ImGuiKey.PageUp] = (int)KeyCode.PageUp;
      _keyMap[(int)ImGuiKey.PageDown] = (int)KeyCode.PageDown;
      _keyMap[(int)ImGuiKey.Home] = (int)KeyCode.Home;
      _keyMap[(int)ImGuiKey.End] = (int)KeyCode.End;
      _keyMap[(int)ImGuiKey.Delete] = (int)KeyCode.Delete;
      _keyMap[(int)ImGuiKey.Backspace] = (int)KeyCode.Backspace;
      _keyMap[(int)ImGuiKey.Enter] = (int)KeyCode.Return;
      _keyMap[(int)ImGuiKey.Escape] = (int)KeyCode.Escape;
      _keyMap[(int)ImGuiKey.A] = (int)KeyCode.A;
      _keyMap[(int)ImGuiKey.C] = (int)KeyCode.C;
      _keyMap[(int)ImGuiKey.V] = (int)KeyCode.V;
      _keyMap[(int)ImGuiKey.X] = (int)KeyCode.X;
      _keyMap[(int)ImGuiKey.Y] = (int)KeyCode.Y;
      _keyMap[(int)ImGuiKey.Z] = (int)KeyCode.Z;
      ImGuiPlugin.UploadKeyMap(_keyMap.Array, _keyMap.Length);

      ImGui.GetIO().Fonts.AddFontDefault();

      unsafe
      {
        io.Fonts.GetTexDataAsRGBA32(out byte* pixelData, out int width, out int height, out int bytesPerPixel);
        io.Fonts.ClearTexData();
      }
    }

    public void Update()
    {
      for (int i = 0; i < _keyDown.Length; i++)
      {
        _keyDown[i] = Input.GetKey((KeyCode)i) ? 1 : 0;
      }

      var io = ImGui.GetIO();
      if (Input.inputString.Length > 0) {
        foreach (char c in Input.inputString)
        {
          io.AddInputCharacter(c);
        }
      }

      ImGuiPlugin.UploadKeyDownStates(_keyDown.Array, _keyDown.Length);

      var mouseWheel = Input.GetAxis("Mouse ScrollWheel");
      var state = new ImGuiPlugin.IOState()
      {
        Time = Time.deltaTime,
        KeyShift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? 1 : 0,
        KeyCtrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ? 1 : 0,
        KeyAlt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) ? 1 : 0,
        KeySuper = Input.GetKey(KeyCode.LeftWindows) || Input.GetKey(KeyCode.RightWindows) ? 1 : 0,
        DisplaySizeX = Screen.width,
        DisplaySizeY = Screen.height,
        DisplayFramebufferSizeX = 1f,
        DisplayFramebufferSizeY = 1f,
        MousePosX = Input.mousePosition.x,
        MousePosY = Screen.height - Input.mousePosition.y,
        MouseDown0 = Input.GetMouseButton(0) ? 1 : 0,
        MouseDown1 = Input.GetMouseButton(1) ? 1 : 0,
        MouseDown2 = Input.GetMouseButton(2) ? 1 : 0,
        MouseWheel = mouseWheel > 0 ? 1 : mouseWheel < 0 ? -1 : 0,
      };

      ImGuiPlugin.UpdateImGuiIO(ref state);
    }

    public void Free()
    {
      _keyMap.Release();
      _keyDown.Release();
    }
  }

  public abstract class ImGuiRendererBase : IImGuiRenderer
  {
    public abstract void Finish();

    public virtual void BeforeLayout()
    {
      ImGui.NewFrame();
    }

    public virtual void AfterLayout()
    {
      ImGui.Render();
    }
  }

  public class ImGuiRendererUnity : ImGuiRendererBase
  {

    ComputeBufferPinned _vertexBufferPinned;
    ComputeBufferPinned _indexBufferPinned;

    private Dictionary<IntPtr, Texture2D> _loadedTextures;

    private int _textureId;
    private IntPtr? _fontTextureId;

    Material _material;
    MaterialPropertyBlock _props;


    public ImGuiRendererUnity() : base()
    {
      _material = new Material(Shader.Find("UI/ImGui"));
      _props = new MaterialPropertyBlock();

      _loadedTextures = new Dictionary<IntPtr, Texture2D>();

      unsafe { RebuildFontAtlas(); }
    }

    public override void Finish()
    {
      if (_vertexBufferPinned != null)
        _vertexBufferPinned.Release();
      if (_indexBufferPinned != null)
        _indexBufferPinned.Release();
    }

    public unsafe void RebuildFontAtlas()
    {
      var io = ImGui.GetIO();
      io.Fonts.GetTexDataAsRGBA32(out byte* pixelData, out int width, out int height, out int bytesPerPixel);

      var pixels = new byte[width * height * bytesPerPixel];
      unsafe { Marshal.Copy(new IntPtr(pixelData), pixels, 0, pixels.Length); }

      var tex2d = new Texture2D(width, height, TextureFormat.ARGB32, false);
      tex2d.SetPixelData(pixels, 0);
      tex2d.Apply();

      if (_fontTextureId.HasValue)
        UnbindTexture(_fontTextureId.Value);

      _fontTextureId = BindTexture(tex2d);

      io.Fonts.SetTexID(_fontTextureId.Value);
      io.Fonts.ClearTexData();
    }

    public IntPtr BindTexture(Texture2D texture)
    {
      var id = new IntPtr(_textureId++);
      _loadedTextures.Add(id, texture);
      return id;
    }

    public void UnbindTexture(IntPtr textureId)
    {
      _loadedTextures.Remove(textureId);
    }

    public override void AfterLayout()
    {
      base.AfterLayout();
      unsafe { RenderDrawData(ImGui.GetDrawData()); }
    }

    void RenderDrawData(ImDrawDataPtr drawData)
    {
      drawData.ScaleClipRects(ImGui.GetIO().DisplayFramebufferScale);
      UpdateBuffers(drawData);
      RenderCommandLists(drawData);
    }


    unsafe void UpdateBuffers(ImDrawDataPtr drawData)
    {
      if (drawData.TotalVtxCount == 0)
        return;

      const int DrawVertDeclarationSize =
          /* sizeof(Vector2) */ 8
        + /* sizeof(Vector2) */ 8
        + /* sizeof(Vector4) */ 4;


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

      _vertexBufferPinned.UpdateData();
      _indexBufferPinned.UpdateData();
    }

    unsafe void RenderCommandLists(ImDrawDataPtr drawData)
    {
      if (_indexBufferPinned == null)
        return;

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
  }

  public class ImGuiRendererNative : ImGuiRendererBase
  {
    enum RenderingEventId
    {
      NewFrame = 0,
      Render = 1,
      Init = 2,
      Shutdown = 3,
    }

    public ImGuiRendererNative() : base()
    {
      GL.IssuePluginEvent(ImGuiPlugin.GetRenderEventFunc(), (int)RenderingEventId.Init);
    }

    public override void Finish()
    {
      GL.IssuePluginEvent(ImGuiPlugin.GetRenderEventFunc(), (int)RenderingEventId.Shutdown);
    }

    public override void BeforeLayout()
    {
      base.BeforeLayout();
      GL.IssuePluginEvent(ImGuiPlugin.GetRenderEventFunc(), (int)RenderingEventId.NewFrame);
    }

    public override void AfterLayout()
    {
      base.AfterLayout();
      GL.IssuePluginEvent(ImGuiPlugin.GetRenderEventFunc(), (int)RenderingEventId.Render);
    }
  }

  public interface IImGuiRenderer
  {
    void BeforeLayout();
    void AfterLayout();
    void Finish();
  }

  public class ImGuiInstance {
    public IImGuiRenderer Renderer { get { return _renderer; } }
    public ImGuiIOController Input { get { return _inputController; } }
    
    public ImGuiInstance() {
      var context = ImGui.CreateContext();
      ImGui.SetCurrentContext(context);

      _inputController = new ImGuiIOController();
      // TODO: Add other platforms to the native renderer.
      if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Direct3D11)
      {
        _renderer = new ImGuiRendererNative();
      }
      else
      {
        _renderer = new ImGuiRendererUnity();
      }
    }

    IImGuiRenderer _renderer;
    ImGuiIOController _inputController;
  }

  public class ImGuiRenderer : MonoBehaviour
  {
    static ImGuiRenderer instance = null;
    public static ImGuiRenderer Get(GameObject obj = null)
    {
      if (obj == null)
      {
        obj = new GameObject("_imgui");
      }
      if (instance == null)
      {
        instance = obj.AddComponent<ImGuiRenderer>();
      }
      return instance;
    }
    ImGuiInstance _imgui;

    public delegate void ImGuiLayoutDelegate();
    public ImGuiLayoutDelegate Layout;

    void Start()
    {
      _imgui = new ImGuiInstance();
      StartCoroutine("CallPluginAtEndOfFrames");
    }

    IEnumerator CallPluginAtEndOfFrames()
    {
      while (true)
      {
        yield return new WaitForEndOfFrame();
        _imgui.Renderer.BeforeLayout();
        Layout();
        _imgui.Renderer.AfterLayout();

      }
    }

    void Update() {
      _imgui.Input.Update();
    }

    void OnDisable()
    {
      _imgui.Input.Free();
      _imgui.Renderer.Finish();
    }
  }
}
