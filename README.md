# NoriVerse

Unity 6.6 project (`6000.6.7f1`). The complete pizza flow starts in Splash and continues through Kitchen, batter, dough, preparation, topping sorting, decoration, baking, rewards and feeding Nori.

## Project contents

Agents: read `AGENTS.md`, `Docs/AGENT_WORKFLOW.md`, and [the future artwork and animation enhancement plan](Docs/Animation_And_Art_Enhancement_Plan.md). The user provides creative feedback; the development model handles implementation, asset integration, testing, and technical decisions.

- `Assets/_Project`: active runtime art, scenes, scripts, shader and tests.
- `Tools/ArtPipeline`: source-art generation and processing tools. These do not ship in the app; keep source art for future editing.
- `Docs`: current workflow and reference analysis.
- `BuildReports`: local validation/audit output, ignored by Git.
- `Builds`: generated Xcode projects, ignored by Git.

Use **NoriVerse → Build All Scenes** only when intentionally rebuilding scene files from their builders. Use **NoriVerse → Run Splash** to play the full game.

## Xcode / iOS

Unity Hub must have iOS Build Support installed. Open the project with the Unity version above.

- **NoriVerse → iOS → Export for Simulator** writes `Builds/iOS-Simulator/Unity-iPhone.xcodeproj` using the installed simulator SDK. This is a development export with debugging enabled.
- **NoriVerse → iOS → Export for Device** writes `Builds/iOS-Device/Unity-iPhone.xcodeproj` using the device SDK.

Open the appropriate generated Xcode project, select `Unity-iPhone` and the intended destination, then Run. For a physical iPhone/iPad, select your Apple development team under Signing & Capabilities and use a bundle identifier registered to that team. The default identifier is `com.noriverse.game`; change it in `IOSBuildTools.cs` if needed. No signing team or account credentials are stored in the project.

The game supports landscape iPhone/iPad, minimum iOS 15, IL2CPP, and requests 60 FPS on mobile. Simulator and device exports are separate because their SDKs/libraries differ. The most recent export selects its SDK in Unity's saved Player Settings.

## Tests

Run PlayMode tests in Unity Test Runner. Tests render the game in memory where needed, but do not write large screenshot sets by default. Set `NORIVERSE_CAPTURE_FRAMES=1` when launching Unity to save captures under `Docs/playmode_captures/`.

Latest cleanup results and export status are recorded in `BuildReports/` and `Docs/Device_Prep_Report.md`.

## Cleanup recovery

The project was not a Git repository at cleanup time. Removed art, rejected code experiments, historical captures and old logs were moved to the sibling archive:

`../NoriVerse_Archive/2026-10-28-device-prep/`

The archive has a manifest of original paths. Its contents are outside Unity's project and do not affect exports. Only reproducible extracted video-frame folders were deleted; the original videos remain in Downloads. Historic reports may reference captures now stored in that archive.
