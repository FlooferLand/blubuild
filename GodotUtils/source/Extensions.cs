using Godot;
using System.Diagnostics.Contracts;

namespace GodotUtils;

public static class Extensions {
	#region Vector2 extensions
	// I could make extensions for Node3D/Character3D/etc (if .Position or .Velocity is being set)
	// Having a .PositionSetX would be shorter.
	[Pure] public static Vector2 WithX(this Vector2 self, float x) => new(x, self.Y);
	[Pure] public static Vector2 WithY(this Vector2 self, float y) => new(self.X, y);

	[Pure] public static Vector2 WithX(this Vector2 self, double x) => new((float)x, self.Y);
	[Pure] public static Vector2 WithY(this Vector2 self, double y) => new(self.X, (float)y);

	/// <summary>
	/// Sums up the values of a Vector <i>(X + Y)</i>,
	/// runs <c>Abs</c> on them, and <b>ATTEMPTS</b> to remap the values to a <i>0 to 1</i> scale.
	/// </summary>
	public static float Combined(this Vector2 self) {
		var vector = self.Abs();
		float combined = (vector.X + vector.Y);
		return combined / 2;
	}
	
	/// <summary>
	/// Maps a vector from [<paramref name="inFrom" />, <paramref name="inTo" />]
	/// to [<paramref name="outFrom" />, <paramref name="outTo" />].
	/// </summary>
	/// <param name="self">(the current Vector)</param>
	/// <param name="inFrom">The start value for the input interpolation.</param>
	/// <param name="inTo">The destination value for the input interpolation.</param>
	/// <param name="outFrom">The start value for the output interpolation.</param>
	/// <param name="outTo">The destination value for the output interpolation.</param>
	/// <returns>The resulting mapped vector.</returns>
	public static Vector2 Remap(this Vector2 self, Vector2 inFrom, Vector2 inTo, Vector2 outFrom, Vector2 outTo) {
		return new Vector2(
			Mathf.Remap(self.X, inFrom.X, inTo.X, outFrom.X, outTo.X),
			Mathf.Remap(self.Y, inFrom.Y, inTo.Y, outFrom.Y, outTo.Y)
		);
	}

	/// <summary>
	/// Snaps the position to the closest (snap) increment
	/// </summary>
	public static Vector2 SnappyBoi(this Vector2 self, float snap = 16) {
		return new Vector2(
			Utils.SnappyBoi(self.X, snap),
			Utils.SnappyBoi(self.Y, snap)
		);
	}

	/// <summary>
	/// Returns this Vector2 as a Vector3 with a Z of 0
	/// </summary>
	public static Vector3 ToVector3(this Vector2 self) {
		return new Vector3(self.X, self.Y, 0f);
	}
	#endregion
	
	#region Vector3 extensions
	// I could make extensions for Node3D/Character3D/etc (if .Position or .Velocity is being set)
	// Having a .PositionSetX would be shorter.
	[Pure] public static Vector3 WithX(this Vector3 self, float x) => new(x, self.Y, self.Z);
	[Pure] public static Vector3 WithY(this Vector3 self, float y) => new(self.X, y, self.Z);
	[Pure] public static Vector3 WithZ(this Vector3 self, float z) => new(self.X, self.Y, z);
	
	[Pure] public static Vector3 WithX(this Vector3 self, double x) => new((float)x, self.Y, self.Z);
	[Pure] public static Vector3 WithY(this Vector3 self, double y) => new(self.X, (float)y, self.Z);
	[Pure] public static Vector3 WithZ(this Vector3 self, double z) => new(self.X, self.Y, (float)z);

	/// Sums up the values of a Vector <i>(X + Y + Z)</i>,
	/// runs <c>Abs</c> and <c>Normalized</c> on them, and remaps the values to a <i>0 to 1</i> scale.
	public static float Combined(this Vector3 self) {
		var vector = self.Abs().Normalized();
		float combined = (vector.X + vector.Y + vector.Z);
		return combined / 3f;
	}

	/// Uses and returns radians
	public static Vector3 SimpleLookAt(this Vector3 current, Vector3 target) {
		return current.Rotated(Vector3.Up, current.AngleTo(target));
	}

	/// Godot source code came in clutch for this one
	/// https://github.com/godotengine/godot/blob/aa5b6ed13e4644633baf2a8a1384c82e91c533a1/scene/3d/node_3d.cpp#L931
	public static Vector3 PointTowards(this Vector3 pos, Vector3 target) {
		var forward = target - pos;
		var lookAtBasis = Basis.LookingAt(forward, Vector3.Up);
		return lookAtBasis.GetEuler();
	}
	public static Vector3 PointTowardsSmooth(this Vector3 pos, Vector3 target, Vector3 rotation, float weight) {
		if (pos.DistanceSquaredTo(target) < 0.02f)
			return rotation;
		
		// Calculating stuff
		var newRotation = pos.PointTowards(target);
		return new Vector3(
			Mathf.LerpAngle(rotation.X, newRotation.X, weight),
			Mathf.LerpAngle(rotation.Y, newRotation.Y, weight),
			Mathf.LerpAngle(rotation.Z, newRotation.Z, weight)
		);
	}
	#endregion
	
	#region AudioStreamPlayer extensions
	/// <summary>
	/// Plays a sound effect while randomizing the pitch.
	/// If an <c>AudioStream</c> is not specified, it will play the current stream.
	/// </summary>
	private static void InternalPlaySfx(
		Node audio,
        AudioStream? stream = null,
		float pitchRandomOffset = 0f,
		float pitchOffset = 0f
	) {
		var streamProp = AudioStreamPlayer.PropertyName.Stream;
		var pitchScaleProp = AudioStreamPlayer.PropertyName.PitchScale;
		var playFunc = AudioStreamPlayer.MethodName.Play;
		float pitchRandom = 0.15f + pitchRandomOffset;
		
		// Setting stuff
		if (stream != null)
			audio.Set(streamProp, stream);
		
		audio.Set(
			pitchScaleProp,
			(1f + pitchOffset) + Utils.Random.RandfRange(-pitchRandom, pitchRandom)
		);
		audio.Call(playFunc);
	}

	#region AudioStreamPlayer
	public static void PlaySfx(
		this AudioStreamPlayer self,
		AudioStream? stream = null,
		float pitchRandomOffset = 0f,
		float pitchOffset = 0f
	) => InternalPlaySfx(self, stream, pitchRandomOffset, pitchOffset);
	#endregion
	
	#region AudioStreamPlayer2D
	public static void PlaySfx(
		this AudioStreamPlayer2D self,
		AudioStream? stream = null,
		float pitchRandomOffset = 0f,
		float pitchOffset = 0f
	) => InternalPlaySfx(self, stream, pitchRandomOffset, pitchOffset);
	#endregion
	#endregion
}
