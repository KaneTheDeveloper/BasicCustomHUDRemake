using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Paths;

namespace BasicCustomHUDRemake
{
    /// <summary>
    /// Handles all HUD rendering using HSM's AutoText delagate.
    /// Creates two seperate hints per player: StandardHUD and RulesHUD.
    /// </summary>
    public class HudManager : IDisposable
    {
        private readonly Dictionary<int, HintPair> _playerHints = new Dictionary<int, HintPair>();
        private readonly object _lock = new object();
        private readonly RulesManager _rulesManager = new RulesManager();
        private readonly RoleConfigManager _roleConfigManager = new RoleConfigManager();
        private bool _disposed;

        /// <summary>
        /// Holds both hint objects for a single player so we can track them together.
        /// </summary>
        private class HintPair
        {
            public Hint StandardHint;
            public Hint RulesHint;
            public Hint AnnouncementHint;
        }

        public void StartHud()
        {
            lock (_lock)
            {
                ClearAllHints();

                try
                {
                    string pluginDir = Path.Combine(PathManager.Configs.FullName, Server.Port.ToString(), "BasicCustomHUD");
                    if (!string.IsNullOrEmpty(pluginDir))
                    {
                        _rulesManager.LoadRules(pluginDir);
                        _roleConfigManager.LoadConfig(pluginDir);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to load configs: {ex.Message}");
                }

                if (BasicCustomHudPlugin.Instance.Config.Debug)
                    Logger.Debug("HUD system started.");
            }
        }

        public void StopHud()
        {
            lock (_lock)
            {
                ClearAllHints();
                if (BasicCustomHudPlugin.Instance.Config.Debug)
                    Logger.Debug("HUD stopped, all hints removed.");
            }
        }

        public void AddPlayer(Player player)
        {
            if (player?.ReferenceHub == null)
                return;

            lock (_lock)
            {
                if (_playerHints.ContainsKey(player.PlayerId))
                    return;

                CreateHintsForPlayer(player);
            }
        }

        public void RemovePlayer(Player player)
        {
            if (player == null)
                return;

            lock (_lock)
            {
                if (!_playerHints.TryGetValue(player.PlayerId, out var pair))
                    return;

                try
                {
                    if (player.ReferenceHub != null)
                    {
                        var display = PlayerDisplay.Get(player.ReferenceHub);
                        if (display != null)
                        {
                            if (pair.StandardHint != null) display.RemoveHint(pair.StandardHint);
                            if (pair.RulesHint != null) display.RemoveHint(pair.RulesHint);
                            if (pair.AnnouncementHint != null) display.RemoveHint(pair.AnnouncementHint);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (BasicCustomHudPlugin.Instance.Config.Debug)
                        Logger.Debug($"Could not remove hints for leaving player: {ex.Message}");
                }

                _playerHints.Remove(player.PlayerId);
            }
        }

        /// <summary>
        /// Creates both StandardHUD and RulesHUD hints for a player.
        /// Each hint gets its own settings from config.
        /// </summary>
        private void CreateHintsForPlayer(Player player)
        {
            var config = BasicCustomHudPlugin.Instance.Config;
            var hub = player.ReferenceHub;
            int playerId = player.PlayerId;

            var pair = new HintPair();

            try
            {
                var display = PlayerDisplay.Get(hub);
                if (display == null)
                    return;

                if (config.StandardHUD.Enabled)
                {
                    pair.StandardHint = CreateHint(config.StandardHUD, hub);
                    display.AddHint(pair.StandardHint);
                }

                if (config.RulesHUD.Enabled)
                {
                    pair.RulesHint = CreateHint(config.RulesHUD, hub);
                    display.AddHint(pair.RulesHint);
                }

                if (config.SpectatorAnnouncementHUD.Enabled)
                {
                    pair.AnnouncementHint = CreateHint(config.SpectatorAnnouncementHUD, hub);
                    display.AddHint(pair.AnnouncementHint);
                }

                _playerHints[playerId] = pair;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to create HUD hints for player: {ex.Message}");
            }
        }

        /// <summary>
        /// Builds a single Hint object from the given HudSettings.
        /// </summary>
        private Hint CreateHint(HudSettings settings, ReferenceHub hub)
        {
            return new Hint
            {
                AutoText = (ev) => BuildText(settings, hub),
                YCoordinate = settings.YCoordinate,
                XCoordinate = settings.XCoordinate,
                FontSize = settings.FontSize,
                Alignment = settings.Alignment,
                SyncSpeed = settings.SyncSpeed
            };
        }

        /// <summary>
        /// Resolves the format string for a specific HudSettings and player.
        /// </summary>
        private string BuildText(HudSettings settings, ReferenceHub hub)
        {
            try
            {
                if (hub == null || hub.gameObject == null)
                    return string.Empty;

                var player = Player.Get(hub);
                if (player == null)
                    return string.Empty;

                // check spectator requirement
                if (settings.OnlySpectator)
                {
                    string r = player.Role.ToString();
                    if (r != "Spectator" && r != "Overwatch")
                        return string.Empty;
                }

                var config = BasicCustomHudPlugin.Instance.Config;
                return $"<size={settings.FontSize}>{HudParameterResolver.Resolve(settings.Format, player, _rulesManager, _roleConfigManager, config.RulePassTime)}</size>";
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private void ClearAllHints()
        {
            var entries = _playerHints.ToList();

            foreach (var kvp in entries)
            {
                try
                {
                    var player = Player.Get(kvp.Key);
                    if (player?.ReferenceHub != null)
                    {
                        var display = PlayerDisplay.Get(player.ReferenceHub);
                        if (display != null)
                        {
                            if (kvp.Value.StandardHint != null) display.RemoveHint(kvp.Value.StandardHint);
                            if (kvp.Value.RulesHint != null) display.RemoveHint(kvp.Value.RulesHint);
                            if (kvp.Value.AnnouncementHint != null) display.RemoveHint(kvp.Value.AnnouncementHint);
                        }
                    }
                }
                catch (Exception)
                {
                    // player probaly gone already
                }
            }

            _playerHints.Clear();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            lock (_lock)
            {
                ClearAllHints();
                _disposed = true;
            }
        }
    }
}
