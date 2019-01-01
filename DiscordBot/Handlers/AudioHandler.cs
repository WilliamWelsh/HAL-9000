using System;
using Discord;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord.Commands;
using Discord.Audio;
using System.Threading;
using System.Speech.Recognition;
using System.IO;
using Discord.Audio;
using System.Speech.Synthesis;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Speech.AudioFormat;
using System.Globalization;
using NAudio.Wave;
using System.Media;

namespace Gideon.Handlers
{
    class AudioHandler
    {
        /*









            I know this file is horrific. There's extra dependencies, meaningless code, and whatnot.
            My goal was to have voice commands. They work, so far, with my *computer* microphone, but
            I'm trying to get it to listen to my discord microphone with user.AudioStream. Copying the
            stream to a memorystream is corrupt for some reason, and I will eventually figure it out.
            I'm writing this in case someone finds this.
            It'll get fixed eventually.
            Hopefully.
















        */
        static ISocketMessageChannel c;
        static SocketGuild Guild;
        static SocketCommandContext context;
        static IAudioClient audioClient;

        static MemoryStream memoryStream = new MemoryStream();
        //static AudioInStream userAudioStream;

        public DiscordSocketClient client;

        List<SocketGuildUser> users = new List<SocketGuildUser>();

        //bool listen = true;
        static SpeechSynthesizer synthesizer = new SpeechSynthesizer();
        public async Task Join(SocketCommandContext Context)
        {
            context = Context;
            var channel = Context.Guild.GetVoiceChannel(294699220743618562);
            c = Context.Guild.GetTextChannel(518186074162331648);
            Guild = Context.Guild;

            (await channel.ConnectAsync()).Dispose();

            audioClient = await channel.ConnectAsync();

            synthesizer.Volume = 100;
            synthesizer.Rate = 1;
            //synthesizer.SelectVoice("Microsoft Server Speech Text to Speech Voice (en-US, Helen)");
            synthesizer.SetOutputToWaveFile("_voice.wav");

            await Speak("Hello");

            //userAudioStream = Guild.GetUser(354458973572956160).AudioStream;
            //await Guild.GetUser(354458973572956160).AudioStream.CopyToAsync(memoryStream);
            //Task.Factory.StartNew(PipeAudioStream);

            StartRecognition();
        }

        //public void PipeAudioStream()
        //{
        //    while (listen)
        //    {
        //        userAudioStream.CopyToAsync(memoryStream);
        //        //if (memoryStream.Length > 20000)
        //        //    memoryStream = new MemoryStream();
        //    }
        //}

        //public void MakeWav()
        //{
        //    listen = false;
        //    byte[] buffer = new byte[16 * 1024];
        //    memoryStream.Position = 0;
        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        int read;
        //        while ((read = memoryStream.Read(buffer, 0, buffer.Length)) > 0)
        //        {
        //            ms.Write(buffer, 0, read);
        //        }
        //        //ms.ToArray();
        //        //File.WriteAllBytes("_recored.wav", ms.ToArray());
        //        using (System.Media.SoundPlayer sound = new System.Media.SoundPlayer(ms))
        //        {
        //            sound.Play();
        //        }
        //    }

            //memoryStream.Seek(0, SeekOrigin.Begin);
            //FileStream fs = File.Create("_output.wav");
            //byte[] buf = new byte[65536];
            //int len = 0;
            //while ((len = memoryStream.Read(buf, 0, 65536)) > 0)
            //{
            //    fs.Write(buf, 0, len);
            //}
            //fs.Close();

            //first option
            //MemoryStream ms = new MemoryStream(bytes);
            //SoundPlayer myPlayer = new SoundPlayer(ms);
            //myPlayer.Play();

            ////second option
            //using (FileStream fs = File.Create("output.wav"))
            //{
            //    fs.Write(bytes, 0, bytes.Length);
            //}
            //myPlayer = new SoundPlayer("myFile.wav");
            //myPlayer.Play();
        //}

        void StartRecognition()
        {
            using ( SpeechRecognitionEngine recognizer = new SpeechRecognitionEngine(new CultureInfo("en-US")))
            {

                Choices sentences = new Choices();
                List<string> names = new List<string>();
                foreach (var user in Guild.Users)
                {
                    if (user.Nickname != null)
                        names.Add(user.Nickname + user.Discriminator);
                    names.Add(user.Username + user.Discriminator);

                    users.Add(user);
                }
                foreach (var name in names)
                {
                    sentences.Add("gideon kick " + name);
                    //sentences.Add("gideon mention " + name);
                    sentences.Add("gideon chat mute " + name);
                }
                sentences.Add("Gideon");
                sentences.Add("Gideon Ping");

                GrammarBuilder gb = new GrammarBuilder(sentences);

                recognizer.LoadGrammar(new Grammar(gb));

                recognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(recognizer_SpeechRecognized);

                recognizer.SetInputToDefaultAudioDevice();
                //    recognizer.SetInputToAudioStream(memoryStream, new SpeechAudioFormatInfo(
                //48000, AudioBitsPerSample.Sixteen, AudioChannel.Mono));
                //    Console.WriteLine("Set audio stream.");

                recognizer.RecognizeAsync(RecognizeMode.Multiple);

                while (true)
                {
                    Console.ReadLine();
                }
            }
        }

        private async Task Speak(string text)
        {
            synthesizer.Speak(text);
            await SendAsync();
        }

        void recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            SaySomething(e.Result.Text).ConfigureAwait(false);
            Console.WriteLine("Recognized text: " + e.Result.Text);
        }

        private async Task SaySomething(string text)
        {
            await c.SendMessageAsync("Reverse said " + text);
            if (text.StartsWith("kick"))
            {
                text = text.Replace("kick ", "");
                string username = text.Remove(text.Length - 4);
                string discriminator = text.Substring(text.Length - 4);
                SocketGuildUser target = null;
                foreach (var user in users)
                {
                    if (user.Username == username && discriminator == user.Discriminator)
                        target = user;
                }
            }

            if (text.StartsWith("chat mute"))
            {
                text = text.Replace("chat mute ", "");
                string username = text.Remove(text.Length - 4);
                string discriminator = text.Substring(text.Length - 4);
                SocketGuildUser target = null;
                foreach (var user in users)
                {
                    if (user.Username == username && discriminator == user.Discriminator)
                    {
                        target = user;
                        break;
                    }
                }
                await ChatMutePlayer(target);
            }

            if (text == "Gideon")
                await Speak("Yes Reverse");

            if (text == "Gideon Ping")
                await Speak("Pong!");
        }

        private static Process CreateStream() => Process.Start(new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-i _voice.wav -ac 2 -bitexact -f s16le -ar 48000 pipe:1",
            UseShellExecute = false,
            RedirectStandardOutput = true,
        });

        private static async Task SendAsync()
        {
            var ffmpeg = CreateStream();
            var output = ffmpeg.StandardOutput.BaseStream;
            var discord = audioClient.CreatePCMStream(AudioApplication.Mixed);
            await output.CopyToAsync(discord);
            await discord.FlushAsync();
        }

        private async Task ChatMutePlayer(SocketGuildUser user)
        {
            await user.AddRoleAsync(Guild.Roles.FirstOrDefault(x => x.Name == "Low IQ"));
            await Guild.GetTextChannel(294699220743618561).SendMessageAsync("", false, Utilities.Embed("Voice Command", $"{user.Username ?? user.Nickname} has been chat muted.", new Color(31, 139, 76), "", user.GetAvatarUrl()));
            await Speak($"Successfully chat muted {user.Username ?? user.Nickname}.");
        }
    }
}
