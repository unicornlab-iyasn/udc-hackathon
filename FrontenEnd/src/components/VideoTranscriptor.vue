<template>
  <div>
    <video
      id="video"
      v-bind:src="videoUrl"
      crossorigin="anonymous"
      @play="play"
      @pause="pause"
      @ended="ended"
      controls
    ></video>

    <div style="width: 100%; text-align: center">
      <p>To start video transcript, play the video.</p>
      <OuButton type="hero" icon="Checkbox" @click="stop" :disabled="!isPlaying"
        >Stop Video Transcript</OuButton
      >
    </div>
  </div>
</template>

<script lang="ts">
import { Component, Prop, Vue } from "vue-property-decorator";
import * as signalR from "@aspnet/signalr";
import * as signalRmsgpack from "@aspnet/signalr-protocol-msgpack";

import Transcriptor from "../utils/VideoTranscriptor";
import OuButton from "./controls/OuButton.vue";

@Component({
  components: {
    OuButton,
  },
})
export default class VideoTranscriptor extends Vue {
  @Prop({ default: "" }) videoUrl!: string;
  @Prop({ default: "en" }) toLanguage!: string;
  @Prop({ default: '["en-US", "ar-EG", "ru-RU"]' }) fromLanguage!: string;
  @Prop() voiceHubUrl!: string;

  private video!: HTMLVideoElement;
  private _transcriptor!: Transcriptor;

  isPlaying: boolean = false;
  isPaused: boolean = false;
  transcript: string = "";

  mounted() {
    console.log("setting video");
    this.video = document.getElementById("video") as HTMLVideoElement;
  }

  play(): void {
    this.video.play();
    this.isPlaying = true;
    this.isPaused = false;
    localStorage.videoUrl = this.videoUrl;
    if (this._transcriptor === null || this._transcriptor === undefined) {
      this._transcriptor = new Transcriptor(this.voiceHubUrl, this.video);
      this._transcriptor.transcriptReadyHandler = (transcript: string) => {
        this.$emit("transcript-ready", transcript);
      };
    }
    this._transcriptor.startTranscript(this.toLanguage, this.fromLanguage);
  }

  pause() {
    this._transcriptor.pauseTranscript();

    this.video.pause();
    this.isPaused = true;
  }

  stop() {
    this._transcriptor.stopTranscript();

    this.video.pause();
    this.video.currentTime = 0;

    this.isPlaying = false;
    this.isPaused = false;
  }

  ended() {
    this._transcriptor.stopTranscript();
    this.isPlaying = false;
    this.isPaused = false;
  }
}
</script>

<style scoped>
video {
  width:50%;
}
</style>
