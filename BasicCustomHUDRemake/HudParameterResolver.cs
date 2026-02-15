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
        public static string Resolve(string format, Player player, RulesManager rulesManager, float ruleInterval)
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
                        return player.Role.ToString();

                    case "time":
                        var elapsed = Round.Duration;
                        return $"{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";

                    case "rules":
                        return rulesManager.GetCurrentRule(ruleInterval);

                    case "mtfspawnleft":
                        return GetMtfSpawnTime();

                    case "chaosspawnleft":
                        return GetChaosSpawnTime();

                    default:
                        return match.Value; // leave unknown params untouched
                }
            });
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
