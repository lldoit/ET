#if UNITY_EDITOR
using System;
using MCPForUnity.Editor.Helpers;
using MCPForUnity.Editor.Tools;
using Newtonsoft.Json.Linq;

namespace YIUIFramework.Editor
{
    [McpForUnityTool(
        "yiui_prefab",
        Description = "Create and update YIUI prefabs through project-safe Unity Editor APIs.",
        Group = "ui")]
    public static class YiuiMcpTool
    {
        public sealed class Parameters
        {
            [ToolParameter("Action: ping, create_panel, add_button, bind_event, generate_code, open_preview")]
            public string Action { get; set; }

            [ToolParameter("YIUI package name such as Lobby.", Required = false)]
            public string PackageName { get; set; }

            [ToolParameter("YIUI panel prefab name such as LobbyPanel.", Required = false)]
            public string PanelName { get; set; }

            [ToolParameter("Prefab asset path.", Required = false)]
            public string PrefabPath { get; set; }

            [ToolParameter("Slash-separated parent path inside the prefab.", Required = false)]
            public string Path { get; set; }

            [ToolParameter("Target object name.", Required = false)]
            public string ObjectName { get; set; }

            [ToolParameter("YIUI event name. Prefix u_Event is optional.", Required = false)]
            public string EventName { get; set; }

            [ToolParameter("Button label text.", Required = false)]
            public string Text { get; set; }
        }

        public static object HandleCommand(JObject @params)
        {
            try
            {
                if (@params == null) return new ErrorResponse("Parameters cannot be null.");
                var action = Read(@params, "action", "Action")?.ToLowerInvariant();
                if (string.IsNullOrWhiteSpace(action)) return new ErrorResponse("action is required.");

                var result = action switch
                {
                    "ping" => YiuiMcpResult.Ok("pong", new { tool = "yiui_prefab" }),
                    "create_panel" => YiuiPrefabToolService.CreateYIUIPanel(
                        Read(@params, "packageName", "PackageName"),
                        Read(@params, "panelName", "PanelName")),
                    "add_button" => YiuiPrefabToolService.AddYIUIButton(
                        Read(@params, "prefabPath", "PrefabPath"),
                        Read(@params, "path", "Path"),
                        Read(@params, "objectName", "ObjectName"),
                        Read(@params, "eventName", "EventName"),
                        Read(@params, "text", "Text")),
                    "bind_event" => YiuiPrefabToolService.BindYIUIEvent(
                        Read(@params, "prefabPath", "PrefabPath"),
                        Read(@params, "objectName", "ObjectName"),
                        Read(@params, "eventName", "EventName")),
                    "generate_code" => YiuiPrefabToolService.GenerateYIUICode(
                        Read(@params, "prefabPath", "PrefabPath")),
                    "open_preview" => YiuiPrefabToolService.OpenPrefabAndCapturePreview(
                        Read(@params, "prefabPath", "PrefabPath")),
                    _ => YiuiMcpResult.Fail($"Unsupported action: {action}")
                };

                return ToMcpResponse(result);
            }
            catch (Exception ex)
            {
                return new ErrorResponse($"yiui_prefab failed: {ex.Message}");
            }
        }

        private static object ToMcpResponse(YiuiMcpResult result)
        {
            if (result.Success) return new SuccessResponse(result.Message, result.Data);
            return new ErrorResponse(result.Message, result.Data);
        }

        private static string Read(JObject source, string camelName, string pascalName)
        {
            return source[camelName]?.ToString() ?? source[pascalName]?.ToString();
        }
    }
}
#endif
