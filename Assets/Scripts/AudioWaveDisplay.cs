using UnityEngine;

public class AudioWaveDisplay : MonoBehaviour {
	public Material waveformMaterial;
	public Material scrollableWaveformMaterial;
	public AudioSource audioSource;

	public int offset = 5000;
	public int skipn  = 1;

	public int listCount = 10000000;
	
	private int _bufferSize = 0;
	private ComputeBuffer _buffer;
	private float[] _samples;
	private float[] _samplesMaxed;
	
	public void SetDisplay()
    {
	//get audio data and find Max of every sample from both channels (average 2)
		// AudioSource aud = GetComponent<AudioSource>();

        _samples = new float[audioSource.clip.samples * audioSource.clip.channels];
        audioSource.clip.GetData(_samples, 0);
		print(_samples.Length);
		
		// * Samples are between 0.0 and 2.0

		int startPosition = -1;

		_samplesMaxed = new float[_samples.Length/2];
		int j = 0;
		for (int i = 0; i < _samplesMaxed.Length; ++i)
		{
            if (j >= _samples.Length) print(j + " er " + i);
            float maxValue = Mathf.Max(Mathf.Abs(_samples[j]), Mathf.Abs(_samples[j+1]));
            _samplesMaxed[i] = maxValue*2;

			if (startPosition == -1 && maxValue != 0.0) startPosition = i;
			j += 2;
		}

		offset = Mathf.Max(startPosition - Screen.width / 2, 0);
		Debug.Log($"{offset} is start position");

		_bufferSize = _samplesMaxed.Length;
		waveformMaterial.SetInt("bufferSize", _bufferSize);
		scrollableWaveformMaterial.SetInt("bufferSize", _bufferSize);
		Debug.Log($"{_bufferSize} is bufferSize");

		ReleaseBuffers();
		
		_buffer = new ComputeBuffer(_bufferSize, 4);
		_buffer.SetData(_samplesMaxed);
		waveformMaterial.SetBuffer("buffer", _buffer);
		scrollableWaveformMaterial.SetBuffer("buffer", _buffer);

		Debug.Log("Set Display");
	}
	
	private void ReleaseBuffers()
    {
		if (_buffer != null) _buffer.Release();
		_buffer = null;
	}
	
	private void GetAudioData()
    {
	
	}
	
	private void Update()
    {
		waveformMaterial.SetInt("offset", offset);
		waveformMaterial.SetInt("skipn", skipn);

		// ! Add zoom in and out
        if (Input.GetKey(KeyCode.LeftArrow))
            offset = Mathf.Min(offset + 5 + skipn, _bufferSize - Screen.width / 2);
        
        if (Input.GetKey(KeyCode.RightArrow))
            offset = Mathf.Max(offset - (5 + skipn), Screen.width / 2);
        
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
			skipn *= 2;
			
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
			skipn /= 2;
			
		if (skipn < 1) skipn = 1;

		// if (_samplesMaxed == null) return;
		// Debug.Log($"Offset Location: {offset} with value {_samplesMaxed[offset]}");
	}
     
    public void SetOffset(float value)
    {
        // Debug.Log((int)(value * _bufferSize));
        offset = (int)(value * _bufferSize);
    }
        
	private void OnDestroy()
    {
        ReleaseBuffers();
    }
}