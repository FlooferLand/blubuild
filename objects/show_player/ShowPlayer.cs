using System;
using System.IO;
using Bluchalk;
using Bluchalk.types;
using Project.Components;

namespace Project;
using Godot;

/// Handles playing a show from a file
public partial class ShowPlayer : Node {
	[ExportGroup("Public")]
	[Export] public BotControllerComp BotController;
	
	[ExportGroup("Private")]
	[Export] public AudioStreamPlayer3D Audio;
	[Export] public TextureRect VisualBitMap;

	private Option<ShowData> showData = Option<ShowData>.None();
	private bool playing = false;
	private int currentEvent = 0;
	private TimeSpan currentTime = TimeSpan.Zero;
	
	public override void _Ready() {
		Start("res://showtapes/test_raw");
	}

	// TODO: Make it so it can read several bits at the same time
	public override void _Process(double delta) {
		// Playing the show
		if (playing && showData.LetSome(out var data) && currentEvent < data.Signal.Count()) {
			if (currentEvent == 0) {
				Audio.Play();
			}
			
			// Getting the events
			var current = data.Signal.Get(currentEvent);
			if (current == null) return;
			var next = data.Signal.Get(currentEvent + 1) ?? current;
			
			if (currentTime > next.TimeStamp) {
				currentEvent += 1;
				DebugUpdateBit(current.RawNote.NoteNumber, !current.Enabled);
			}
			
			// Counting the time
			currentTime += TimeSpan.FromSeconds(delta);
			//GD.Print($"Time: {currentTime}  |  Event: {currentEvent / 2}");
		}
	}

	private void DebugUpdateBit(int bit, bool value) {
		const int jawBit = 84;
		const int headUpBit = 48;
		
		string bitString = bit switch {
			jawBit => "20 TD - Jaw",
			headUpBit => "10 TD - Head up"
		};
		
		// Updating the animation
		BotController?.TriggerAction(bitString, value);
		
		// Updating the debug bit visualizer
		var img = (VisualBitMap.Texture as ImageTexture).GetImage()
		          ?? Image.CreateEmpty(4, 4, false, Image.Format.Rgb8);
		img.Fill(Colors.Black);
		img.SetPixel(0, 0, (bit == jawBit && value) ? Colors.Green : Colors.DarkRed);
		img.SetPixel(1, 0, (bit == headUpBit && value) ? Colors.Green : Colors.DarkRed);
		(VisualBitMap.Texture as ImageTexture).SetImage(img);
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
		currentEvent = 0;
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
