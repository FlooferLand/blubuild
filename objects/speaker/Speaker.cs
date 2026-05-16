using System;
using Bluchalk;
using Bluchalk.shows;
using Godot;

namespace Project;

public partial class Speaker : Node3D {
	[Signal] public delegate void AudioFrameEventHandler(byte[] frame);

	[Export] public required Greybox Greybox;

	[ExportGroup("Local")]
	[Export] public required BetterAudioPlayer3D AudioPlayer;

	int audioChunkSize => (int) (stream.MixRate * stream.BufferLength);

	AudioStreamGenerator stream = null!;
	AudioStreamGeneratorPlayback? streamPlayback = null;

	int byteSeek = 0;

	public override void _Ready() {
		stream = (AudioPlayer.Stream as AudioStreamGenerator)!;

		// Audio format setup
		if (Multiplayer.IsServer()) {
			Multiplayer.PeerConnected += id => {
				if (Greybox.ReelToReel.ServerLoadedWav is not { } wavHeader) return;
				RpcId(id, nameof(ClientReceiveWavHeader), wavHeader.SampleRate, wavHeader.Bits, wavHeader.Channels);
			};
			Greybox.ReelToReel.ShowLoaded += () => {
				byteSeek = 0;
				if (Greybox.ReelToReel.ServerLoadedWav is not { } wavHeader) return;
				stream.MixRate = wavHeader.SampleRate;
				Rpc(nameof(ClientReceiveWavHeader), wavHeader.SampleRate, wavHeader.Bits, wavHeader.Channels);
			};
			if (NetworkManager.IsDedicatedServer) AudioPlayer.QueueFree();
		}

		// Client playback
		if (Multiplayer.IsClientOrIntegrated()) {
			AudioFrame += frame => {
				if (streamPlayback == null) return;

				// TODO: Make other audio formats worked. Locked to stereo 16b. Also combine the channels into mono in case the audio is stereo for some reason
				var samples = new Vector2[frame.Length / 4];
				for (int i = 0; i < samples.Length; i++) {
					int offset = i * 4;
					float left = (short) (frame[offset] | frame[offset + 1] << 8) / (float) (short.MaxValue + 1);
					float right = (short) (frame[offset + 2] | frame[offset + 3] << 8) / (float) (short.MaxValue + 1);
					samples[i] = new Vector2(left, right);
				}
				streamPlayback.PushBuffer(samples);
			};
		}
	}

	// Server-only
	public override void _Process(double delta) {
		if (Greybox.ReelToReel.ServerLoadedShow is not { } loadedShow) return;
		if (Greybox.ReelToReel.ServerLoadedWav is not { } loadedWav) return;
		ProcessAudio(loadedShow, loadedWav);
	}

	void ProcessAudio(RshwFormat.RshwData loadedShow, WavHeader loadedWav) {
		int chunkSize = audioChunkSize * (loadedWav.Channels * 2);

		int bytesPerSample = loadedWav.Channels * (loadedWav.Bits / 8);
		int newByteSeek = (int) (Greybox.Seek * stream.MixRate) * bytesPerSample;
		if (newByteSeek < byteSeek + chunkSize) return;

		int end = Math.Min(byteSeek + chunkSize, loadedShow.audio.Length);
		if (end > byteSeek) {
			byte[] frame = loadedShow.audio[byteSeek..end];
			Rpc(nameof(SendAudioFrame), frame);
		}
		byteSeek += chunkSize;
	}

	[Rpc(CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable, TransferChannel = TransferChannels.ShowAudio)]
	void SendAudioFrame(byte[] frame) {
		EmitSignalAudioFrame(frame);
	}

	// Setting up the stream player on clients
	[Rpc(CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable, TransferChannel = TransferChannels.ShowAudio)]
	void ClientReceiveWavHeader(int sampleRate, int bits, int channels) {
		if (NetworkManager.IsDedicatedServer) return;
		stream.MixRate = sampleRate;

		AudioPlayer.Play();
		streamPlayback = (AudioPlayer.GetStreamPlayback<AudioStreamGeneratorPlayback>())!;
	}
}
