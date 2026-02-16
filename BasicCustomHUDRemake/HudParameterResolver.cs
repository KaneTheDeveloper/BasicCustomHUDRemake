using System;
using System.Text.RegularExpressions;
using LabApi.Features.Wrappers;

namespace BasicCustomHUDRemake
{
    /// <summary>
    /// Resolves parameter placeholders like {tps}, {playercount} etc in the HUD format string.
    /// Each parameter is wraped in curly braces and gets replaced with the actual value.
    /// </summary>
    public static class HudParameterResolver
    {
        private static readonly Regex ParamRegex = new Regex(@"\{(\w+)\}", RegexOptions.Compiled);

        /// <summary>
        /// Takes the format string from config and replaces all {param} placeholders
        /// with there actual values. Unknown params are left as-is.
        /// </summary>
        public static string Resolve(string format, Player player, RulesManager rulesManager, RoleConfigManager roleManager, float ruleInterval)
        {
            if (string.IsNullOrEmpty(format))
                return string.Empty;

            return ParamRegex.Replace(format, match =>
            {
                string param = match.Groups[1].Value.ToLowerInvariant();

                switch (param)
                {
                    case "tps":
                        return ((float)Server.Tps).ToString("F1");

                    case "playercount":
                        return Server.PlayerCount.ToString();

                    case "maxplayers":
                        return Server.MaxPlayers.ToString();

                    case "id":
                        return player.PlayerId.ToString();

                    case "playername":
                        return player.Nickname ?? "Unknown";

                    case "role":
                        string rName = roleManager.GetRoleName(player.Role);
                        string rColor = roleManager.GetRoleColor(player.Role);
                        return $"<color={rColor}>{rName}</color>";

                    case "time":
                        var elapsed = Round.Duration;
                        return $"{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";

                    case "rules":
                        return rulesManager.GetCurrentRule(ruleInterval);

                    case "mtfspawnleft":
                        return GetMtfSpawnTime();

                    case "chaosspawnleft":
                        return GetChaosSpawnTime();

                    case "nextspawn":
                        return GetNextSpawnTime();

                    case "spectated_name":
                        return GetSpectatedPlayerName(player);

                    case "spectated_id":
                        return GetSpectatedPlayerId(player);

                    case "spectated_role":
                        return GetSpectatedPlayerRole(player, roleManager);

                    case "spectatorcount":
                        return GetSpectatorCount();

                    case "generatorcount":
                        return GetGeneratorCount();

                    case "warhead":
                        return GetWarheadStatus();

                    default:
                        return match.Value; // leave unknown params untouched
                }
            });
        }

        private static string GetNextSpawnTime()
        {
            try
            {
                float minTime = float.MaxValue;
                bool found = false;

                void CheckWave(LabApi.Features.Wrappers.RespawnWaves.Wave wave)
                {
                    if (wave != null)
                    {
                        float t = wave.TimeLeft;
                        if (t > 0 && t < minTime)
                        {
                            minTime = t;
                            found = true;
                        }
                    }
                }

                CheckWave(RespawnWaves.PrimaryMtfWave);
                CheckWave(RespawnWaves.MiniMtfWave);
                CheckWave(RespawnWaves.PrimaryChaosWave);
                CheckWave(RespawnWaves.MiniChaosWave);

                if (found)
                {
                    var ts = TimeSpan.FromSeconds(minTime);
                    return $"{ts.Minutes:D2}:{ts.Seconds:D2}";
                }

                return "N/A";
            }
            catch
            {
                return "N/A";
            }
        }

        private static Player GetSpectatedTarget(Player player)
        {
            try
            {
                if (player == null || player.ReferenceHub == null) return null;
                
                // Use native ReferenceHub to find spectated player
                var targetHub = player.ReferenceHub.spectatorManager.ServerCurrentSpectatingTarget;
                if (targetHub == null) return null;

                return Player.Get(targetHub);
            }
            catch
            {
                return null;
            }
        }

        private static string GetSpectatedPlayerName(Player player)
        {
            var target = GetSpectatedTarget(player);
            return target != null ? target.Nickname : "None";
        }

