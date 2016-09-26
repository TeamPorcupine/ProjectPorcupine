#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using UnityEngine;
using UnityEngine.UI;

namespace ProjectPorcupine.Localization
{
    /**
     * <summary>
     * This class will automatically translate texts it is attached to.
     * </summary>
     */
    [AddComponentMenu("Localization/Text Localizer"), RequireComponent(typeof(Text))]
    public class TextLocalizer : MonoBehaviour
    {
        // The values that will be inserted with string.Format(..).
        // The array can have a length of 0 without any issue.
        public string[] formatValues;

        // Should the text immediately localize in the Start() method?
        // In most cases, that is what you'll want. (Thats why it defaults to true)
        public bool localizeInStart = true;

        // The text this localizer is localizing.
        // Hide this in the inspector, due to it being useless there.
        [HideInInspector]
        public Text text;

        // The string the text had before localizing.
        // Hide this in the inspector, due to it being useless there.
        [HideInInspector]
        public string defaultText;

        // The language that was selected last frame.
        // This is used for auto-translating as soon as the user switches language to something else.
        private string lastLanguage;

        /// <summary>
        /// Updates the text with last formatValues.
        /// </summary>
        public void UpdateText()
        {
            text.text = LocalizationTable.GetLocalization(defaultText, formatValues);
        }

        /**
         * <summary>
         * Updates the text with the given formatValues.
         * </summary>
         * <para>
         * params string[] formatValues: The values that should be inserted into {0}, {1}, etc.
         * </para>
         */
        public void UpdateText(params string[] formatValues)
        {
            lastLanguage = LocalizationTable.currentLanguage;
            this.formatValues = formatValues;
            text.text = LocalizationTable.GetLocalization(defaultText, formatValues);
        }

        /**
         * <summary>
         * Updates the text with the given text and formatValues.
         * 
         * This method is called differently due to params in the UpdateText(..) method
         * that might confuse the compiler.
         * </summary>
         * 
         * <para>
         * string text: The *new* defaultText.
         * </para>
         * <para>
         * params string[] formatValues: The values that should be inserted into {0}, {1}, etc.
         * </para>
         */
        public void UpdateTextCustom(string text, params string[] formatValues)
        {
            lastLanguage = LocalizationTable.currentLanguage;
            this.formatValues = formatValues;
            defaultText = text;
            this.text.text = LocalizationTable.GetLocalization(text, formatValues);
        }

        private void Awake()
        {
            // Get the Text component on this GameObject.
            // This can't throw errors, due to the RequireComponent.
            text = GetComponent<Text>();

            // Set the defaultText to what the text currently has.
            defaultText = text.text;
        }

        private void Start()
        {
            // Set the last language to what's currently selected.
            lastLanguage = LocalizationTable.currentLanguage;

            // Register callback, used when new localization files are downloaded
            LocalizationTable.CBLocalizationFilesChanged += UpdateText;

            if (localizeInStart)
            {
                // Update the text content, if the text should localize immediately.
                UpdateText(formatValues);
            }
        }

        private void Update()
        {
            // Check if the language has changed.
            if (lastLanguage != LocalizationTable.currentLanguage)
            {
                // The language has changed, apply changes to the text.
                lastLanguage = LocalizationTable.currentLanguage;
                UpdateText(formatValues);

                // Rescales text component of the prefab button to fit everything
                TextScaling.ScaleAllTexts();
            }
        }
    }
}
