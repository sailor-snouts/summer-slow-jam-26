# Self-hosted runner fleet

Unity 6 Personal licenses can't be activated on cloud CI (account-token licensing — there's no `.ulf` to hand a hosted runner). So the Unity jobs run on a **self-hosted runner** that uses this machine's already-activated Unity. This folder sets up and manages those runners.

## The one GitHub constraint that shapes everything

Self-hosted runners are shared at the **organization** level, but **not across personal-account repos** — a `roboparker/*` repo can only use a runner registered to that exact repo, while every repo in the `sailor-snouts` org can share one org runner. One physical machine hosts many runners, so the model is:

- **One `org` runner** → covers every repo in `sailor-snouts`.
- **One `repo` runner per personal repo** that needs Unity CI (this template, etc.).

The fleet is declared in [`runners.yaml`](runners.yaml) — adding a repo is one list entry + `Install-Runners.ps1 -Name <it>`. (If you'd rather have a single shared runner, move the Unity repos into the `sailor-snouts` org and keep only the org entry.)

## The config: `runners.yaml`

```yaml
installRoot: C:\r          # keep SHORT (see MAX_PATH note below)
labels: [windows, unity]
runners:
  - name: unity-starter-template          # GitHub registration name
    folder: ust                            # short on-disk dir under installRoot
    type: repo                             # repo | org
    target: roboparker/unity-starter-template
    autostart: true                        # launch at your logon?
  - name: sailor-snouts
    folder: ss
    type: org
    target: sailor-snouts
    autostart: false
```

## Setup

Prerequisites: Unity installed via Hub, the [`gh` CLI](https://cli.github.com) authenticated (`gh auth login`) with admin on the targets, and an **elevated** PowerShell. From an **Administrator** PowerShell:

```powershell
cd runners
powershell -NoProfile -ExecutionPolicy Bypass -File .\Install-Runners.ps1            # all runners
powershell -NoProfile -ExecutionPolicy Bypass -File .\Install-Runners.ps1 -Name ust  # just one
```

`Install-Runners.ps1` downloads the runner (cached), registers each one at its short path with the shared labels, enables OS + git long paths, and **applies `autostart`** — creating an at-logon task for `autostart: true` runners and removing it for `false`. Re-run it any time after editing the YAML (e.g. to flip an `autostart`).

**Why a logon task, not a Windows service:** Unity 6 ties the Personal license to the **Windows user** signed into Unity — a service runs as `NETWORK SERVICE` (no license; builds fail with *"No valid Unity Editor license found"*), and a **Microsoft-account** login can't be a service logon at all. So a runner runs in *your* session, in its own **titled console window** (`Runner <name>`) so you can tell them apart. Keep Unity Hub signed in on this account.

**Surviving unattended reboots:** a logon task needs someone logged in — automatic on a machine you use daily. For an unattended reboot (3am Windows Update) enable **auto-login** via `netplwiz` (untick *"Users must enter a user name and password"*). Security trade-off: anyone powering on the machine lands in your session.

## Managing the fleet

`Manage-Runners.ps1 <action> -Name <name|all>` — `-Name` targets one runner (by name or folder); omit it for all.

```powershell
./Manage-Runners.ps1                          # status: which is which, running?, autostart?
./Manage-Runners.ps1 start   -Name ust        # start one in a titled window
./Manage-Runners.ps1 stop    -Name sailor-snouts
./Manage-Runners.ps1 restart                  # all
```

You don't need them all running — start only the ones you need, stop the rest before a heavy local Unity session. Closing a runner's titled console window also stops it (autostart relaunches it next logon). Each runner checks out into its own `_work` dir, isolated from your dev clone.

**Why the install path is so short (`C:\r\ust`):** Unity's UI Toolkit importer ignores Windows `LongPathsEnabled`, so deep package files (e.g. `com.unity.2d.tooling`) under `<installRoot>\<folder>\_work\<repo>\<repo>\Library\PackageCache\...` exceed the 260-char `MAX_PATH` and **player builds fail** with `DirectoryNotFoundException`. The tiny `installRoot` + short `folder` keep the total under the limit. Don't lengthen them.

## How the jobs use it

The Unity jobs in [`.github/workflows/ci.yml`](../.github/workflows/ci.yml) target `runs-on: [self-hosted, windows, unity]` and are **manual-dispatch only** (Actions tab ▸ CI ▸ Run workflow), because a personal machine isn't always online and queued jobs would otherwise hang. They call [`Run-Unity.ps1`](../.github/scripts/Run-Unity.ps1), which finds the editor matching `ProjectVersion.txt` and runs tests / WebGL build / solution generation in batch mode — no Unity secrets anywhere.

> This tooling lives in the template for convenience, but it manages runners for *many* repos. If the fleet grows, lift `runners/` into a shared infra repo (e.g. a `sailor-snouts/.github` or dedicated `ci` repo) and point the config there.
