using Godot;
using System;

public partial class Main : Node
{
	// GUI nodes
	private AnimationPlayer guiAnim;
	private SpinBox daysSpinBox;
	private SpinBox hourSpinBox;
	private SpinBox minuteSpinBox;
	// Timer nodes
	private AnimationPlayer dayAnim;
	private AnimationPlayer progressAnim;
	// Timer ready nodes
	private AnimationPlayer timerReadyAnim;
	// Other
	private const string CFG_PATH = "user://config.cfg";
	private ConfigFile cfg = new ConfigFile();
	// Time
	private DateTime startTime = DateTime.Now;
	private DateTime endTime = DateTime.Now;

	public override void _Ready()
	{
		// Init onready variables
		guiAnim = GetNode<AnimationPlayer>("GUI/AnimationPlayer");
		daysSpinBox = GetNode<SpinBox>("GUI/CenterContainer/VBoxContainer/HBoxContainer/DaysSpinBox");
		hourSpinBox = GetNode<SpinBox>("GUI/CenterContainer/VBoxContainer/HBoxContainer/HourSpinBox");
		minuteSpinBox = GetNode<SpinBox>("GUI/CenterContainer/VBoxContainer/HBoxContainer/MinuteSpinBox");
		dayAnim = GetNode<AnimationPlayer>("Timer/Day");
		progressAnim = GetNode<AnimationPlayer>("Timer/Progress");
		// Load config
		if (FileAccess.FileExists(CFG_PATH))
		{
			cfg.Load(CFG_PATH);
		}
		else return;
		DateTime tempStartTime = new DateTime(
				cfg.GetValue("Start", "year", 0).AsInt32(),
				cfg.GetValue("Start", "month", 0).AsInt32(),
				cfg.GetValue("Start", "day", 0).AsInt32(),
				cfg.GetValue("Start", "hour", 0).AsInt32(),
				cfg.GetValue("Start", "minute", 0).AsInt32(),
				0
		);
		DateTime tempEndTime = new DateTime(
			cfg.GetValue("End", "year", 0).AsInt32(),
			cfg.GetValue("End", "month", 0).AsInt32(),
			cfg.GetValue("End", "day", 0).AsInt32(),
			cfg.GetValue("End", "hour", 0).AsInt32(),
			cfg.GetValue("End", "minute", 0).AsInt32(),
			0
		);
		if (tempEndTime > DateTime.Now && tempStartTime < tempEndTime)
		{
			startTime = tempStartTime;
			endTime = tempEndTime;
			StartCountdown();
		}
	}

	public void _OnTrackButtonPressed()
	{
		GD.Print("Track button pressed.");
		startTime = DateTime.Now;
		endTime = new DateTime(startTime.Year, startTime.Month, startTime.Day, (int)hourSpinBox.Value, (int)minuteSpinBox.Value, 0).AddDays(daysSpinBox.Value);
		cfg.SetValue("Start", "year", Variant.CreateFrom(startTime.Year));
		cfg.SetValue("Start", "month", Variant.CreateFrom(startTime.Month));
		cfg.SetValue("Start", "day", Variant.CreateFrom(startTime.Day));
		cfg.SetValue("Start", "hour", Variant.CreateFrom(startTime.Hour));
		cfg.SetValue("Start", "minute", Variant.CreateFrom(startTime.Minute));
		cfg.SetValue("End", "year", Variant.CreateFrom(endTime.Year));
		cfg.SetValue("End", "month", Variant.CreateFrom(endTime.Month));
		cfg.SetValue("End", "day", Variant.CreateFrom(endTime.Day));
		cfg.SetValue("End", "hour", Variant.CreateFrom(endTime.Hour));
		cfg.SetValue("End", "minute", Variant.CreateFrom(endTime.Minute));
		cfg.Save(CFG_PATH);
		StartCountdown();
	}

	private void StartCountdown()
	{
		GD.Print("Start countdown with start time ", startTime, " and end time ", endTime, ".");
		guiAnim.Play("start_timer");
		DateTime now = DateTime.Now;
		// Sync day with clock
		dayAnim.Play("day");
		dayAnim.Seek(((((now.Hour * 60) + now.Minute) * 60) + now.Second) / 100);
		// Set up progress
		TimeSpan duration = endTime.Subtract(startTime);
		long totalMinutes = TimeSpan2Minutes(duration);
		progressAnim.Play("progress");
		progressAnim.SpeedScale = 1.0f / (float)totalMinutes;
		// Set progress
		TimeSpan done = now.Subtract(startTime);
		long doneMinutes = TimeSpan2Minutes(done);
		progressAnim.Seek(doneMinutes * progressAnim.SpeedScale * 60);
	}

	private long TimeSpan2Minutes(TimeSpan timeSpan)
	{
		return ((((timeSpan.Days * 24) + timeSpan.Hours) * 60) + timeSpan.Minutes);
	}

	public void OnTimerReady()
	{
		guiAnim.Play("timer_ready");
	}

	public void _OnRichMetaClicked(string meta) {
		OS.ShellOpen(meta);
	}
}
