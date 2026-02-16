using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LabApi.Features.Console;
using PlayerRoles;

namespace BasicCustomHUDRemake
{
    /// <summary>
    /// Manages role configuration (names and colors) via rolecolors.yml.
    /// </summary>
    public class RoleConfigManager
    {
        private class RoleDisplay
        {
            public string Name { get; set; }
            public string Color { get; set; } // Hex code like #FF0000 or color name like red
        }

        private readonly Dictionary<RoleTypeId, RoleDisplay> _roleDisplays = new Dictionary<RoleTypeId, RoleDisplay>();
        private string _configPath;

        public void LoadConfig(string configDirectory)
        {
            if (!Directory.Exists(configDirectory))
            {
                try
                {
                    Directory.CreateDirectory(configDirectory);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Could not create config directory for role colors: {ex.Message}");
                    return;
                }
            }

            _configPath = Path.Combine(configDirectory, "rolecolors.yml");

            if (!File.Exists(_configPath))
            {
                CreateDefaultConfig();
            }

            ParseConfig();
        }

        public string GetRoleName(RoleTypeId role)
        {
            return _roleDisplays.TryGetValue(role, out var display) ? display.Name : role.ToString();
        }

        public string GetRoleColor(RoleTypeId role)
        {
            return _roleDisplays.TryGetValue(role, out var display) ? display.Color : "white";
        }

        private void CreateDefaultConfig()
        {
            try
            {
                var lines = new List<string>
                {
                    "# Format: RoleId: Name, Color",
                    "# Colors can be hex codes (#FF0000) or standard names (red, blue, etc.)"
                };

                foreach (RoleTypeId role in Enum.GetValues(typeof(RoleTypeId)))
                {
                    if (role == RoleTypeId.None) continue;

                    string defaultColor = GetDefaultColor(role);
                    string defaultName = role.ToString();

                    // Prettier default names for some roles
                    if (role == RoleTypeId.ClassD) defaultName = "Class-D";
                    if (role == RoleTypeId.Scientist) defaultName = "Scientist";
                    if (role == RoleTypeId.FacilityGuard) defaultName = "Facility Guard";
                    if (role == RoleTypeId.NtfPrivate) defaultName = "Nine-Tailed Fox Private";
                    if (role == RoleTypeId.NtfSergeant) defaultName = "Nine-Tailed Fox Sergeant";
                    if (role == RoleTypeId.NtfSpecialist) defaultName = "Nine-Tailed Fox Specialist";
                    if (role == RoleTypeId.NtfCaptain) defaultName = "Nine-Tailed Fox Captain";
                    if (role == RoleTypeId.ChaosConscript) defaultName = "Chaos Insurgency Conscript";
                    if (role == RoleTypeId.ChaosRifleman) defaultName = "Chaos Insurgency Rifleman";
                    if (role == RoleTypeId.ChaosRepressor) defaultName = "Chaos Insurgency Repressor";
                    if (role == RoleTypeId.ChaosMarauder) defaultName = "Chaos Insurgency Marauder";

                    lines.Add($"{role}: {defaultName}, {defaultColor}");
                }

                File.WriteAllLines(_configPath, lines);
                Logger.Info($"Created default role config at: {_configPath}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to create default role config: {ex.Message}");
            }
        }

        private void ParseConfig()
        {
            _roleDisplays.Clear();
            try
            {
                var lines = File.ReadAllLines(_configPath);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                    var parts = line.Split(':');
                    if (parts.Length < 2) continue;

                    if (Enum.TryParse(parts[0].Trim(), out RoleTypeId role))
                    {
                        var values = parts[1].Split(',');
                        string name = values[0].Trim();
                        string color = values.Length > 1 ? values[1].Trim() : "white";

                        _roleDisplays[role] = new RoleDisplay { Name = name, Color = color };
                    }
                }
                Logger.Debug($"Loaded {_roleDisplays.Count} role configurations.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to parse role config: {ex.Message}");
            }
        }

        private string GetDefaultColor(RoleTypeId role)
        {
            switch (role.GetTeam())
            {
                case Team.SCPs: return "red";
                case Team.FoundationForces: return "#0096FF"; // MTF Blue
                case Team.ChaosInsurgency: return "#008F1C"; // Chaos Green
                case Team.Scientists: return "#FFFF7C"; // Scientist Yellow
                case Team.ClassD: return "#FF8E00"; // Class-D Orange
                case Team.Dead: return "grey"; // Spectator
                default: return "white";
            }
        }
    }
}
