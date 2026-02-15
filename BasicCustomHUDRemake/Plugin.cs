using System;
using LabApi.Events.CustomHandlers;
using LabApi.Features;
using LabApi.Features.Console;
using LabApi.Loader.Features.Plugins;
using System.Threading.Tasks;

namespace BasicCustomHUDRemake
{
    /// <summary>
    /// Main entry point. Registers events and initilizes the hud manager.
    /// </summary>
    public class BasicCustomHudPlugin : Plugin<Config>
    {
        public override string Name { get; } = "BasicCustomHUD";
        public override string Description { get; } = "Displays a customizable HUD with player info during rounds.";
        public override string Author { get; } = "KaneDev";
        public override Version Version { get; } = new Version(2, 0, 0);
        public override Version RequiredApiVersion { get; } = new Version(LabApiProperties.CompiledVersion);

        public static BasicCustomHudPlugin Instance { get; private set; }

        private EventHandlers _eventHandlers;
        private HudManager _hudManager;

        public override void Enable()
        {
            Instance = this;

            _hudManager = new HudManager();
            _eventHandlers = new EventHandlers(_hudManager);

            CustomHandlersManager.RegisterEventsHandler(_eventHandlers);
            Logger.Info("BasicCustomHUD has been enabled! v" + Version);

            // Silent auto-update check in background
            Task.Run(async () => await AutoUpdater.CheckForUpdates());
        }

        /// <summary>
        /// Cleanup everthing to prevent memory leaks when plugin disables.
        /// </summary>
        public override void Disable()
        {
            if (_eventHandlers != null)
                CustomHandlersManager.UnregisterEventsHandler(_eventHandlers);

            _hudManager?.Dispose();

            _eventHandlers = null;
            _hudManager = null;
            Instance = null;

            Logger.Info("BasicCustomHUD has been disabled.");
        }
    }
}
