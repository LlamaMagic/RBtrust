# [RBtrust][github-repo]

[![Download][download-badge]][download-link]
[![Discord][discord-badge]][discord-invite]

🌎 **English** • [中文][readme-zh]

**RBtrust** is a dungeon plugin + OrderBot scripts for [RebornBuddy][rebornbuddy]. It automatically runs Duty Support and Trust dungeons.

[readme-zh]: ./README.zh.md "中文"
[readme-en]: ./README.md "English"
[github-repo]: https://github.com/TheManta/RBtrust "RBtrust on GitHub"
[download-badge]: https://img.shields.io/badge/-Download-brightgreen
[download-link]: #installation "Download"
[discord-badge]: https://img.shields.io/badge/Discord-7389D8?logo=discord&logoColor=ffffff&labelColor=6A7EC2
[discord-invite]: https://discord.gg/bmgCq39 "Discord"
[rebornbuddy]: https://www.rebornbuddy.com/ "RebornBuddy"

## Supported Dungeons

### 2.0 - A Realm Reborn

  ❌ Lv. 15: Sastasha\
  ❌ Lv. 16: The Tam-Tara Deepcroft\
  ❌ Lv. 17: Copperbell Mines\
  ❌ Lv. 20: The Bowl of Embers\
  ❌ Lv. 24: The Thousand Maws of Toto-Rak\
  ❌ Lv. 28: Haukke Manor\
  ❌ Lv. 32: Brayflox's Longstop\
  ❌ Lv. 34: The Navel\
  ❌ Lv. 41: The Stone Vigil\
  ❌ Lv. 44: The Howling Eye\
  ❌ Lv. 50.1: Castrum Meridianum\
  ❌ Lv. 50.2: The Praetorium\
  ❌ Lv. 50.3: The Porta Decumana

### 5.0 - Shadowbringers

  ✔️ Lv. 71: Holminster Switch\
  ✔️ Lv. 73: Dohn Mheg\
  ✔️ Lv. 75: The Qitana Ravel\
  ✔️ Lv. 77: Malikah's Well\
  ✔️ Lv. 79: Mt. Gulg\
  ⚠️ Lv. 80.1: Amaurot\
  ❌ Lv. 80.2: The Grand Cosmos\
  ❌ Lv. 80.3: Anamnesis Anyder\
  ❌ Lv. 80.4: The Heroes' Gauntlet\
  ❌ Lv. 80.5: Matoya's Relict\
  ❌ Lv. 80.6: Paglth'an

### 6.0 - Endwalker

  ✔️ Lv. 81: The Tower of Zot\
  ✔️ Lv. 83: The Tower of Babil\
  ❌ Lv. 85: Vanaspati\
  ❌ Lv. 87: Ktisis Hyperboreia\
  ❌ Lv. 89.1: The Aitiascope\
  ❌ Lv. 89.2: The Mothercrystal\
  ✔️ Lv. 90.1 The Dead Ends\
  ❌ Lv. 90.2: Alzadaal's Legacy

## Installation

### Prerequisites

-   [RebornBuddy][rebornbuddy] with active license (paid)
-   (Optional) Better combat routine, such as [Magitek][magitek-discord] (free)
-   (Optional) Food plugin, such as [Gluttony][gluttony] (free)
-   (Optional) Self-repair plugin, such as [AutoRepairLisbeth][llama-plugins]

[magitek-discord]: https://discord.gg/rDsFbKr "Magitek Discord"
[llama-plugins]: https://github.com/nt153133/LlamaPlugins "AutoRepairLisbeth"
[gluttony]: https://github.com/domesticwarlord86/Gluttony "Gluttony"

### Automatic Setup (recommended)

Want **automatic installation and updates**, including prerequisites?

Install the [RepoBuddy][repobuddy] plugin -- `RBtrust` is configured by default!

[repobuddy]: https://github.com/Zimgineering/repoBuddy "repoBuddy"

#### Adding `RBtrust` to RepoBuddy

ℹ️ New RepoBuddy users can skip this step.

In case your repoBuddy config is too old or otherwise missing `RBtrust`, you can add it via repoBuddy's settings menu:

-   **Name:** `RBtrust`
-   **Type:** `Plugin`
-   **URL:** `https://github.com/TheManta/RBtrust.git/trunk`

## Usage

⚠️ Some classes may not survive certain bosses. ⚠️ If you can't clear even after tuning combat routine settings, try running the previous dungeon until you out-level and can skip the "difficult" one.

Each dungeon is handled by a separate OrderBot script that repeats the dungeon infinitely. Graduating to the next dungeon must be done manually by changing scripts.

To load a dungeon script:

1. Start RebornBuddy and set the BotBase dropdown to `Order Bot`.
2. Click `Load Profile` and navigate to `RebornBuddy\Plugins\RBtrust\Profiles`.
3. Select the `.xml` script for the desired dungeon.
4. Back in RebornBuddy, click `Start`.

## Troubleshooting

For live volunteer support, join the [Project BR Discord][discord-invite] channel `#rbtrust-issues`.

When asking for help, always include:

-   which OrderBot `.xml` script you loaded,
-   your class + Trust NPC list + scenario vs. avatar mode,
-   what you tried to do,
-   what went wrong,
-   **logs from the `RebornBuddy\Logs\` folder.**

No need to ask if anyone's around or for permission to ask -- just go for it!

### How can I stop dying to a certain boss?

Maybe you can, maybe you can't.

RBtrust has limited combat abilities, so some classes struggle with certain bosses. Some things to try:

-   Upgrade your gear and food to better survive big hits.
-   Adjust your combat routine to better use damage mitigation, heals, life-steal, etc.
-   Change class (not a real solution)

Worst case scenario: out-level and skip that dungeon by grinding the previous one, or kill the boss manually if needed for MSQ progression.

### When starting a script, why does it says the "Trust" plugin isn't installed?

The RBtrust folder might not have been fully extracted or put in the correct place.

Check your Plugins tab to see if the "Trust" plugin is listed and enable if it is. If the plugin isn't there, try closing RebornBuddy and cleanly [reinstalling][download] RBtrust.
