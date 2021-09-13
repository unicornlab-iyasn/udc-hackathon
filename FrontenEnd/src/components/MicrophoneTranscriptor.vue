<template>
  <div>
    <div style="width: 100%; text-align: center">
      <OuButton
        type="hero"
        icon="Checkbox"
        @click="start"
        >Start Audio Transcript</OuButton
      >
    </div>
    <div style="width: 100%; text-align: center">
      <OuButton
        type="hero"
        icon="Checkbox"
        @click="pause"
        >Pause Audio Transcript</OuButton
      >
    </div>
    <div style="width: 100%; text-align: center">
      <OuButton type="hero" icon="Checkbox" @click="stop" 
        >Stop Audio Transcript</OuButton
      >
    </div>
  </div>
</template>

<script lang="ts">
import { Component, Prop, Vue } from "vue-property-decorator";
import * as signalR from "@aspnet/signalr";
import * as signalRmsgpack from "@aspnet/signalr-protocol-msgpack";

import Transcriptor from "../utils/AudioTranscriptor";
import OuButton from "./controls/OuButton.vue";
import { start } from "repl";

@Component({
  components: {
    OuButton,
  },
})
export default class MicrophoneTranscriptor extends Vue {
  @Prop({ default: "en" }) toLanguage!: string;
  @Prop({ default: '["en-US", "ar-EG", "ru-RU"]' }) fromLanguage!: string;
  @Prop() voiceHubUrl!: string;

  private _transcriptor!: Transcriptor;
  transcript: string = "";
  stream!: MediaStream;
  audioContext!: AudioContext;

  async start() {
    var audioStream = await  navigator.mediaDevices.getUserMedia({ video: false, audio: true });
    if (this._transcriptor === null || this._transcriptor === undefined) {
      this._transcriptor = new Transcriptor(this.voiceHubUrl, audioStream);
      this._transcriptor.transcriptReadyHandler = (transcript: string) => {
        this.$emit("transcript-ready", transcript);
      };
    }
    this._transcriptor.startTranscript(
      this.toLanguage,
      this.fromLanguage
    );
  }

  pause() {
    this._transcriptor.pauseTranscript();
  }

  stop() {
    this._transcriptor.stopTranscript();
  }
}
</script>
