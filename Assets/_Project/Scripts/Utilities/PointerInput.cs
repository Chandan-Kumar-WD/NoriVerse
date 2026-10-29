using UnityEngine;

namespace NoriVerse.Utilities
{
    public enum PointerPhase { None, Began, Held, Ended }

    /// <summary>One pointer (first touch, or the mouse in the editor) with the world position under it.</summary>
    public struct PointerState
    {
        public PointerPhase phase;
        public Vector2 world;
        public bool IsDown => phase == PointerPhase.Began || phase == PointerPhase.Held;
    }

    /// <summary>
    /// Reads touch or mouse through the Input Manager so every scene uses the same code path on device and in the editor.
    /// Call <see cref="Read"/> once per frame from a controller; it keeps no state of its own except the "was down" flag.
    /// </summary>
    public static class PointerInput
    {
        private static bool wasDown;
        private static bool suppressPress;
        public static void SuppressCurrentPress() => suppressPress = true;
        private static int lastFrame = -1;
        private static PointerState cached;
        private static PointerState? injected;

        /// <summary>Lets automated tests act as a finger. Pass phase None (or call <see cref="ClearInjected"/>) to stop.</summary>
        public static void InjectForTests(PointerPhase phase, Vector2 world) => injected = new PointerState { phase = phase, world = world };
        public static void ClearInjected() => injected = null;

        public static PointerState Read(Camera cam)
        {
            var pointer = ReadUnblocked(cam);
            if (suppressPress)
            {
                if (pointer.phase == PointerPhase.None || pointer.phase == PointerPhase.Ended) suppressPress = false;
                return default;
            }
            return NoriVerse.Gameplay.PizzaFlow.ExitConfirmation.IsOpen ? default : pointer;
        }

        public static PointerState ReadUnblocked(Camera cam)
        {
            if (injected.HasValue) return injected.Value;
            if (lastFrame == Time.frameCount) return cached;
            lastFrame = Time.frameCount;

            Vector2 screen;
            bool down;
            if (Input.touchCount > 0)
            {
                Touch t = Input.GetTouch(0);
                screen = t.position;
                down = t.phase != TouchPhase.Ended && t.phase != TouchPhase.Canceled;
            }
            else
            {
                screen = Input.mousePosition;
                down = Input.GetMouseButton(0);
            }

            PointerPhase phase = PointerPhase.None;
            if (down) phase = wasDown ? PointerPhase.Held : PointerPhase.Began;
            else if (wasDown) phase = PointerPhase.Ended;
            wasDown = down;

            Vector2 world = cam != null ? (Vector2)cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -cam.transform.position.z)) : screen;
            cached = new PointerState { phase = phase, world = world };
            return cached;
        }
    }
}
