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
  @Prop({ default: "ru" }) toLanguage!: string;
  @Prop({ default: "en-US" }) priorityLanguage!: string;
  @Prop({ default: '["ar-EG", "ru-RU"]' }) fromLanguage!: string;
  @Prop() voiceHubUrl!: string;

  private _transcriptor!: Transcriptor;
  transcript: string = "";
  stream!: MediaStream;
  audioContext!: AudioContext;

  start(): void {
    //     let device = navigator.mediaDevices.getUserMedia({ audio: true });
    // let items = []
    // device.then((stream) => {
    //   // keep the context in a global variable
    //   this.stream = stream
    //   let bufferSize = 1024 * 16
    //   let audioContext = new AudioContext()
    //   this.audioContext = audioContext
    //   let processor = audioContext.createScriptProcessor(bufferSize, 1, 1)
    //   this.processor = processor
    //   processor.connect(audioContext.destination)
    //   this.input = audioContext.createMediaStreamSource(stream)
    //   this.input.connect(processor)
    //   processor.onaudioprocess = (e) => {
    //     // receives data from microphone
    //     const buffer = e.inputBuffer.getChannelData(0) // get only one audio channel
    //     socket.emit('micBinaryStream', buffer) // send to server via web socket
    //   }
    // });

    var audioStream = new MediaStream();

    if (navigator.mediaDevices) {
      console.log("getUserMedia supported.");
      navigator.mediaDevices
        .getUserMedia({ audio: true})
        .then(function (stream) {
          // video.srcObject = stream;
          // video.onloadedmetadata = function (e) {
          //   video.play();
          //   video.muted = true;
          //}; 
          audioStream = stream;
        })
        .catch(function (err) {
          console.log("The following gUM error occurred: " + err);
        });
    } else {
      console.log("getUserMedia not supported on your browser!");
    }

    // //start transcribing
    if (this._transcriptor === null || this._transcriptor === undefined) {
      this._transcriptor = new Transcriptor(this.voiceHubUrl, audioStream);
      this._transcriptor.transcriptReadyHandler = (transcript: string) => {
        this.$emit("transcript-ready", transcript);
      };
    }
    this._transcriptor.startTranscript(
      this.toLanguage,
      this.priorityLanguage,
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
