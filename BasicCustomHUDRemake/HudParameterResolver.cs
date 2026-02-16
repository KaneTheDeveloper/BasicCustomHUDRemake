using System;
using System.Linq;
using System.Text.RegularExpressions;
using LabApi.Features.Wrappers;
using PlayerRoles;
using Respawning;
using Respawning.Waves;

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
                    case "nextwave":
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

        /// <summary>
        /// Gets the time until the next spawn wave.
        /// Uses the same approach as RespawnTimer plugin:
        /// - During normal countdown: reads Timer.TimeLeft from all TimeBasedWave instances
        /// - When wave is selected/spawning: shows "Spawning..."
        /// </summary>
        private static string GetNextSpawnTime()
        {
            try
            {
                // When wave is selected or spawning, show spawnings status
                var state = WaveManager.State;
                if (state == WaveQueueState.WaveSelected || state == WaveQueueState.WaveSpawning)
                    return "Spawning...";

                var waves = WaveManager.Waves.OfType<TimeBasedWave>().ToList();
                float minTime = float.MaxValue;
                bool found = false;

                foreach (var tbw in waves)
                {
                    try
                    {
                        float t = tbw.Timer.TimeLeft;
                        if (t > 0 && t < minTime)
                        {
                            minTime = t;
                            found = true;
                        }
                    }
                    catch { }
                }

                if (found)
                {
                    int totalSeconds = Math.Max(0, (int)minTime);
                    int minutes = totalSeconds / 60;
                    int seconds = totalSeconds % 60;
                    return $"{minutes:D2}:{seconds:D2}";
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
                if (player == null) return null;
                
                // Use LabApi's CurrentlySpectating property
                return player.CurrentlySpectating;
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
                
                foreach (var gen in Map.Generators)
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
                if (Warhead.IsDetonated)
                    return "Detonated";

                if (Warhead.IsDetonationInProgress)
                {
                    // Use DetonationTime which represents the remaining time during countdown
                    int timeLeft = (int)Warhead.DetonationTime;
                    if (timeLeft > 0)
                        return $"Detonating in {timeLeft}s";
                    return "Detonating";
                }

                // Check if lever is enabled (Armed status)
                if (Warhead.LeverStatus)
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
                // Check if MTF wave is currently spawning
                var mtfState = WaveManager.State;
                if (mtfState == WaveQueueState.WaveSelected || mtfState == WaveQueueState.WaveSpawning)
                {
                    // Check if any MTF wave timer is ready
                    var readyNtf = WaveManager.Waves.OfType<NtfSpawnWave>().FirstOrDefault(w => w.Timer.IsReadyToSpawn);
                    if (readyNtf != null)
                        return "Spawning...";
                }

                var ntf = WaveManager.Waves.OfType<NtfSpawnWave>().FirstOrDefault();
                if (ntf != null)
                {
                    float t = ntf.Timer.TimeLeft;
                    if (t > 0)
                    {
                        int totalSeconds = Math.Max(0, (int)t);
                        int minutes = totalSeconds / 60;
                        int seconds = totalSeconds % 60;
                        return $"{minutes:D2}:{seconds:D2}";
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
                // Check if Chaos wave is currently spawning
                var ciState = WaveManager.State;
                if (ciState == WaveQueueState.WaveSelected || ciState == WaveQueueState.WaveSpawning)
                {
                    // Check if any Chaos wave timer is ready
                    var readyCi = WaveManager.Waves.OfType<ChaosSpawnWave>().FirstOrDefault(w => w.Timer.IsReadyToSpawn);
                    if (readyCi != null)
                        return "Spawning...";
                }

                var ci = WaveManager.Waves.OfType<ChaosSpawnWave>().FirstOrDefault();
                if (ci != null)
                {
                    float t = ci.Timer.TimeLeft;
                    if (t > 0)
                    {
                        int totalSeconds = Math.Max(0, (int)t);
                        int minutes = totalSeconds / 60;
                        int seconds = totalSeconds % 60;
                        return $"{minutes:D2}:{seconds:D2}";
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
