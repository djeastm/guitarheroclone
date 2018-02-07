using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Chart
{
	public string Name;
	public string Artist;
	public string Charter;
	public int Offset;
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
	
	public List<Note> Notes;
}
