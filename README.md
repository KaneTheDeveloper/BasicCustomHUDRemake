<h1 align="center">BasicCustomHUDRemake</h1>

<div align="center">
  <p>
    <strong>A fully customizable and optimized HUD plugin for SCP: Secret Laboratory.</strong>
  </p>
  
  <p>
    <a href="https://github.com/KaneTheDeveloper/BasicCustomHUDRemake/releases">
      <img src="https://img.shields.io/github/v/release/KaneTheDeveloper/BasicCustomHUDRemake?style=for-the-badge&color=blue" alt="Latest Version">
    </a>
  </p>
</div>

---

## Description

BasicCustomHUDRemake allows server owners to completely overhaul the in-game HUD. It creates a cleaner, more informative display for players, replacing standard elements with a system that supports:
- **Custom Player Info**: Display HP, role, round time, and more.
- **Rotating Rules**: Show server rules that cycle automatically on the spectator screen.
- **Spectator Announcements**: Fixed messages for spectators (e.g., Discord links).
- **Role Customization**: Define custom names and colors for every class in the game.
- **Optimized Performance**: Built with performance in mind using HintServiceMeow.

---

## Configuration & Parameters

This plugin offers extensive customization through `config.yml`, `rules.yml`, and `rolecolors.yml`.

### Available Placeholders
Use these placeholders in your `Format` string to display dynamic data.

**General Info:**
| Placeholder | Description |
| :--- | :--- |
| `{playername}` | The player's current nickname. |
| `{id}` | The player's ID. |
| `{role}` | The **custom name** of the player's role (colored automatically based on `rolecolors.yml`). |
| `{time}` | Current round duration (MM:SS). |
| `{tps}` | Server Ticks Per Second. |
| `{playercount}` | Current number of players. |
| `{maxplayers}` | Server maximum slots. |
| `{nextspawn}` | Time remaining until the **next** spawn wave (MTF or Chaos). |
| `{warhead}` | Current status of the Alpha Warhead (Idle, Armed, Detonating...). |
| `{generatorcount}` | Generators activated vs Total (e.g., `3/5`). |
| `{spectatorcount}` | Total number of spectators watching the round. |

**Spectator Specific:**
| Placeholder | Description |
| :--- | :--- |
| `{spectated_name}` | Name of the player you are watching. |
| `{spectated_id}` | ID of the player you are watching. |
| `{spectated_role}` | Role of the player you are watching (colored automatically). |
| `{rules}` | Displays the rotating server rules (only works in `RulesHUD`). |

### Role Configuration (`rolecolors.yml`)
Customize how roles appear in the HUD! The plugin automatically generates this file.
```yaml
NtfCaptain: MTF Captain, #0096FF
ClassD: Test Subject, orange
Scp173: The Sculpture, red
```

### Rules Configuration (`rules.yml`)
Add your server rules here. They will rotate automatically on the spectator screen.
```yaml
- No teamkilling allowed
- Respect all players
- Join our Discord!
```

---

## Credits

- **Originally made by**: [@thecroshel](https://github.com/thecroshel/BasicCustomHUD)
- **Remake by**: [@KaneTheDeveloper](https://github.com/KaneTheDeveloper)

