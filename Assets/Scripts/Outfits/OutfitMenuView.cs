using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    /// <summary>
    /// Drives the outfit menu, Disco Elysium style: left panel lists the current player character's
    /// stats (base plus outfit modifiers), the center shows the character wearing the equipped
    /// outfit, and the right panel lists the wardrobe - click an outfit to equip it. Lives on the
    /// generated "Outfits" overlay scene (Tools > Game > Create Outfit Scene); the panel contents are
    /// rebuilt at runtime, so restyle the panels freely.
    /// </summary>
    public class OutfitMenuView : MonoBehaviour
    {
        private static readonly Color ButtonColor = new Color(0.15f, 0.17f, 0.22f, 1f);
        private static readonly Color EquippedColor = new Color(0.45f, 0.38f, 0.16f, 1f);
        private const string GainColor = "#8CD98C";
        private const string LossColor = "#E08A8A";

        [Tooltip("The outfit pool shown on the right.")]
        [SerializeField] private Wardrobe wardrobe;

        [Header("Left - stats")]
        [SerializeField, Tooltip("Rows are generated under this at runtime (needs a VerticalLayoutGroup).")]
        private RectTransform statsContainer;

        [Header("Center - preview")]
        [SerializeField] private Image previewImage;
        [SerializeField] private TMP_Text previewName;
        [SerializeField] private TMP_Text previewDescription;

        [Header("Right - outfit list")]
        [SerializeField, Tooltip("Outfit buttons are cloned from the template under this at runtime.")]
        private RectTransform outfitsContainer;

        [SerializeField, Tooltip("Disabled button cloned once per outfit (plus a None entry).")]
        private Button outfitButtonTemplate;

        private void OnEnable()
        {
            Outfits.Changed += OnOutfitChanged;
            RefreshAll();
        }

        private void OnDisable()
        {
            Outfits.Changed -= OnOutfitChanged;
        }

        private void OnOutfitChanged(CharacterData character) => RefreshAll();

        private void RefreshAll()
        {
            CharacterData current = PlayerCharacter.CurrentData;
            RefreshStats(current);
            RefreshPreview(current);
            RefreshList(current);
        }

        // ----- Left: stats -------------------------------------------------------------------

        private void RefreshStats(CharacterData character)
        {
            Clear(statsContainer);
            if (character == null)
            {
                CreateRow(statsContainer, "No player character.", 26, FontStyles.Bold);
                return;
            }

            AddCategory(character, StatCategory.Brain, Stat.Drive, Stat.Willpower, Stat.Observation, Stat.Empathy);
            AddCategory(character, StatCategory.Brawn, Stat.Vigor, Stat.Endurance, Stat.Agility, Stat.Technique);
            AddCategory(character, StatCategory.Beauty, Stat.Charm, Stat.Taunt, Stat.Bonhomie, Stat.Hostility);

            CreateRow(statsContainer, " ", 12, FontStyles.Normal); // spacer

            int masc = Outfits.EffectiveMasculine(character);
            int fem = Outfits.EffectiveFeminine(character);
            int mascMod = Outfits.MasculineModifier(character);
            int femMod = Outfits.FeminineModifier(character);
            CreateRow(statsContainer, $"Masc {masc}{ModifierSuffix(mascMod)}   Fem {fem}{ModifierSuffix(femMod)}", 24, FontStyles.Bold);
        }

        private void AddCategory(CharacterData character, StatCategory category, params Stat[] stats)
        {
            int total = Outfits.EffectiveCategory(character, category);
            int totalMod = Outfits.CategoryModifier(character, category);
            CreateRow(statsContainer, $"{category.ToString().ToUpperInvariant()}  {total}{ModifierSuffix(totalMod)}", 28, FontStyles.Bold);

            foreach (Stat stat in stats)
            {
                int value = Outfits.EffectiveStat(character, stat);
                int mod = Outfits.Modifier(character, stat);
                CreateRow(statsContainer, $"  {stat}  {value}{ModifierSuffix(mod)}", 24, FontStyles.Normal);
            }
        }

        // " (+1)" in green, " (-2)" in red, empty when unmodified.
        private static string ModifierSuffix(int modifier)
        {
            if (modifier == 0)
                return string.Empty;
            string color = modifier > 0 ? GainColor : LossColor;
            return $"  <color={color}>({(modifier > 0 ? "+" : "")}{modifier})</color>";
        }

        // ----- Center: preview ---------------------------------------------------------------

        private void RefreshPreview(CharacterData character)
        {
            OutfitData equipped = Outfits.GetEquipped(character);
            Sprite worn = Outfits.WornSprite(character);

            if (previewImage != null)
            {
                Sprite shown = worn != null ? worn : (character != null ? character.WorldSprite : null);
                previewImage.sprite = shown;
                previewImage.enabled = shown != null;
            }

            if (previewName != null)
                previewName.text = character != null ? character.DisplayName : "-";

            if (previewDescription != null)
                previewDescription.text = equipped != null
                    ? $"{equipped.DisplayName}\n<size=70%>{equipped.Description}</size>"
                    : "Nothing equipped.";
        }

        // ----- Right: outfit list ------------------------------------------------------------

        private void RefreshList(CharacterData character)
        {
            if (outfitsContainer == null || outfitButtonTemplate == null)
                return;

            // Rebuild from scratch each refresh - the list is tiny and this keeps state simple.
            for (int i = outfitsContainer.childCount - 1; i >= 0; i--)
            {
                Transform child = outfitsContainer.GetChild(i);
                if (child != outfitButtonTemplate.transform)
                    Destroy(child.gameObject);
            }

            OutfitData equipped = Outfits.GetEquipped(character);
            AddOutfitButton(character, null, equipped == null);
            if (wardrobe == null)
                return;
            foreach (OutfitData outfit in wardrobe.Outfits)
                if (outfit != null)
                    AddOutfitButton(character, outfit, equipped == outfit);
        }

        private void AddOutfitButton(CharacterData character, OutfitData outfit, bool isEquipped)
        {
            Button button = Instantiate(outfitButtonTemplate, outfitsContainer);
            button.gameObject.SetActive(true);

            // The scroll content's VerticalLayoutGroup controls child height, so a plain button would
            // collapse to zero (invisible and unclickable) without a preferred height. Give it one.
            LayoutElement layout = button.GetComponent<LayoutElement>();
            if (layout == null)
                layout = button.gameObject.AddComponent<LayoutElement>();
            layout.minHeight = 72f;
            layout.preferredHeight = 72f;

            var label = button.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = outfit != null ? outfit.DisplayName : "None";

            var image = button.GetComponent<Image>();
            if (image != null)
                image.color = isEquipped ? EquippedColor : ButtonColor;

            button.onClick.AddListener(() => Outfits.Equip(character, outfit));
        }

        // ----- helpers -------------------------------------------------------------------------

        private static void Clear(RectTransform container)
        {
            if (container == null)
                return;
            for (int i = container.childCount - 1; i >= 0; i--)
                Destroy(container.GetChild(i).gameObject);
        }

        private static void CreateRow(RectTransform parent, string text, int fontSize, FontStyles style)
        {
            if (parent == null)
                return;
            var go = new GameObject("Row", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.richText = true;
            tmp.raycastTarget = false;
        }
    }
}
