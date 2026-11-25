extends RefCounted

# Frame-start wall-clock intervals, not Godot's smoothed/clamped simulation delta.
# A sample pairs the previous frame's CPU update loop with its complete interval.
var frame_samples: Array[float] = []
var cpu_samples: Array[float] = []
var result: Dictionary = {}
var settle_until_usec := 0
var sample_duration_usec := 0
var previous_usec := -1
var started_usec := -1
var sampled_seconds := 0.0
var completed := false

func reset(now_usec: int, settle_seconds: float, duration_seconds: float):
	frame_samples.clear()
	cpu_samples.clear()
	result.clear()
	settle_until_usec = now_usec + int(settle_seconds * 1000000)
	sample_duration_usec = maxi(1, int(duration_seconds * 1000000))
	previous_usec = -1
	started_usec = -1
	sampled_seconds = 0
	completed = false

func advance(now_usec: int, previous_cpu_ms: float) -> bool:
	var previous := previous_usec
	previous_usec = now_usec
	if completed or previous < settle_until_usec or previous < 0 or now_usec <= previous:
		return false
	if started_usec < 0:
		started_usec = previous
	frame_samples.append((now_usec - previous) / 1000.0)
	cpu_samples.append(previous_cpu_ms)
	sampled_seconds = (now_usec - started_usec) / 1000000.0
	if now_usec - started_usec < sample_duration_usec:
		return false
	completed = true
	result = {"frames": frame_samples.size(), "seconds": sampled_seconds,
		"fps": frame_samples.size() / sampled_seconds,
		"frame_ms": summarize(frame_samples), "cpu_update_ms": summarize(cpu_samples)}
	return true

static func summarize(samples: Array[float]) -> Dictionary:
	if samples.is_empty():
		return {}
	var sorted := samples.duplicate()
	sorted.sort()
	var total := 0.0
	for value in samples:
		total += value
	var middle: int = sorted.size() / 2
	var median: float = sorted[middle]
	if sorted.size() % 2 == 0:
		median = (sorted[middle - 1] + sorted[middle]) * 0.5
	return {"mean": total / samples.size(), "median": median,
		"p95": sorted[maxi(0, ceili(sorted.size() * 0.95) - 1)], "worst": sorted.back()}
