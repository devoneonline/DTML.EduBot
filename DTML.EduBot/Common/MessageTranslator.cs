﻿namespace DTML.EduBot.Common
{
    using Google.Cloud.Translation.V2;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class MessageTranslator
    {
        public const string DEFAULT_LANGUAGE = "en";
        private static TranslationClient client = TranslationClient.Create();

        /// <summary>
        /// Identify language for input text
        /// </summary>
        /// <param name="inputText">text whose language should be detected</param>
        /// <returns></returns>
        public static async Task<string> IdentifyLangAsync(string inputText)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(inputText))
                {
                    return DEFAULT_LANGUAGE;
                }

                var response = await client.DetectLanguageAsync(inputText);
                return response.Language;
            }
            catch
            {
                return DEFAULT_LANGUAGE;
            }
        }

        /// <summary>
        /// Translate text to english and send forward
        /// </summary>
        /// <param name="inputText">input text to be converted</param>
        /// <param name="inputLang">Language to which the inputText should be translated to </param>
        /// <returns>translated string</returns>
        public static async Task<string> TranslateTextAsync(string inputText, string inputLang = "en")
        {
            if (String.IsNullOrWhiteSpace(inputText))
            {
                return inputText;
            }

            try
            {
                var message = await client.TranslateTextAsync(inputText, inputLang);
                return message.TranslatedText;
            }
            catch
            {
                return inputText;
            }
        }

        public static async Task<IReadOnlyCollection<string>> TranslateTextAsync(IReadOnlyCollection<string> inputStrings, string targetLanguage)
        {
            if (inputStrings == null || !inputStrings.Any())
            {
                return new List<string>();
            }

            try
            {
                var results = await client.TranslateTextAsync(inputStrings, targetLanguage);
                var translatedStrings = results.Select(result => result.TranslatedText);
                return translatedStrings.ToList();
            }
            catch (Exception)
            {
                // TODO: Add logging.
                return inputStrings;
            }

        }
    }
}