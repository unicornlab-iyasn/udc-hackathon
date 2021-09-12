using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Translation;

namespace TranslatorConsoleApp
{
    class Program
    {
        public static async Task TranslationWithMicrophoneAsync_withLanguageDetectionEnabled()
        {
            string fromLanguage = "ru-RU";
            var config = SpeechTranslationConfig.FromSubscription("970994f6d00443dfa3b01ba78e016744", "westeurope");
            config.SpeechRecognitionLanguage = fromLanguage;
            config.AddTargetLanguage("en");
            config.SetProperty(PropertyId.SpeechServiceConnection_ContinuousLanguageIdPriority, "Accuracy");
            var autoDetectSourceLanguageConfig = AutoDetectSourceLanguageConfig.FromLanguages(new string[] { "en-US", "ar-EG", "ru-RU" });
            using (var recognizer = new TranslationRecognizer(config, autoDetectSourceLanguageConfig))
            {
                recognizer.Recognizing += (s, e) =>
                {
                    var lidResult = e.Result.Properties.GetProperty(PropertyId.SpeechServiceConnection_AutoDetectSourceLanguageResult);
                    Console.WriteLine($"RECOGNIZED in '{lidResult}': Text={e.Result.Text}");
                    foreach (var element in e.Result.Translations)
                    {
                        Console.WriteLine($" TRANSLATING into '{element.Key}': {element.Value.ToString()}");
                    }
                };

                recognizer.Recognized += (s, e) =>
                {
                    if (e.Result.Reason == ResultReason.TranslatedSpeech)
                    {
                        var lidResult = e.Result.Properties.GetProperty(PropertyId.SpeechServiceConnection_AutoDetectSourceLanguageResult);
                        Console.WriteLine($"RECOGNIZED in '{lidResult}': Text={e.Result.Text}");
                        //Console.WriteLine($"RECOGNIZED Text={e.Result.Text}");
                        foreach (var element in e.Result.Translations)
                        {
                            Console.WriteLine($"    TRANSLATED into '{element.Key}': {element.Value.ToString()}");
                        }
                    }
                    else if (e.Result.Reason == ResultReason.RecognizedSpeech)
                    {
                        var lidResult = e.Result.Properties.GetProperty(PropertyId.SpeechServiceConnection_AutoDetectSourceLanguageResult);
                        Console.WriteLine($"RECOGNIZED in '{lidResult}': Text={e.Result.Text}");
                        //Console.WriteLine($"RECOGNIZED Text={e.Result.Text}");
                        Console.WriteLine($"    Speech not translated.");
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

        public static async Task MultiLingualRecognitionWithUniversalV2Endpiont()
        {
            var v2EndpointInString = String.Format("wss://{0}.stt.speech.microsoft.com/speech/universal/v2", "westeurope");
            var v2EndpointUrl = new Uri("https://westeurope.api.cognitive.microsoft.com/sts/v1.0/issuetoken");
            var config = SpeechTranslationConfig.FromEndpoint(v2EndpointUrl, "970994f6d00443dfa3b01ba78e016744");
            config.SetProperty(PropertyId.SpeechServiceConnection_ContinuousLanguageIdPriority, "Accuracy");
            var autoDetectSourceLanguageConfig = AutoDetectSourceLanguageConfig.FromLanguages(new string[] { "en-US", "ru-RU" });

            var stopRecognition = new TaskCompletionSource<int>();
            using (var recognizer = new SpeechRecognizer(config, autoDetectSourceLanguageConfig))
            {
                // Starts recognizing.
                Console.WriteLine("Say something...");

                var result = await recognizer.RecognizeOnceAsync().ConfigureAwait(false);

                // Checks result.
                if (result.Reason == ResultReason.RecognizedSpeech)
                {
                    var lidResult = AutoDetectSourceLanguageResult.FromResult(result);
                    Console.WriteLine($"RECOGNIZED: Text={result.Text} with language={lidResult.Language}");
                }
                else if (result.Reason == ResultReason.NoMatch)
                {
                    Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = CancellationDetails.FromResult(result);
                    Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                        Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                        Console.WriteLine($"CANCELED: Did you update the subscription info?");
                    }
                }
            }
        }

        public static async Task MultiLingualTranslation()
        {
            var v2EndpointInString = String.Format("wss://{0}.stt.speech.microsoft.com/speech/universal/v2", "westeurope");
            var v2EndpointUrl = new Uri("https://westeurope.api.cognitive.microsoft.com/sts/v1.0/issuetoken");
            var config = SpeechTranslationConfig.FromEndpoint(v2EndpointUrl, "970994f6d00443dfa3b01ba78e016744");

            // Source lang is required, but is currently NoOp 
            string fromLanguage = "en-US";
            config.SpeechRecognitionLanguage = fromLanguage;

            config.AddTargetLanguage("ru");
            config.AddTargetLanguage("ar");

            config.SetProperty(PropertyId.SpeechServiceConnection_ContinuousLanguageIdPriority, "Latency");
            var autoDetectSourceLanguageConfig = AutoDetectSourceLanguageConfig.FromLanguages(new string[] { "en-US", "ru-RU" });

            var stopTranslation = new TaskCompletionSource<int>();
            using (var audioInput = AudioConfig.FromWavFileInput(@"C:\Users\iyas9\Desktop\UDC Hackathon\udc-hackathon\ConsoleApp\Recording.wav"))
            //using (var audioInput = AudioConfig.FromDefaultMicrophoneInput())
            {
                using (var recognizer = new TranslationRecognizer(config, autoDetectSourceLanguageConfig, audioInput))
                {
                    recognizer.Recognizing += (s, e) =>
                    {
                        var lidResult = e.Result.Properties.GetProperty(PropertyId.SpeechServiceConnection_AutoDetectSourceLanguageResult);

                        Console.WriteLine($"RECOGNIZING in '{lidResult}': Text={e.Result.Text}");
                        foreach (var element in e.Result.Translations)
                        {
                            Console.WriteLine($"    TRANSLATING into '{element.Key}': {element.Value}");
                        }
                    };

                    recognizer.Recognized += (s, e) =>
                    {
                        if (e.Result.Reason == ResultReason.TranslatedSpeech)
                        {
                            var lidResult = e.Result.Properties.GetProperty(PropertyId.SpeechServiceConnection_AutoDetectSourceLanguageResult);

                            Console.WriteLine($"RECOGNIZED in '{lidResult}': Text={e.Result.Text}");
                            foreach (var element in e.Result.Translations)
                            {
                                Console.WriteLine($"    TRANSLATED into '{element.Key}': {element.Value}");
                            }
                        }
                        else if (e.Result.Reason == ResultReason.RecognizedSpeech)
                        {
                            Console.WriteLine($"RECOGNIZED: Text={e.Result.Text}");
                            Console.WriteLine($"    Speech not translated.");
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

                        stopTranslation.TrySetResult(0);
                    };

                    recognizer.SpeechStartDetected += (s, e) =>
                    {
                        Console.WriteLine("\nSpeech start detected event.");
                    };

                    recognizer.SpeechEndDetected += (s, e) =>
                    {
                        Console.WriteLine("\nSpeech end detected event.");
                    };

                    recognizer.SessionStarted += (s, e) =>
                    {
                        Console.WriteLine("\nSession started event.");
                    };

                    recognizer.SessionStopped += (s, e) =>
                    {
                        Console.WriteLine("\nSession stopped event.");
                        Console.WriteLine($"\nStop translation.");
                        stopTranslation.TrySetResult(0);
                    };

                    // Starts continuous recognition. Uses StopContinuousRecognitionAsync() to stop recognition.
                    Console.WriteLine("Start translation...");
                    await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

                    Task.WaitAny(new[] { stopTranslation.Task });
                    await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
                }
            }
        }



        static async Task Main(string[] args)
        {
            //added this so it can show russian language
            Console.OutputEncoding = Encoding.UTF8;
            await MultiLingualTranslation();
        }
    }
}
