using System;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;

namespace BasicCustomHUDRemake
{
    public class EventHandlers : CustomEventsHandler
    {
        private readonly HudManager _hudManager;

        public EventHandlers(HudManager hudManager)
        {
            _hudManager = hudManager;
        }

        public override void OnServerRoundStarted()
        {
            try
            {
                var cfg = BasicCustomHudPlugin.Instance.Config;
                if (!cfg.IsEnabled) return;

                if (!cfg.StandardHUD.Enabled && !cfg.RulesHUD.Enabled)
                    return;

                _hudManager.StartHud();
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in OnServerRoundStarted: {ex}");
            }
        }

        public override void OnServerRoundEnded(RoundEndedEventArgs ev)
        {
            try
            {
                _hudManager.StopHud();
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in OnServerRoundEnded: {ex}");
            }
        }

        public override void OnPlayerSpawned(PlayerSpawnedEventArgs ev)
        {
            try
            {
                var cfg = BasicCustomHudPlugin.Instance.Config;
                if (!cfg.IsEnabled) return;

                if (!cfg.StandardHUD.Enabled && !cfg.RulesHUD.Enabled)
                    return;

                if (Round.IsRoundStarted)
                    _hudManager.AddPlayer(ev.Player);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in OnPlayerSpawned: {ex}");
            }
        }

        public override void OnPlayerLeft(PlayerLeftEventArgs ev)
        {
            try
            {
                _hudManager.RemovePlayer(ev.Player);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in OnPlayerLeft: {ex}");
            }
        }
    }
}
