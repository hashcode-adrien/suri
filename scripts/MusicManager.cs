using Godot;
using System;

namespace Suri
{
    /// <summary>
    /// Music manager that plays procedural ambient background music.
    /// </summary>
    public partial class MusicManager : Node
    {
        private AudioStreamPlayer _audioPlayer;
        private AudioStreamGenerator _generator;
        private bool _isPlaying = true;
        
        // Music parameters
        private const float SampleRate = 22050f;
        private const float BaseFrequency = 261.63f; // C4
        private float _phase = 0f;
        private double _timeElapsed = 0.0;
        private int _currentChord = 0;
        
        // Chord progression: C major, A minor, F major, G major
        private readonly float[][] _chordFrequencies = new float[][]
        {
            new float[] { 130.81f, 164.81f, 196.00f }, // C major (C3, E3, G3)
            new float[] { 110.00f, 130.81f, 164.81f }, // A minor (A2, C3, E3)
            new float[] { 174.61f, 220.00f, 261.63f }, // F major (F3, A3, C4)
            new float[] { 196.00f, 246.94f, 293.66f }  // G major (G3, B3, D4)
        };
        
        private const float ChordDuration = 4.0f; // Each chord plays for 4 seconds
        private const float MusicVolume = 0.3f;

        public override void _Ready()
        {
            // Create audio generator
            _generator = new AudioStreamGenerator
            {
                MixRate = SampleRate,
                BufferLength = 0.1f // 100ms buffer
            };

            // Create audio player
            _audioPlayer = new AudioStreamPlayer
            {
                Stream = _generator,
                VolumeDb = Mathf.LinearToDb(MusicVolume),
                Autoplay = true
            };
            
            AddChild(_audioPlayer);
            
            GD.Print("Music manager initialized");
        }

        public override void _Process(double delta)
        {
            if (!_isPlaying || _audioPlayer == null)
                return;

            _timeElapsed += delta;
            
            // Update chord progression
            int newChord = (int)(_timeElapsed / ChordDuration) % _chordFrequencies.Length;
            if (newChord != _currentChord)
            {
                _currentChord = newChord;
            }

            FillAudioBuffer();
        }

        private void FillAudioBuffer()
        {
            var playback = (AudioStreamGeneratorPlayback)_audioPlayer.GetStreamPlayback();
            if (playback == null)
                return;

            int framesToFill = playback.GetFramesAvailable();
            
            // Don't fill too much at once to avoid audio artifacts
            framesToFill = Mathf.Min(framesToFill, 1024);
            
            if (framesToFill <= 0)
                return;

            // Get current chord frequencies
            var chordFreqs = _chordFrequencies[_currentChord];
            
            // Generate frames
            for (int i = 0; i < framesToFill; i++)
            {
                // Mix the three notes of the chord
                float sample = 0f;
                
                for (int note = 0; note < chordFreqs.Length; note++)
                {
                    float freq = chordFreqs[note];
                    float amplitude = 0.15f / chordFreqs.Length; // Divide by number of notes
                    
                    // Add harmonics for richer sound
                    sample += amplitude * Mathf.Sin(_phase * freq * 2f * Mathf.Pi / SampleRate);
                    sample += amplitude * 0.5f * Mathf.Sin(_phase * freq * 2f * 2f * Mathf.Pi / SampleRate); // Octave
                }
                
                // Apply gentle envelope to smooth transitions
                float envelopePosition = (float)((_timeElapsed % ChordDuration) / ChordDuration);
                float envelope = 1.0f;
                
                // Fade in at start of chord
                if (envelopePosition < 0.1f)
                    envelope = envelopePosition / 0.1f;
                // Fade out at end of chord
                else if (envelopePosition > 0.9f)
                    envelope = (1.0f - envelopePosition) / 0.1f;
                
                sample *= envelope;
                
                // Push stereo frame (same for left and right)
                playback.PushFrame(new Vector2(sample, sample));
                
                _phase += 1f;
            }
        }

        public void ToggleMusic()
        {
            _isPlaying = !_isPlaying;
            
            if (_audioPlayer != null)
            {
                if (_isPlaying)
                {
                    _audioPlayer.Play();
                    GD.Print("Music enabled");
                }
                else
                {
                    _audioPlayer.Stop();
                    GD.Print("Music disabled");
                }
            }
        }

        public void SetVolume(float linearVolume)
        {
            if (_audioPlayer != null)
            {
                _audioPlayer.VolumeDb = Mathf.LinearToDb(Mathf.Clamp(linearVolume, 0.0001f, 1.0f));
            }
        }

        public bool IsPlaying => _isPlaying;
    }
}
