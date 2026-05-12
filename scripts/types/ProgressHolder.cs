using System;
using Godot;
using SharpCompress.Common;

namespace Project;

public record ProgressHolder(string Goal) : IDisposable {
    public delegate void UpdateEventHandler(ProgressHolder holder);
    public event UpdateEventHandler? TaskUpdate;

    public static readonly ProgressTask EmptyTask = new("");

    public ProgressHolder(string goal, UpdateEventHandler onUpdate) : this(goal) {
        TaskUpdate += onUpdate;
    }

    public ProgressTask Task = new(Goal);
    public bool Finished = false;
    public float PercentageComplete => (Task.TargetBytes ?? 0) - (float) Task.ProgressBytes;

    public struct ProgressTask(string message, long? targetBytes = 0) {
        public string Message = message;
        public long? TargetBytes = targetBytes;
        public long ProgressBytes = 0;
        public bool Indeterminate = false;
        public void Reset() { this = EmptyTask; }
    }

    public void Set(string @string, long? targetBytes = null, bool indeterminate = false) {
        Task.Message = @string;
        Task.TargetBytes = targetBytes;
        Task.Indeterminate = indeterminate || targetBytes == null;
        if (Finished) Finished = false;
        if (Task.TargetBytes != null && Task.ProgressBytes == Task.TargetBytes)
            Finish();
        TaskUpdate?.Invoke(this);
    }

    public void Finish() {
        Task.Reset();
        Finished = true;
        TaskUpdate?.Invoke(this);
    }

    public IProgress<ProgressReport> CreateProgressReport() {
        return new Progress<ProgressReport>(p => {
            Task.Message = $"Working on {p.EntryPath}";
            Task.ProgressBytes = p.BytesTransferred;
            if (p.TotalBytes != null && p.BytesTransferred == p.TotalBytes)
                Finish();
            TaskUpdate?.Invoke(this);
        });
    }

    public void UpdateUi(ProgressBar node, bool makeInvis = false) {
        node.Visible = !Finished || !makeInvis;
        node.Value = PercentageComplete;
        node.Indeterminate = Task.Indeterminate;
    }
    public void UpdateUi(Label node, bool makeInvis = false) {
        node.Visible = !Finished || !makeInvis;
        node.Text = Task.Message;
    }

    public void Dispose() => Finish();
}