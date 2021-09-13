using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Translation;

namespace TranslatorConsoleApp
{
    class Program
    {
        public static async Task TranslationWithMicrophoneAsync_withLanguageDetectionEnabled(string toLanguage, string[] fromLanguages)
        {
            try
            {
                var toLanguageCode = ToLanguageValidation(toLanguage);
                var v2EndpointInString = String.Format("wss://{0}.stt.speech.microsoft.com/speech/universal/v2", "westeurope");
                var v2EndpointUrl = new Uri(v2EndpointInString);
                var translationConfig = SpeechTranslationConfig.FromEndpoint(v2EndpointUrl, "970994f6d00443dfa3b01ba78e016744");
                translationConfig.SpeechRecognitionLanguage = fromLanguages[0];
                translationConfig.AddTargetLanguage(toLanguage);
                translationConfig.SetProperty(PropertyId.SpeechServiceConnection_ContinuousLanguageIdPriority, "Accuracy");
                translationConfig.SetProperty(PropertyId.SpeechServiceConnection_SingleLanguageIdPriority, "Accuracy");
                var autoDetectSourceLanguageConfig = AutoDetectSourceLanguageConfig.FromLanguages(fromLanguages);
                using (var recognizer = new TranslationRecognizer(translationConfig, autoDetectSourceLanguageConfig))
                {
                    // Subscribes to events.
                    recognizer.Recognizing += (s, e) =>
                    {
                        Console.WriteLine($"RECOGNIZING Text={e.Result.Text}");
                        var detectionResult = e.Result.Properties.GetProperty(PropertyId.SpeechServiceConnection_AutoDetectSourceLanguageResult);
                        foreach (var element in e.Result.Translations)
                        {
                            Console.WriteLine($"TRANSLATING Translated from " +
                                $"'{detectionResult}' into '{element.Key}': {element.Value}");
                        }
                    };

                    recognizer.Recognized += (s, e) =>
                    {
                        if (e.Result.Reason == ResultReason.TranslatedSpeech)
                        {
                            Console.WriteLine($"RECOGNIZED Text={e.Result.Text}");
                            var detectionResult = e.Result.Properties.GetProperty(PropertyId.SpeechServiceConnection_AutoDetectSourceLanguageResult);
                            foreach (var element in e.Result.Translations)
                            {
                                Console.WriteLine($"TRANSLATED Translated from " +
                                    $"'{detectionResult}' into '{element.Key}': {element.Value}");
                            }
                        }
                        else if (e.Result.Reason == ResultReason.RecognizedSpeech)
                        {
                            Console.WriteLine($"RECOGNIZED: Text={e.Result.Text}");
                            Console.WriteLine($"Speech not translated.");
                        }
                        else if (e.Result.Reason == ResultReason.NoMatch)
                        {
                            Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                        }
                    };

                    recognizer.Canceled += (s, e) =>
                    {
                        Console.WriteLine($"CANCELED: Reason={e.Reason}");

                        if (e.Reason == CancellationReason.Error)
                        {
                            Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                            Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                            Console.WriteLine($"CANCELED: Did you update the subscription info?");
                        }
                    };

                    recognizer.SessionStarted += (s, e) =>
                    {
                        Console.WriteLine("\nSession started event.");
                    };

                    recognizer.SessionStopped += (s, e) =>
                    {
                        Console.WriteLine("\nSession stopped event.");
                    };
                    Console.WriteLine("Say something...");
                    await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

                    do
                    {
                        Console.WriteLine("Press Enter to stop");
                    } while (Console.ReadKey().Key != ConsoleKey.Enter);
                    await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
        }


        static async Task Main(string[] args)
        {
            //added this so it can show russian language
            Console.OutputEncoding = Encoding.Unicode;
            var toLanguage = "ru";
            var fromLanguages = new string[] { "en-US", "ar-EG", "ru-RU" };
            await TranslationWithMicrophoneAsync_withLanguageDetectionEnabled(toLanguage, fromLanguages);
        }

        public static (string Language, string Code) ToLanguageValidation(string toLanguage)
        {
            if (string.IsNullOrWhiteSpace(toLanguage)) throw new ArgumentNullException("Selected language is null or white space.");
            var allLanguages = new List<(string Language, string Code)>{
            ("Afrikaans","af"), ("Arabic","ar"), ("Bangla","bn"), ("Bosnian (Latin)","bs"), ("Bulgarian","bg"),
                ("Cantonese (Traditional)","yue"), ("Catalan","ca"), ("Chinese Simplified","zh-Hans"), ("Chinese Traditional","zh-Hant"),
                ("Croatian","hr"), ("Czech","cs"), ("Danish","da"), ("Dutch","nl"), ("English","en"), ("Estonian","et"), ("Fijian","fj"),
                ("Filipino","fil"), ("Finnish","fi"), ("French","fr"), ("German","de"), ("Greek","el"), ("Gujarati","gu"),
                ("Haitian Creole","ht"), ("Hebrew","he"), ("Hindi","hi"), ("Hmong Daw","mww"), ("Hungarian","hu"), ("Indonesian","id"),
                ("Irish","ga"), ("Italian","it"), ("Japanese","ja"), ("Kannada","kn"), ("Kiswahili","sw"), ("Klingon","tlh-Latn"),
                ("Klingon (plqaD)","tlh-Piqd"), ("Korean","ko"), ("Latvian","lv"), ("Lithuanian","lt"), ("Malagasy","mg"),
                ("Malay","ms"), ("Malayalam","ml"), ("Maltese","mt"), ("Maori","mi"), ("Marathi","mr"), ("Norwegian","nb"), ("Persian","fa"),
                ("Polish","pl"), ("Portuguese (Brazil)","pt-br"), ("Portuguese (Portugal)","pt-pt"), ("Punjabi","pa"),
                ("Queretaro Otomi","otq"), ("Romanian","ro"), ("Russian","ru"), ("Samoan","sm"), ("Serbian (Cyrillic)","sr-Cyrl"),
                ("Serbian (Latin)","sr-Latn"), ("Slovak","sk"), ("Slovenian","sl"), ("Spanish","es"), ("Swedish","sv"), ("Tahitian","ty"),
                ("Tamil","ta"), ("Telugu","te"), ("Thai","th"), ("Tongan","to"), ("Turkish","tr"), ("Ukrainian","uk"), ("Urdu","ur"),
                ("Vietnamese","vi"), ("Welsh","cy"), ("Yucatec Maya","yua")
            };
            var selectedLanguage = allLanguages.Where(x => x.Code == toLanguage).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(selectedLanguage.Language)) throw new MissingFieldException("Selected language is not recogonized.");
            return selectedLanguage;
        }
    }
}
