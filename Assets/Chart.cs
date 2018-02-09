using System.Collections;
using System.Collections.Generic;

public enum Difficulty
{
	Easy,
	Medium,
	Hard,
	Expert
}

[System.Serializable]
public class Chart
{
	public string Name;
	public string Artist;
	public string Charter;
	public float Offset;
	public int Resolution;
	public string Player2;
	public int Difficulty;
	public float PreviewStart;
	public float PreviewEnd;
	public string Genre;
	public string MediaType;
	public string MusicStream;

	public List<TimeSignature> timeSignatures;
	public List<BPM> bpms; // x1000

	public Dictionary<Difficulty, List<Note>> Notes;	
}
