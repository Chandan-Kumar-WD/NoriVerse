# Repository cleanup and iOS preparation — 2026-10-28

The project had no Git repository. Removed material is recoverable in the sibling `NoriVerse_Archive/2026-10-28-device-prep/` folder, with a manifest of original paths. Original reference videos remain in Downloads. Only regenerable extracted video frames were deleted.

## Cleanup

- Audited dependencies of all eight enabled scenes and removed 41 unused runtime art files (23.9 MiB) with their metadata from Assets.
- Moved the rejected articulated splash code, approved-but-unused art and its generation scripts outside the project. The original splash and smooth pose transitions remain active.
- Archived historical captures/logs and generated IDE project files. Retained current art source files, generation/processing tools, reference analysis, scene builders and meaningful tests.
- Deleted about 1,251 MiB of extracted video frames, which can be recreated from the original videos.
- Removed the unused procedural star method and stale baking/reaction fields; removed Rider/Visual Studio integration packages. Kept engine modules under Unity's normal code stripping rather than guessing which editor dependencies were removable.
- Pruned import manifests to current art. Added `.gitignore` for generated caches, builds, reports, screenshots and the local MCP configuration.
- Test screenshot output is now opt-in through `NORIVERSE_CAPTURE_FRAMES=1`; required in-memory render assertions still run.

## Verification

All eight scenes rebuild successfully. All 18 PlayMode tests pass, with 0 failures, including a second run after the iOS texture settings change. Local results: `BuildReports/cleanup-final-tests.xml`. Build log: `BuildReports/cleanup-build.log`.

Unity's installed iOS support successfully exported an Xcode project at `Builds/iOS-Device/Unity-iPhone.xcodeproj`. Native compilation succeeded using Xcode, the iPhoneOS SDK, generic iOS destination, Release configuration and signing disabled. Final log: `BuildReports/xcode-device-optimized-build.log`.

`IOSBuildTools.cs` adds separate simulator/device export commands. These set the appropriate SDK, IL2CPP, landscape iPhone/iPad and minimum iOS 15. `MobileFrameRate.cs` requests 60 FPS on mobile. The bundle identifier defaults to `com.noriverse.game`; use an identifier registered to the user's Apple development team for signed device deployment.

The export is a Unity development build with debugging enabled, compiled by Xcode with native Release optimization. The unsigned arm64 app is suitable for size measurement; a real-device install still requires the user's signing team and an available device. The separate simulator export succeeded at `Builds/iOS-Simulator/Unity-iPhone.xcodeproj`; it was not used for the reported device size and was not installed/run in Simulator.

## App size

Final unsigned arm64 iPhone/iPad app bundle: **145.18 MB (138.46 MiB)**. Before mobile texture compression it was 365.20 MB, a 60.2% reduction. High-quality ASTC 4×4 iOS overrides preserve sprite dimensions/alpha while reducing texture storage; CPU-readable sauce remains RGBA32. This is a Unity development export compiled with Xcode native Release optimization, not an App Store thinned release.

The exact built app bundle measurement is in `BuildReports/app-size.json`, excluding Xcode source files, DerivedData, symbol files outside the app and the recovery archive. Actual device disk allocation and a future App Store thinned release may differ. No physical device was available for installation, so an on-device allocated-size measurement is not claimed.

See the root README for export/run instructions and archive recovery.
