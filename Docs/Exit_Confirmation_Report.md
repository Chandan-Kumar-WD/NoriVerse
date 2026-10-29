# Exit confirmation

Implemented the supplied confirmation design for all six gameplay screens: Cooking, Dough, PizzaPrep, ToppingSort, PizzaDecorate and PizzaBake. The kitchen remains the home destination and retains its previously requested button-free top area.

Back now opens a pink and cream question panel over a dimmed activity. The green check returns home; either X closes the panel. Outside touches do nothing. The activity pauses while the panel is open, retains its current state on cancellation, and cannot receive touches through the dialog. Dismissal consumes the current press. The panel scales to fit the camera view. It uses shared runtime UI shapes and existing font resources, with no additional image generation or credits.

Implementation: `Assets/_Project/Scripts/Gameplay/PizzaFlow/ExitConfirmation.cs`, shared `SceneBackButton`, pointer input gate, and the Cooking scene builder. The oven reward now retains access to its back button. The Cooking scene has been rebuilt with the missing navigation handler.

Validation covers all six screens, both cancellation controls, confirmation to Kitchen, paused time, blocked gameplay input, outside touches, four aspect ratios and the existing full recipe regressions. A rendered screenshot is saved in `Docs/playmode_captures/exit_confirmation/cooking_confirmation.png`. Test results: `scratchpad/exit_results_final.xml`; log: `scratchpad/exit_tests_final.log`. Physical-device validation remains pending.

Final result: all 18 PlayMode tests passed, zero failures. Unity was relaunched from Splash in Play Mode for user review.
