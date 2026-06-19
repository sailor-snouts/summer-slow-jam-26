using UnityEngine;

namespace Game
{
    /// <summary>
    /// Simple NPC movement: amble in a random direction for a while, pause, then pick a new
    /// direction — repeat. It only sets <see cref="Mover.MoveDirection"/>, so it's a drop-in
    /// alternative to <see cref="PlayerController"/>: same Mover, a different "brain".
    ///
    /// Move time, pause time and speed each follow the same pattern: a range (min..max) sets the
    /// bounds, and a distribution curve maps a uniform random 0..1 to where in that range the
    /// value lands (curve Y: 0 = min, 1 = max). A linear curve = uniform across the range.
    /// Optionally restricts wandering to a rectangle around the start position.
    /// </summary>
    [RequireComponent(typeof(Mover))]
    [DisallowMultipleComponent]
    public class Wander : MonoBehaviour
    {
        // These are laid out by WanderEditor (range slider + its curve), so no [Header]s here.
        [SerializeField, Min(0f)] private Vector2 moveTimeRange = new Vector2(0f, 3f);
        [SerializeField, Tooltip("X = random 0..1 → where in Move Time the value lands (Y: 0 = min, 1 = max).")]
        private AnimationCurve moveTimeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [SerializeField, Min(0f)] private Vector2 pauseTimeRange = new Vector2(0f, 2f);
        [SerializeField, Tooltip("X = random 0..1 → where in Pause Time the value lands (Y: 0 = min, 1 = max).")]
        private AnimationCurve pauseTimeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        // Speed is a fraction (0..1) of the Mover's max speed.
        [SerializeField] private Vector2 speedRange = new Vector2(0f, 1f);
        [SerializeField, Tooltip("X = random 0..1 → where in Speed the value lands (Y: 0 = min, 1 = max).")]
        private AnimationCurve speedCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Header("Area restriction")]
        [SerializeField, Tooltip("Keep the NPC within a rectangular area around its start position.")]
        private bool restrictArea = false;

        [SerializeField, Tooltip("Size (width x height) of the allowed area.")]
        private Vector2 areaSize = new Vector2(6f, 6f);

        [SerializeField, Tooltip("Shift the area's center from the start position.")]
        private Vector2 areaCenterOffset = Vector2.zero;

        [SerializeField, Min(0f), Tooltip("Look-ahead in seconds of travel, used as a skin: if moving at the " +
                                          "current speed would leave the area within this time, the NPC stops and wanders a new direction.")]
        private float edgeLookahead = 0.2f;

        private Mover mover;
        private Vector2 home;
        private Vector2 direction;
        private float timer;
        private bool moving;

        private Vector2 AreaCenter => home + areaCenterOffset;

        private void Awake()
        {
            mover = GetComponent<Mover>();
            home = transform.position; // the area is centered here
        }

        // Start paused, then begin the move/pause cycle.
        private void OnEnable() => StartPause();

        private void Update()
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                if (moving)
                    StartPause();
                else
                    StartMove();
            }

            // If the next step would carry it out of the area, abort this leg: pause now, and a
            // fresh leg (new random direction) starts after the pause — i.e. run the cycle again.
            if (moving && restrictArea && WillExitArea())
                StartPause();

            mover.MoveDirection = moving ? direction : Vector2.zero;
        }

        private void StartMove()
        {
            // A random unit direction (cosmetic randomness — not the seeded dice RNG).
            float angle = Random.Range(0f, Mathf.PI * 2f);
            Vector2 unit = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            // Encode speed as the direction's length (0..1); the Mover scales velocity by it.
            float speed = Mathf.Clamp01(SampleRange(speedRange, speedCurve));
            direction = unit * speed;

            timer = SampleRange(moveTimeRange, moveTimeCurve);
            moving = true;
        }

        private void StartPause()
        {
            direction = Vector2.zero;
            timer = SampleRange(pauseTimeRange, pauseTimeCurve);
            moving = false;
        }

        // Lerp across [range.x, range.y], using the curve to shape the distribution of a uniform sample.
        private static float SampleRange(Vector2 range, AnimationCurve curve)
        {
            float t = Mathf.Clamp01(curve.Evaluate(Random.value));
            return Mathf.Lerp(range.x, range.y, t);
        }

        // True if, at the current speed, the NPC would step outside the area within the look-ahead
        // "skin". Only guards the inside→outside crossing, so if it's somehow already outside
        // (e.g. shoved by physics) it isn't trapped — a later leg wanders it back in.
        private bool WillExitArea()
        {
            Vector2 pos = transform.position;
            Vector2 half = areaSize * 0.5f;
            Vector2 min = AreaCenter - half;
            Vector2 max = AreaCenter + half;

            bool insideNow = pos.x >= min.x && pos.x <= max.x && pos.y >= min.y && pos.y <= max.y;
            if (!insideNow)
                return false;

            // Skin = speed × look-ahead: direction.magnitude is the speed fraction and Mover.MoveSpeed
            // is the full speed, so this is the real velocity projected over edgeLookahead seconds.
            Vector2 predicted = pos + direction * mover.MoveSpeed * edgeLookahead;

            return predicted.x < min.x || predicted.x > max.x
                || predicted.y < min.y || predicted.y > max.y;
        }

        private void OnDrawGizmosSelected()
        {
            if (!restrictArea)
                return;

            Vector2 center = Application.isPlaying ? AreaCenter : (Vector2)transform.position + areaCenterOffset;
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.6f);
            Gizmos.DrawWireCube(center, areaSize);
        }
    }
}
