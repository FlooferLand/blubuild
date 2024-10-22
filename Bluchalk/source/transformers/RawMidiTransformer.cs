using Bluchalk.types;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Project;

namespace Bluchalk;

/// Defines how to read/write from raw midi
public class RawMidiTransformer : IBaseTransformer {
    public Boolean RespectsFormat(Stream stream) {
        return true;
    }

    public Result<ShowData> Read(Stream stream) {
        var file = MidiFile.Read(stream);
        if (file.TimeDivision is not TicksPerQuarterNoteTimeDivision) {
            return Result<ShowData>.Err("Only the \"Ticks per quarter note\" time division is currently supported!");
        }

        // Reading the notes
        var usedChannels = new List<Note>();
        var output = new SignalContainer();
        foreach (var note in file.GetNotes()) {
            var startTime = TimeConverter.ConvertTo<MetricTimeSpan>(note.GetTimedNoteOnEvent().Time, file.GetTempoMap());
            var endTime = TimeConverter.ConvertTo<MetricTimeSpan>(note.GetTimedNoteOffEvent().Time, file.GetTempoMap());
            output.AddEvent(note.NoteNumber, new SignalEvents.BitEvent(startTime, note, true));
            output.AddEvent(note.NoteNumber, new SignalEvents.BitEvent(endTime, note, false));
        }

        // Adding the end notes
        foreach (var note in usedChannels) {
            output.AddEvent(note.NoteNumber, new SignalEvents.BitEvent(((TimeSpan)file.GetDuration<MetricTimeSpan>()) - TimeSpan.FromTicks(5), note, false));
        }
        
        return Result<ShowData>.Ok(new ShowData("Unknown", file.GetDuration<MetricTimeSpan>(), output));
    }
}
