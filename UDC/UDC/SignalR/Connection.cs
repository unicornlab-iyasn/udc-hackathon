using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Translation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UDC.Speech;

namespace UDC.SignalR
{
    public class Connection
    {
        public string SessionId;
        public TranslationRecognizer SpeechClient;
        public VoiceAudioStream AudioStream;
    }
}
