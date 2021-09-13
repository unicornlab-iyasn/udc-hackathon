<template>
  <div id="app">
    <h1 class="ms-font-xxl">Video to Text (Prototype)</h1>

    <div class="ms-Grid" dir="ltr">
      <div class="ms-Grid-row">
        <div class="ms-Grid-col ms-sm6">
          <VideoTranscriptor
            :videoUrl="videoUrl"
            :toLanguage="toLanguage"
            :fromLanguage="fromLanguage"
            :voiceHubUrl="voiceHubUrl"
            @transcript-ready="transcriptReady"
          />
        </div>

        <div class="ms-Grid-col ms-sm6">
          <p>Provide URL to the video you want to transcribe.</p>
          <OuTextField v-model="videoUrl" label="Video URL:"></OuTextField>
        </div>
        <div class="ms-Grid-col ms-sm6">
          <p>Provide the language code to which the speech is translated.</p>
          <OuTextField
            v-model="toLanguage"
            label="Tranlate To Language:"
          ></OuTextField>
        </div>
                <div class="ms-Grid-col ms-sm6">
          <p>
            Provide other languages that are expected in this audio.
          </p>
          <OuTextField
            v-model="fromLanguage"
            label="From Language:"
          ></OuTextField>
        </div>
                <div class="ms-Grid-col ms-sm6">
          <MicrophoneTranscriptor
            :toLanguage="toLanguage"
            :fromLanguage="fromLanguage"
            :voiceHubUrl="voiceHubUrl"
            @transcript-ready="transcriptReady"
          />
        </div>
      </div>
    </div>

    <p>{{ transcript }}</p>
  </div>
</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import VideoTranscriptor from "./components/VideoTranscriptor.vue";
import MicrophoneTranscriptor from "./components/MicrophoneTranscriptor.vue";
import OuTextField from "./components/controls/OuTextField.vue";
import Config from "./config";

@Component({
  components: {
    VideoTranscriptor,
    OuTextField,
    MicrophoneTranscriptor,
  },
})
export default class App extends Vue {
  toLanguage: string = "en";
  fromLanguage: string = "[\"en-US\", \"ar-EG\", \"ru-RU\"]";
  videoUrl: string = "";
  transcript: string = "";

  get voiceHubUrl(): string {
    return Config.VOICEHUB_URL;
  }

  mounted() {
    if (localStorage.videoUrl) this.videoUrl = localStorage.videoUrl;
  }

  transcriptReady(val: string) {
    this.transcript = val;
  }
}
</script>

<style>
#app {
  font-family: "Segoe UI", Tahoma, Geneva, Verdana, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}
</style>
