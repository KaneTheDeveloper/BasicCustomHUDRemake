using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LabApi.Features.Console;

namespace BasicCustomHUDRemake
{
    /// <summary>
    /// Loads rules from rules.yml and cycles trough them based on config timer.
    /// Each line in the file is treated as a seperate rule.
    /// </summary>
    public class RulesManager
    {
        private List<string> _rules = new List<string>();
        private int _currentIndex;
        private DateTime _lastSwitch = DateTime.MinValue;

        public void LoadRules(string configDirectory)
        {
            _rules.Clear();
            _currentIndex = 0;

            if (!Directory.Exists(configDirectory))
            {
                try
                {
                    Directory.CreateDirectory(configDirectory);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Could not create config directory for rules: {ex.Message}");
                    return;
                }
            }

            string rulesPath = Path.Combine(configDirectory, "rules.txt");

            if (!File.Exists(rulesPath))
            {
                // create a default file so admins know where to put rules
                try
                {
                    File.WriteAllLines(rulesPath, new[]
                    {
                        "No teamkilling allowed",
                        "Respect all players",
                        "Follow staff instructions",
                        "Have fun!"
                    });
                    Logger.Info($"Created default rules file at: {rulesPath}");
                }
                catch (Exception ex)
                {
                    Logger.Error($"Couldnt create rules file: {ex.Message}");
                    return;
                }
            }

            try
            {
                var lines = File.ReadAllLines(rulesPath)
                    .Where(l => !string.IsNullOrWhiteSpace(l))
                    .ToList();

                _rules = lines;
                Logger.Debug($"Loaded {_rules.Count} rules from file.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to read rules file: {ex.Message}");
            }
        }

        /// <summary>
        /// Returns the current rule text. Automaticaly switches to next rule
        /// when enough time has passed based on the interval.
        /// </summary>
        public string GetCurrentRule(float intervalSeconds)
        {
            if (_rules.Count == 0)
                return "No rules loaded";

            var now = DateTime.UtcNow;
            if ((now - _lastSwitch).TotalSeconds >= intervalSeconds)
            {
                _currentIndex = (_currentIndex + 1) % _rules.Count;
                _lastSwitch = now;
            }

            return _rules[_currentIndex];
        }
    }
}
