# Meteor Watch — Lab 4

A one-life space shooter refactored into small gameplay components. Destroy five regular meteors to spawn a large meteor; large meteors take five laser hits. Every subsequent group of five regular kills spawns another large meteor.

## Open and play

1. Open this folder in **Unity 6000.2.8f1** (the version already used by the supplied project).
2. Let Package Manager restore dependencies, including **Input System 1.14.2** and **Cinemachine 3.1.7**.
3. Open `Assets/Scenes/Week5Lab.unity` and press Play. This is also the first and only enabled build scene.

Move with **WASD**, **arrow keys**, or a gamepad's **left stick / D-pad**. Hold **Space**, **gamepad A / South**, or **right trigger** to fire. After a collision, press **R** or **gamepad Start** to restart.

Movement accelerates smoothly and clamps to the play area. Diagonal movement is normalized. The old wraparound was removed so Cinemachine can follow continuously. Meteors use the Week 4 orbit instead of falling: they circle the ship, speed up as they get closer, and turn to face it. Regular meteors join a radius of 5, then spiral inward at 0.7 units/sec. The large meteor joins 7.5 and closes at 0.35 units/sec. Leave one alone and it reaches the ship. Fire cooldown is 0.25 seconds. Shots travel upward, so line the ship up underneath a meteor. One meteor contact ends the run and stops spawning and movement.

## SOLID design

- **Single responsibility:** `ShooterInput` owns input actions; `ShipMotor` moves the rigidbody; `ShipWeapon` handles firing/cooldown; `Health` implements damage rules; `MeteorSpawner` creates enemies; `GameSession` owns progression; `GameHud` presents state; `ShooterCamera` handles camera feedback. `GameManager` connects the components.
- **Open/closed:** attach another `MeteorMovement` implementation to a prefab to change enemy motion without editing damage, spawning or scoring. `FallingMeteorMovement` is the straight-line alternative; `OrbitMeteorMovement` is the Week 4 orbit. Additional `IDamageable` targets work with the existing laser. Session events allow additional destruction effects without changing combat.
- **Liskov substitution:** `BigMeteor` uses the same `Meteor` contract and collision/damage implementation; its five-hit health is prefab data. The small subclass preserves the original prefab script identity.
- **Interface segregation:** `IShipInput` exposes only movement and fire; `IDamageable` exposes only damage; `IGameStatus` exposes the read-only state and destruction event needed by presentation and movement.
- **Dependency inversion:** ship coordination consumes `IShipInput`, lasers consume `IDamageable`, and HUD/camera/movement consume `IGameStatus`. The composition root supplies concrete dependencies. Gameplay rules have no dependency on Cinemachine, UI or the Input System.

No gameplay script uses `UnityEngine.Input`, string-based invokes, tags for damage routing, or `GameObject.Find`. Input actions are cloned per session and enabled/disabled with component lifetime. A projectile is consumed before additional trigger callbacks can damage another target, and meteor destruction is guarded against duplicate scoring.

## Input and camera authoring

`Assets/Settings/ShooterControls.inputactions` contains the Gameplay action map with Move, Fire and Restart. Active Input Handling is **Input System Package (New)**.

The saved scene contains a **Cinemachine Brain** on Main Camera and a **Ship Tracking Camera** with **Cinemachine Camera**, **Position Composer**, **Basic Multi Channel Perlin**, and `ShooterCamera`. At startup, the spawned ship becomes the tracking target. A four-unit upward offset keeps approaching meteors in view. Orthographic size eases from 8 to 10.5 while any large meteor exists. Destruction triggers a short decaying shake, stronger for a large meteor. Escaping meteors do not score or shake. Zoom returns when the final large meteor is destroyed or leaves the play area.

`Assets/Settings/DestructionNoise.asset` holds the noise profile. World-space stars make camera motion visible. Tune camera values on the camera rig, movement values on the prefabs, and spawn interval on GameManager's Meteor Spawner component.

**Tools → Space Shooter → Configure Project** rebuilds the lab's saved component wiring and default tuning. It is an authoring utility, not a step required before playing; it overwrites those defaults when deliberately invoked.

## Verification and recording

Use **Window → General → Test Runner**. EditMode tests cover five-hit boss health, one-hit regular health, boss thresholds, escaping bosses and game-over rules. PlayMode tests exercise actual Input System events, physics collisions, camera following, noise decay, boss zoom, duplicate hits and restart.

Build a development player using **Tools → Space Shooter → Build Windows Demo**. Normal launch is human-controlled. For a repeatable automated gameplay recording, launch the development build with:

```powershell
.\Builds\Windows\MeteorWatch.exe -screen-width 1280 -screen-height 720 -screen-fullscreen 0 -demoCapture "C:\path\to\frames"
```

The opt-in recorder drives a virtual gamepad through the normal input bindings, captures 30 FPS PNG frames, and writes a result summary to `capture.txt`. It aims at meteors, attempts a boss kill, then flies into an enemy to demonstrate the one-life rule and restart. It does not award kills or inject damage. Encode frames with FFmpeg:

```text
ffmpeg -framerate 30 -i frame-%05d.png -c:v libx264 -pix_fmt yuv420p -crf 20 gameplay.mp4
```

## Week 4 enemy movement (quiz points)

Lab 3's `EnemyController` orbits a target, scales speed by squared distance (closer is faster), and faces the target with `Dot` / `Cross` / `Atan2`. `LookAt`, `LookRotation`, `RotateAround`, `RotateTowards`, and `FromToRotation` are not used. That motion is now `OrbitMotion`, driven by `OrbitMeteorMovement`.

The original lab orbited on the horizontal XZ plane. This shooter plays on XY, so the same three solutions rotate around Z:

- **Quaternion offset:** `Quaternion.Euler(0, 0, degrees) * offset`
- **Cross tangent:** `Cross(forward, radial)`, then `MoveTowards` the preferred radius
- **TRS matrix:** `Matrix4x4.TRS` around the ship

Each spawn cycles those solutions and flips orbit direction. Regular meteors use the lab speeds (6.5 near, 2.2 far) and join a radius of 5, then spiral into the ship at 0.7 units/sec. The large meteor joins 7.5 and closes at 0.35 units/sec, slow enough that five upward shots can connect while the camera is zoomed out. A meteor that starts on top of the ship does not teleport out to the orbit, so contact still costs the one life.

The course quiz form itself is submitted in the class site. These points replace that quiz's grading; the behavior above is the response.

## Submission details still needed

- **Team (3–4 people):** add the actual teammate names before submission.
- **Public GitHub repository:** publish this folder's `Assets`, `Packages`, `ProjectSettings`, `.gitignore`, README and recording. The ignore file excludes Unity caches and local builds.
- **Video:** include the final gameplay MP4 with the submission or a link to it.

## References

- [Cinemachine tracking scenarios](https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/3d-tracking-scenarios.html)
- [Cinemachine noise setup](https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/setup-apply-noise.html)
- [Input System actions](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/Actions.html)

Sprites and the original four prefabs come from the supplied prototype.
