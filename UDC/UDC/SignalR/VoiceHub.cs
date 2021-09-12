using Microsoft.AspNetCore.SignalR;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Translation;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UDC.Speech;

namespace UDC.SignalR
{
    public class VoiceHub : Hub
    {
        private static IConfiguration _configuration;
        private static IHubContext<VoiceHub> _hubContext;
        private static Dictionary<string, Connection> _connections;

        public VoiceHub(IConfiguration configuration, IHubContext<VoiceHub> ctx)
        {
            _configuration = configuration;

            if (_connections == null)
                _connections = new Dictionary<string, Connection>();

            if (_hubContext == null)
                _hubContext = ctx;
        }

        #region SignalR messages

        public async void AudioStart(byte[] args)
        {
            try
            {
                Debug.WriteLine($"Connection {Context.ConnectionId} starting audio.");

                var str = System.Text.Encoding.ASCII.GetString(args);
                var keys = JObject.Parse(str);
                var toLanguage = keys["toLanguage"].Value<string>();
                var priorityLanguage = keys["priorityLanguage"].Value<string>();
                var fromLanguagesStr = keys["fromLanguage"].Value<string>();
                var fromLanguages = JsonConvert.DeserializeObject<string[]>(fromLanguagesStr);
                var fromLanguagesList = fromLanguages.ToList();
                fromLanguagesList.Insert(0, priorityLanguage);
                fromLanguages = fromLanguagesList.ToArray();

                var toLanguageCode = ToLanguageValidation(toLanguage);
                var audioStream = new VoiceAudioStream();

                var audioFormat = AudioStreamFormat.GetWaveFormatPCM(16000, 16, 1);
                var audioConfig = AudioConfig.FromStreamInput(audioStream, audioFormat);

                var v2EndpointInString = String.Format("wss://{0}.stt.speech.microsoft.com/speech/universal/v2", _configuration["Speech:Region"]);
                var v2EndpointUrl = new Uri(_configuration["Speech:Endpoint"]);
                var translationConfig = SpeechTranslationConfig.FromEndpoint(v2EndpointUrl, _configuration["Speech:Key"]);
                //var translationConfig = SpeechTranslationConfig.FromSubscription(_configuration["Speech:Key"], _configuration["Speech:Region"]);
                //translationConfig.EndpointId = _configuration["Speech:Endpoint"];
                translationConfig.SpeechRecognitionLanguage = priorityLanguage;

                translationConfig.SetProperty(PropertyId.SpeechServiceConnection_ContinuousLanguageIdPriority, "Accuracy");
                translationConfig.SetProperty(PropertyId.SpeechServiceConnection_SingleLanguageIdPriority, "Accuracy");
                // This is required, even when language id is enabled.
                translationConfig.AddTargetLanguage(toLanguage);

                var autoDetectSourceLanguageConfig = AutoDetectSourceLanguageConfig.FromLanguages(fromLanguages);

                var speechClient = new TranslationRecognizer(translationConfig, autoDetectSourceLanguageConfig, audioConfig);

                speechClient.Recognized += _speechClient_Recognized;
                speechClient.Recognizing += _speechClient_Recognizing;
                speechClient.Canceled += _speechClient_Canceled;

                string sessionId = speechClient.Properties.GetProperty(PropertyId.Speech_SessionId);

                var conn = new Connection()
                {
                    SessionId = sessionId,
                    AudioStream = audioStream,
                    SpeechClient = speechClient,
                };

                _connections.Add(Context.ConnectionId, conn);

                await speechClient.StartContinuousRecognitionAsync();

                Debug.WriteLine("Audio start message.");
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error: " + e.Message);
            }

        }

        public void ReceiveAudio(byte[] audioChunk)
        {
            //Debug.WriteLine("Got chunk: " + audioChunk.Length);

            _connections[Context.ConnectionId].AudioStream.Write(audioChunk, 0, audioChunk.Length);
        }

        public async Task SendTranscript(string text, string sessionId)
        {
            var connection = _connections.Where(c => c.Value.SessionId == sessionId).FirstOrDefault();
            await _hubContext.Clients.Client(connection.Key).SendAsync("IncomingTranscript", text);
        }

        #endregion

        #region Speech events

        private void _speechClient_Canceled(object sender, TranslationRecognitionCanceledEventArgs e)
        {
            Debug.WriteLine("Recognition was cancelled.");
        }

        private async void _speechClient_Recognizing(object sender, TranslationRecognitionEventArgs e)
        {
            Debug.WriteLine($"{e.SessionId} > Intermediate result: {e.Result.Text}");
            var detectionResult = e.Result.Properties.GetProperty(PropertyId.SpeechServiceConnection_AutoDetectSourceLanguageResult);
            foreach (var (language, translation) in e.Result.Translations)
            {
                Debug.WriteLine($"{e.SessionId} > Intermediate result: Translated from " +
                    $"'{detectionResult}' into '{language}': {translation}");
            }
            //await SendTranscript(e.Result.Text, e.SessionId);
            await SendTranscript(e.Result.Translations.FirstOrDefault().Value, e.SessionId);
        }

        private async void _speechClient_Recognized(object sender, TranslationRecognitionEventArgs e)
        {
            Debug.WriteLine($"{e.SessionId} > Final result: {e.Result.Text}");
            //await SendTranscript(e.Result.Text, e.SessionId);
            await SendTranscript(e.Result.Translations.FirstOrDefault().Value, e.SessionId);
        }

        #endregion


        #region SignalR events

        public async override Task OnDisconnectedAsync(Exception exception)
        {
            var connection = _connections[Context.ConnectionId];
            await connection.SpeechClient.StopContinuousRecognitionAsync();
            connection.SpeechClient.Dispose();
            connection.AudioStream.Dispose();
            _connections.Remove(Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
        }

        #endregion

        public (string Language, string Code) ToLanguageValidation(string toLanguage)
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
