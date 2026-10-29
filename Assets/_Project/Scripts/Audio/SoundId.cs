namespace NoriVerse.Audio
{
    /// <summary>Every game sound. Names match the files in Assets/_Project/Audio (snake_case there, PascalCase here).</summary>
    public enum SoundId
    {
        None,
        // Music
        MusSplash, MusGameplayLoop,
        // Nori's voice
        VoBrand, VoGiggle, VoHmmWorried, VoYawn, VoOoh, VoYay, VoWow, VoMmmAnticipate, VoNom, VoChew, VoYummy, VoHumHappy,
        // Effects
        UiTap, DishLockedWobble, SceneWhoosh, ItemPopIn, ItemPickup, DropPlopBowl, DropReturn, WhiskFlyIn, WhiskStirLoop,
        SuccessArpeggio, RollingPinLoop, DoughCutTrace, CutCompleteSparkle, SauceSquishLoop, CheeseSprinkle, SortCorrectDing,
        ToppingPlacePop, ConfirmButton, OvenRackSlide, OvenPowerClick, OvenBakeLoop, OvenDoneDing, StarDing, ConfettiPop,
    }
}
