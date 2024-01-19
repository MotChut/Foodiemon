using Godot;

public partial class AudioController : AudioStreamPlayer
{
	[Export] public AudioStreamOggVorbis title;
	[Export] public AudioStreamOggVorbis home;
	int volumnRate = 0;
	public void PlayTitle()
	{
		Stream = title;
		Play();
	}

	public void PlayHome()
	{
		Stream = home;
		Play();
	}

	public string SetVolumnRate()
	{
		string text = "";
		volumnRate++;
		if(volumnRate == 6) volumnRate = 0;

		switch(volumnRate)
		{
			case 0: 
			VolumeDb = 1;
			text = "100%";
			break;
			case 1: 
			VolumeDb = -5;
			text = "80%";
			break;
			case 2: 
			VolumeDb = -10;
			text = "60%";
			break;
			case 3: 
			VolumeDb = -15;
			text = "40%";
			break;
			case 4: 
			VolumeDb = -20;
			text = "20%";
			break;
			case 5: 
			VolumeDb = -999;
			text = "0%";
			break;
		}

		return text;
	}
}