        private static string GetSpectatedPlayerId(Player player)
        {
            var target = GetSpectatedTarget(player);
            return target != null ? target.PlayerId.ToString() : "None";
        }

        private static string GetSpectatedPlayerRole(Player player, RoleConfigManager roleManager)
        {
            var target = GetSpectatedTarget(player);
            if (target == null) return "None";
            
            string rName = roleManager.GetRoleName(target.Role);
            string rColor = roleManager.GetRoleColor(target.Role);
            return $"<color={rColor}>{rName}</color>";
        }

        private static string GetSpectatorCount()
        {
            // Count players with Spectator role
            // Assuming Player.List is available in LabApi or we iterate hubs
            try
            {
                int count = 0;
                foreach (var hub in ReferenceHub.AllHubs)
                {
                    if (hub.roleManager.CurrentRole.RoleTypeId == PlayerRoles.RoleTypeId.Spectator)
                        count++;
                }
                return count.ToString();
            }
            catch
            {
                return "0";
            }
        }

        private static string GetGeneratorCount()
        {
            try
            {
                int engaged = 0;
                int total = 0;
                
                foreach (var gen in MapGeneration.Distributors.Scp079Generator.List)
                {
                    total++;
                    if (gen.Engaged)
                        engaged++;
                }
                
                return $"{engaged}/{total}";
            }
            catch
            {
                return "0/0";
            }
        }

        private static string GetWarheadStatus()
        {
            try
            {
                if (LabApi.Features.Warhead.IsDetonated)
                    return "Detonated";

                if (LabApi.Features.Warhead.IsInProgress)
                {
                    int timeLeft = (int)LabApi.Features.Warhead.TimeLeft;
                    return $"Detonating in {timeLeft}s";
                }

                if (AlphaWarheadController.Singleton.TimeLeft <= 0) // Already blew up but status might lag
                     return "Detonated";

                // Check lever status using native controller if generic wrapper doesn't have it
                // LabApi.Features.Warhead.IsLeverEnabled might exist? 
                // Let's use native to be safe for "Armed" status
                if (AlphaWarheadOutsitePanel.nukeside.Networkenabled)
                     return "Armed";
                
                return "Idle";
            }
            catch
            {
                return "Unknown";
            }
        }

        private static string GetMtfSpawnTime()
        {
            try
            {
                // check main wave first, then mini wave
                var mainWave = RespawnWaves.PrimaryMtfWave;
                if (mainWave != null)
                {
                    float timeLeft = mainWave.TimeLeft;
                    if (timeLeft > 0)
                    {
                        var ts = TimeSpan.FromSeconds(timeLeft);
                        return $"{ts.Minutes:D2}:{ts.Seconds:D2}";
                    }
                }

                var miniWave = RespawnWaves.MiniMtfWave;
                if (miniWave != null)
                {
                    float timeLeft = miniWave.TimeLeft;
                    if (timeLeft > 0)
                    {
                        var ts = TimeSpan.FromSeconds(timeLeft);
                        return $"{ts.Minutes:D2}:{ts.Seconds:D2}";
                    }
                }

                return "N/A";
            }
            catch
            {
                return "N/A";
            }
        }

        private static string GetChaosSpawnTime()
        {
            try
            {
                var mainWave = RespawnWaves.PrimaryChaosWave;
                if (mainWave != null)
                {
                    float timeLeft = mainWave.TimeLeft;
                    if (timeLeft > 0)
                    {
                        var ts = TimeSpan.FromSeconds(timeLeft);
                        return $"{ts.Minutes:D2}:{ts.Seconds:D2}";
                    }
                }

                var miniWave = RespawnWaves.MiniChaosWave;
                if (miniWave != null)
                {
                    float timeLeft = miniWave.TimeLeft;
                    if (timeLeft > 0)
                    {
                        var ts = TimeSpan.FromSeconds(timeLeft);
                        return $"{ts.Minutes:D2}:{ts.Seconds:D2}";
                    }
                }

                return "N/A";
            }
            catch
            {
                return "N/A";
            }
        }
    }
}
