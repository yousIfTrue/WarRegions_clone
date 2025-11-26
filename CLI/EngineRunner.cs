namespace WarRegions.CLI
{
    // Simple interactive CLI for controlling the minimal engine via EngineController.
    // No using directives here because GlobalUsings.cs is expected to provide them.
    public class EngineRunner
    {
        private readonly Core.Controllers.EngineController _controller = new Core.Controllers.EngineController();
        private bool _exitRequested;

        public static int Main(string[] args)
        {
            var runner = new EngineRunner();

            // If arguments were provided, run a single command and exit
            if (args != null && args.Length > 0)
            {
                try
                {
                    runner.HandleArgs(args);
                    return 0;
                }
                catch (Exception ex)
                {
                    Core.Engine.Debug.LogError($"EngineRunner error: {ex.Message}");
                    return 1;
                }
            }

            // Otherwise run interactive REPL
            runner.RunInteractive();
            return 0;
        }

        private void HandleArgs(string[] args)
        {
            var cmd = args[0].ToLowerInvariant();
            switch (cmd)
            {
                case "start":
                    _controller.StartEngine();
                    break;
                case "stop":
                    _controller.StopEngine();
                    break;
                case "create":
                case "instantiate":
                    {
                        var name = args.Length > 1 ? args[1] : "GameObject";
                        var go = _controller.Instantiate(name);
                        Core.Engine.Debug.Log($"Instantiated GameObject '{go?.name}'");
                        break;
                    }
                case "load":
                    {
                        if (args.Length < 2) throw new ArgumentException("load requires scene name");
                        var scene = _controller.LoadScene(args[1], setActive: true);
                        Core.Engine.Debug.Log($"Loaded scene '{scene?.name}' and set active");
                        break;
                    }
                case "unload":
                    {
                        if (args.Length < 2) throw new ArgumentException("unload requires scene name");
                        var ok = _controller.UnloadScene(args[1]);
                        Core.Engine.Debug.Log(ok ? $"Unloaded scene '{args[1]}'" : $"Scene '{args[1]}' not found");
                        break;
                    }
                default:
                    throw new ArgumentException($"Unknown command: {cmd}");
            }
        }

        public void RunInteractive()
        {
            Core.Engine.Debug.Log("EngineRunner CLI - type 'help' for commands.");
            _controller.Initialize();

            while (!_exitRequested)
            {
                Console.Write("> ");
                var line = Console.ReadLine();
                if (line == null) break; // EOF

                var parts = SplitArgs(line);
                if (parts.Length == 0) continue;

                var cmd = parts[0].ToLowerInvariant();
                try
                {
                    switch (cmd)
                    {
                        case "help":
                        case "?":
                            PrintHelp();
                            break;
                        case "start":
                            _controller.StartEngine();
                            break;
                        case "stop":
                            _controller.StopEngine();
                            break;
                        case "create":
                        case "instantiate":
                            {
                                var name = parts.Length > 1 ? parts[1] : "GameObject";
                                var go = _controller.Instantiate(name);
                                Core.Engine.Debug.Log($"Instantiated GameObject '{go?.name}'");
                                break;
                            }
                        case "list":
                            {
                                if (parts.Length < 2) { Core.Engine.Debug.Log("Usage: list scenes|roots [sceneName]"); break; }
                                var what = parts[1].ToLowerInvariant();
                                if (what == "scenes")
                                {
                                    var scenes = Core.Engine.SceneManager.GetAllScenes();
                                    if (scenes == null || scenes.Length == 0) Core.Engine.Debug.Log("No scenes loaded.");
                                    else
                                    {
                                        Core.Engine.Debug.Log($"Scenes ({scenes.Length}):");
                                        foreach (var s in scenes) Core.Engine.Debug.Log($" - {s.name}");
                                    }
                                }
                                else if (what == "roots")
                                {
                                    if (parts.Length > 2)
                                    {
                                        var name = parts[2];
                                        var scene = Core.Engine.SceneManager.GetSceneByName(name);
                                        if (scene == null) { Core.Engine.Debug.Log($"Scene '{name}' not found."); break; }
                                        var roots = scene.GetRootGameObjects();
                                        Core.Engine.Debug.Log($"Roots in '{name}':");
                                        foreach (var r in roots) Core.Engine.Debug.Log($" - {r.name}");
                                    }
                                    else
                                    {
                                        var active = Core.Engine.SceneManager.GetActiveScene();
                                        if (active == null) { Core.Engine.Debug.Log("No active scene."); break; }
                                        var roots = active.GetRootGameObjects();
                                        Core.Engine.Debug.Log($"Roots in active scene '{active.name}':");
                                        foreach (var r in roots) Core.Engine.Debug.Log($" - {r.name}");
                                    }
                                }
                                else Core.Engine.Debug.Log("Unknown list target. Use: list scenes | list roots [sceneName]");
                                break;
                            }
                        case "load":
                            {
                                if (parts.Length < 2) { Core.Engine.Debug.Log("Usage: load <sceneName>"); break; }
                                var scene = _controller.LoadScene(parts[1], setActive: true);
                                Core.Engine.Debug.Log($"Loaded and activated scene '{scene?.name}'");
                                break;
                            }
                        case "unload":
                            {
                                if (parts.Length < 2) { Core.Engine.Debug.Log("Usage: unload <sceneName>"); break; }
                                var ok = _controller.UnloadScene(parts[1]);
                                Core.Engine.Debug.Log(ok ? $"Unloaded scene '{parts[1]}'" : $"Scene '{parts[1]}' not found");
                                break;
                            }
                        case "destroy":
                            {
                                if (parts.Length < 2) { Core.Engine.Debug.Log("Usage: destroy <rootName>"); break; }
                                var active = Core.Engine.SceneManager.GetActiveScene();
                                if (active == null) { Core.Engine.Debug.Log("No active scene."); break; }
                                var go = active.Find(parts[1]);
                                if (go == null) { Core.Engine.Debug.Log($"Root GameObject '{parts[1]}' not found in active scene."); break; }
                                _controller.DestroyGameObject(go);
                                Core.Engine.Debug.Log($"Destroyed GameObject '{parts[1]}'");
                                break;
                            }
                        case "delayed":
                            {
                                if (parts.Length < 3) { Core.Engine.Debug.Log("Usage: delayed <seconds> <message>"); break; }
                                if (!float.TryParse(parts[1], out var sec))
                                {
                                    Core.Engine.Debug.Log("Invalid seconds value.");
                                    break;
                                }
                                var msg = string.Join(" ", parts.Skip(2));
                                Core.Engine.Debug.Log($"Scheduling delayed log in {sec} seconds: {msg}");
                                Core.Engine.GameEngine.Instance.DelayedCall(() => Core.Engine.Debug.Log($"[delayed] {msg}"), sec);
                                break;
                            }
                        case "time":
                            {
                                Core.Engine.Debug.Log($"time: {Core.Engine.Time.time:F3}, unscaledTime: {Core.Engine.Time.unscaledTime:F3}, deltaTime: {Core.Engine.Time.deltaTime:F3}, frame: {Core.Engine.Time.framecount}");
                                break;
                            }
                        case "quit":
                        case "exit":
                            _controller.Quit();
                            _exitRequested = true;
                            break;
                        default:
                            Core.Engine.Debug.Log($"Unknown command '{cmd}'. Type 'help' for available commands.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Core.Engine.Debug.LogError($"Command error: {ex.Message}");
                }
            }

            Core.Engine.Debug.Log("EngineRunner: exiting.");
        }

        private void PrintHelp()
        {
            Core.Engine.Debug.Log("Commands:");
            Core.Engine.Debug.Log("  help                     - show this help");
            Core.Engine.Debug.Log("  start                    - start the engine");
            Core.Engine.Debug.Log("  stop                     - stop the engine");
            Core.Engine.Debug.Log("  create|instantiate [name]- create a GameObject in active scene");
            Core.Engine.Debug.Log("  list scenes              - list all scenes");
            Core.Engine.Debug.Log("  list roots [sceneName]   - list root GameObjects");
            Core.Engine.Debug.Log("  load <sceneName>         - load and activate scene");
            Core.Engine.Debug.Log("  unload <sceneName>       - unload scene");
            Core.Engine.Debug.Log("  destroy <rootName>       - destroy a root GameObject by name");
            Core.Engine.Debug.Log("  delayed <s> <message>    - schedule a delayed log");
            Core.Engine.Debug.Log("  time                     - print time info");
            Core.Engine.Debug.Log("  quit | exit              - stop engine (if running) and exit");
        }

        // Simple argument splitter (preserves quoted strings)
        private static string[] SplitArgs(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return new string[0];
            var parts = new List<string>();
            var current = new System.Text.StringBuilder();
            bool inQuotes = false;
            for (int i = 0; i < input.Length; i++)
            {
                var c = input[i];
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    continue;
                }
                if (!inQuotes && char.IsWhiteSpace(c))
                {
                    if (current.Length > 0)
                    {
                        parts.Add(current.ToString());
                        current.Clear();
                    }
                    continue;
                }
                current.Append(c);
            }
            if (current.Length > 0) parts.Add(current.ToString());
            return parts.ToArray();
        }
    }
}

