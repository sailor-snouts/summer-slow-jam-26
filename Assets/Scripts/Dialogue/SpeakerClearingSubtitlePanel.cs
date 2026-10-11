using PixelCrushers.DialogueSystem;

namespace Game
{
    // Drop-in replacement for StandardUISubtitlePanel (same settings): keeps stacking lines while the same
    // character talks, but clears the log as soon as a different character speaks. Only has an effect
    // when Accumulate Text is on.
    public class SpeakerClearingSubtitlePanel : StandardUISubtitlePanel
    {
        private int lastSpeakerId = -1;

        protected override void SetSubtitleTextContent(Subtitle subtitle)
        {
            // Lines with no text (sequence-only nodes) don't count as a change of speaker.
            if (accumulateText && subtitle != null && subtitle.speakerInfo != null
                && subtitle.formattedText != null && !string.IsNullOrEmpty(subtitle.formattedText.text))
            {
                int speaker = subtitle.speakerInfo.id;
                if (speaker != lastSpeakerId)
                    ClearText();
                lastSpeakerId = speaker;
            }

            base.SetSubtitleTextContent(subtitle);
        }

        public override void ClearText()
        {
            base.ClearText();
            lastSpeakerId = -1;
        }
    }
}
