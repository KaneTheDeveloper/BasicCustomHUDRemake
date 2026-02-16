using System.ComponentModel;
using HintServiceMeow.Core.Enum;

namespace BasicCustomHUDRemake
{
    /// <summary>
    /// Display settings for a single HUD hint element.
    /// Both StandardHUD and RulesHUD use this same class.
    /// </summary>
    public class HudSettings
    {
        [Description("Enable or disable this HUD element.")]
        public bool Enabled { get; set; } = true;

        [Description("Only show this HUD to spectators?")]
        public bool OnlySpectator { get; set; } = false;

        [Description("Vertical position on screen. Higher value = lower on screen.")]
        public int YCoordinate { get; set; } = 700;

        [Description("Horizontal position on screen.")]
        public int XCoordinate { get; set; } = 0;

        [Description("Font size of the text.")]
        public int FontSize { get; set; } = 20;

        [Description("Text alignment. Options: Left, Center, Right")]
        public HintAlignment Alignment { get; set; } = HintAlignment.Center;

        [Description("Update speed. Options: Fastest, Fast, Normal, Slow, Slowest, UnSync")]
        public HintSyncSpeed SyncSpeed { get; set; } = HintSyncSpeed.Normal;

        [Description("Format string with {param} placeholders. Supports rich text tags.\n        # Common params: {playername}, {id}, {role}, {time}, {tps}, {nextspawn}, {nextwave}, {warhead}, {spectatorcount}, {generatorcount}")]
        public string Format { get; set; } = "";
    }

    public class Config
    {
        [Description("Is the plugin enabled?")]
        public bool IsEnabled { get; set; } = true;

        [Description("Should debug messages be shown in console?")]
        public bool Debug { get; set; } = false;

        [Description("Settings for the main player info HUD.")]
        public HudSettings StandardHUD { get; set; } = new HudSettings
        {
            Enabled = true,
            YCoordinate = 700,
            XCoordinate = 0,
            FontSize = 20,
            Alignment = HintAlignment.Center,
            SyncSpeed = HintSyncSpeed.Normal,
            Format = "<color=red>NAME:</color>{playername} | <color=green>TIME:</color>{time} | <color=#42e9f5>TPS:</color>{tps} | <color=blue>ROLE:</color> {role} | <color=#777777>ID:</color>{id}\nNext Spawn: {nextspawn}"
        };

        [Description("Settings for the rotating rules display HUD.")]
        public HudSettings RulesHUD { get; set; } = new HudSettings
        {
            Enabled = true,
            OnlySpectator = true,
            YCoordinate = 730,
            XCoordinate = 0,
            FontSize = 16,
            Alignment = HintAlignment.Center,
            SyncSpeed = HintSyncSpeed.Slow,
            Format = "<color=#ffcc00>{rules}</color>"
        };

        [Description("Settings for the spectator announcement HUD (fixed text above standard HUD).")]
        public HudSettings SpectatorAnnouncementHUD { get; set; } = new HudSettings
        {
            Enabled = true,
            OnlySpectator = true,
            YCoordinate = 650,
            XCoordinate = 0,
            FontSize = 20,
            Alignment = HintAlignment.Center,
            SyncSpeed = HintSyncSpeed.Slow,
            Format = "Join Our Discord Server!"
        };

        [Description("How many seconds before switching to the next rule in the {rules} parameter.")]
        public float RulePassTime { get; set; } = 5f;
    }
}
