# UDC Hackathon - Live Translation Web and Console Apps
> The main approach in this project was aimed at maximizing reusability and simplifying the integration into company's projects. The frontend was developed solely to ease the future implementations.
>
> - Web App is there to showcase a realistic use of the live translation functionality.
> - Console App is there to simplify the understanding of the live translation function. This is done by removing any additional code that is used for streaming the data back and forth in the Web App.
>
> It is known that a demo is worth a thousand documentations, please watch this one 

## Web Application
**Features:**
- Live transcribing and translation of an input stream from the microphone
- Live transcribing and translation of an audio stream from a video
- Automatic source language detection (up to 10 possible source languages at once)
- Selection of the destination language

**Approach:**
- Backend is build using .Net Core
- Frontend is build using Vue.js
- Audio stream is transmitted using a SignalR hub.
- Speech recognition, language identification and translation are done using Azure Cognitive Services.
- Speech detection and language identification priority is set to "Accuracy" rather than "Latency". This leads to a minor increase in the delay. However, it eliminates any uncertain recognitions and language identifications, especially in the beginning of the stream. 

**Running the app locally:**
The backend solution should be opened in Visual Studio and executed from there. For the frontend:
`
npm install
`
`
npm run serve
`
Video [sample](https://nmoqsa.blob.core.windows.net/thumbnails/WhatsApp%20Video%202021-09-12%20at%205.55.10%20PM.mp4) with "ar-EG" language, recorded by a friend of mine in fluent Arabic. Paste the url in the "video url" field. Note that not all languages support identification, please refer to the [documentation](https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/language-support).

**Architecture:**
![](Docs/UDC%20Web%20App.jpg?raw=true)

## Console Application
**Running the app locally:**
The backend solution should be opened in Visual Studio and executed from there.

**Architecture:**
![](Docs/UDC%20Console%20App.jpg?raw=true)

### Sources
- Cognitive Services integration and functions were built using Microsoft's [documentation](https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/get-started-speech-translation?tabs=script%2Cwindowsinstall&pivots=programming-language-csharp).
- SignlaR streaming was build referencing this [sample](https://github.com/msimecek/Sample-Continuous-S2T).
> NOTE: Even though the sample was used as a base, it underwent a number of drastic changes in both backend and frontend. A number of functionalities where added and the existing ones where edited due to being outdated. If you want more details on development, please contact me. I will be happy to provide with an in-depth breakdown of the code.