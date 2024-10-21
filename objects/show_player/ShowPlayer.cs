using System;
using System.Collections.Generic;
using System.IO;
using Bluchalk;
using Bluchalk.types;
using Project.Components;

namespace Project;
using Godot;

/// Handles playing a show from a file
public partial class ShowPlayer : Node3D {
	[ExportGroup("Public")]
	[Export] public BotControllerComp BotController;
	
	[ExportGroup("Private")]
	[Export] public AudioStreamPlayer3D Audio;
	[Export] public TextureRect VisualBitMap;

	private Option<ShowData> showData = Option<ShowData>.None();
	private bool playing = false;
	private Dictionary<int, int> currentEvents = new();
	private Dictionary<int, bool> bitMap = new();
	private TimeSpan currentTime = TimeSpan.Zero;
	
	public override void _Ready() {
		Start("res://showtapes/test_raw");
	}

	// TODO: Make it so it can read several bits at the same time
	public override void _Process(double delta) {
		// Playing the show
		if (playing && showData.LetSome(out var data)) {
			if (!Audio.Playing)
				Audio.Play();
			foreach ((int id, var channel) in data.Signal.Channels()) {
				if (!currentEvents.ContainsKey(id))
					currentEvents.TryAdd(id, 0);
				if (!bitMap.ContainsKey(id))
					bitMap.TryAdd(id, false);
				if (currentEvents[id] >= channel.CountEvents())
					continue;

				// Getting the events
				var current = channel.GetEvent(currentEvents[id]);
				if (current == null) return;
				var next = channel.GetEvent(currentEvents[id] + 1) ?? current;

				if (currentTime > next.TimeStamp) {
					currentEvents[id] += 1;
					DebugUpdateBit(current.RawNote.NoteNumber, ! current.Enabled);
				}
			}
			
			// Counting the time
			currentTime += TimeSpan.FromSeconds(delta);
			if (currentTime >= data.Length) {
				Stop();
			}
			
			// Updating the debug bit visualizer
			var img = (VisualBitMap.Texture as ImageTexture).GetImage()
			          ?? Image.CreateEmpty(4, 4, false, Image.Format.Rgb8);
			int i = 0;
			img.Fill(Colors.Black);
			foreach ((int k, bool v) in bitMap) {
				img.SetPixel(i, 0, v ? Colors.Green : Colors.DarkRed);
				i++;
			}
			(VisualBitMap.Texture as ImageTexture).SetImage(img);
		}
	}

	private void DebugUpdateBit(int bit, bool value) {
		const int jawBit = 84;
		const int headUpBit = 56;
		const int strumBit = 48;
		
		string bitString = bit switch {
			headUpBit => "10 TD - Head up",
			jawBit => "20 TD - Jaw",
			strumBit => "30 TD - Strum",
		};
		
		BotController?.TriggerAction(bitString, value);
		bitMap[bit] = value;
	}

	#region Public playback stuff
	public void Start(string filePath) {
		string audioPath = filePath + "/audio.mp3";
		string signalPath = filePath + "/signal.mid";
		
		// Loading the audio
		using (var file = FileAccess.Open(audioPath, FileAccess.ModeFlags.Read)) {
			var stream = new AudioStreamMP3();
			stream.Data = file.GetBuffer((long)file.GetLength());
			Audio.Stream = stream;
		}
		
		// Loading the signal
		var signal = Result<ShowData>.Err($"Failed to load the signal file at \"{signalPath}\"");
		using (var file = FileAccess.Open(signalPath, FileAccess.ModeFlags.Read)) {
			signal = new RawMidiTransformer().Read(new MemoryStream(file.GetBuffer((long)file.GetLength())));
		}
		
		// Playing stuff
		var sig = signal.Unwrap();
		currentTime = TimeSpan.Zero;
		showData = Option<ShowData>.Some(sig);
		playing = true;
	}

	public void Stop() {
		currentEvents.Clear();
		showData = Option<ShowData>.None();
		playing = false;
		Audio.Stop();
	}

	public void Pause() {
		playing = false;
	}
	
	public void Unpause() {
		playing = true;
	}
	#endregion
}
