using System;
using System.IO;
using System.Media;
using System.Reflection;
using System.Windows;

namespace MfanaBotCyberApplicationP3.Services
{
    public class AudioServices : IDisposable
    {
        private SoundPlayer player;
        private bool isPlaying = false;
        private bool useSystemSounds = true;

        public AudioServices()
        {
            try
            {
                player = new SoundPlayer();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Audio initialization error: {ex.Message}");
                useSystemSounds = true;
            }
        }

        /// <summary>
        /// Play greeting - tries multiple approaches
        /// </summary>
        public void PlayGreeting()
        {
            try
            {
                // Try embedded resource first
                if (TryPlayEmbeddedSound("mfana_greeting.wav"))
                    return;

                // Try file system
                string soundPath = GetSoundFilePath();
                if (!string.IsNullOrEmpty(soundPath) && File.Exists(soundPath))
                {
                    player.SoundLocation = soundPath;
                    player.Play();
                    isPlaying = true;
                    return;
                }

                // Fallback to system sound
                if (useSystemSounds)
                {
                    SystemSounds.Beep.Play();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Audio error: {ex.Message}");
                // Don't crash the app - just continue silently
            }
        }

        private bool TryPlayEmbeddedSound(string fileName)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                // Try multiple resource name formats
                string[] possibleNames = {
                    $"MfanaSecurityBotWPF.Resources.{fileName}",
                    $"MfanaSecurityBotWPF.{fileName}",
                    $"MfanaSecurityBotWPF.Assets.{fileName}"
                };

                foreach (string resourceName in possibleNames)
                {
                    using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                    {
                        if (stream != null)
                        {
                            player.Stream = stream;
                            player.Play();
                            isPlaying = true;
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Embedded sound error: {ex.Message}");
            }
            return false;
        }

        private string GetSoundFilePath()
        {
            try
            {
                // Check common locations
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string[] paths = {
                    Path.Combine(baseDir, "Resources", "mfana_greeting.wav"),
                    Path.Combine(baseDir, "mfana_greeting.wav"),
                    Path.Combine(baseDir, "Sounds", "mfana_greeting.wav"),
                    Path.Combine(baseDir, "Audio", "mfana_greeting.wav")
                };

                foreach (string path in paths)
                {
                    if (File.Exists(path))
                        return path;
                }

                // Try to find in parent directories
                string parent = Directory.GetParent(baseDir)?.FullName;
                if (parent != null)
                {
                    string fallbackPath = Path.Combine(parent, "Resources", "mfana_greeting.wav");
                    if (File.Exists(fallbackPath))
                        return fallbackPath;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"File search error: {ex.Message}");
            }
            return null;
        }

        public void PlayVoiceGreeting()
        {
            PlayGreeting();
        }

        public void StopSound()
        {
            try
            {
                if (isPlaying && player != null)
                {
                    player.Stop();
                    isPlaying = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Stop error: {ex.Message}");
            }
        }

        public bool IsPlaying => isPlaying;

        public void Dispose()
        {
            try
            {
                player?.Dispose();
                isPlaying = false;
            }
            catch { }
        }
    }
}