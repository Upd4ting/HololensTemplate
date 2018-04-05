#if UNITY_EDITOR
using UnityEngine;
#elif UNITY_WSA
#endif

namespace Assets.Scripts {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Windows.Foundation.Diagnostics;
    using Windows.Storage;

    using UnityEngine;

    public class Logs {
    #if !UNITY_EDITOR && UNITY_WSA
        private static          Logs               Instance => lazy.Value;
        private static readonly Lazy<Logs>         lazy = new Lazy<Logs>(() => new Logs());
        private const           string             guid = "4dc2826e-54a1-4ba9-bf63-92b73ea1ac4a";
        private                 StreamWriter       streamWriter;
        private readonly        LoggingChannel     loggingChannel;
        private readonly        FileLoggingSession fls;
        private readonly        Queue<Action>      queue   = new Queue<Action>();
        private readonly        SemaphoreSlim      ss      = new SemaphoreSlim(1, 1);
        private                 bool               Writing = true;
        private readonly        Task               WritingTask;

        public static void Init() {
            Logs l = Instance;
        }

        public static List<string> Header { get; } = new List<string> {
            $"Application:   {Application.productName}",
            $"Company:       {Application.companyName}",
            $"Platform:      {Application.platform.ToString()}",
            $"Unityversion:  {Application.unityVersion}",
            $"Version:       {Application.version}",
            $"Starting time: {DateTime.Now.ToString(CultureInfo.CurrentCulture)}"
        };

        private Logs() {
            Create();
            loggingChannel = new LoggingChannel(Application.productName, null, new Guid());
            fls            = new FileLoggingSession(Application.productName);
            fls.AddLoggingChannel(loggingChannel);
            Application.logMessageReceived += ApplicationOnLogMessageReceived;
            WritingTask = Task.Run(async () => {
                while (Writing)
                    if (queue.Count > 0)
                        queue.Dequeue()();
                    else
                        await Task.Delay(1000);
            });
        }

        private void ApplicationOnLogMessageReceived(string condition, string stackTrace, LogType type) {
            LoggingLevel level;
            switch (type) {
                case LogType.Error:
                    level = LoggingLevel.Error;
                    break;
                case LogType.Assert:
                    level = LoggingLevel.Critical;
                    break;
                case LogType.Warning:
                    level = LoggingLevel.Warning;
                    break;
                case LogType.Log:
                    level = LoggingLevel.Information;
                    break;
                case LogType.Exception:
                    level = LoggingLevel.Critical;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            loggingChannel.LogMessage(condition, level);
            float timeStartup = Time.realtimeSinceStartup;

            queue.Enqueue(async () => {
                ss.Wait();
                await streamWriter.WriteLineAsync($"[{DateTime.Now.ToString("hh:mm:ss", CultureInfo.CurrentCulture)},{string.Format("{0:0.00000}", timeStartup)}] {level.ToString()}: {condition}");
                await streamWriter.FlushAsync();
                ss.Release();
            });
        }

        ~Logs() {
            Task.Run(async () => {
                Writing = false;
                await WritingTask;
                StorageFile file = await fls.CloseAndSaveToFileAsync();
                streamWriter.Dispose();
                GC.Collect();
            });
            Application.logMessageReceived -= ApplicationOnLogMessageReceived;
        }

        private async void Create() {
            StorageFolder folder = ApplicationData.Current.TemporaryFolder;
            StorageFile   file   = null;

            file = await folder.CreateFileAsync("Logs.log", CreationCollisionOption.OpenIfExists);

            //todo FIX
            Debug.Log($"File: {file}");
            Stream stream = await file.OpenStreamForWriteAsync();
            if (stream.Length > 0)
                stream.Position = stream.Length - 1;
            streamWriter = new StreamWriter(stream);

            string longest = Header.ToList().Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur);
            int    size    = longest.Length;
            if (stream.Length > 0)
                await streamWriter.WriteLineAsync();

            await streamWriter.WriteAsync($"┌");
            for (var i = 0; i < size + 2; i++)
                await streamWriter.WriteAsync("─");
            await streamWriter.WriteLineAsync("┐");

            foreach (string s in Header)
                if (s == longest) { await streamWriter.WriteLineAsync($"│ {s} │"); } else {
                    await streamWriter.WriteAsync($"│ ");

                    await streamWriter.WriteAsync(s);

                    for (var i = 0; i < size - s.Length; i++)
                        await streamWriter.WriteAsync($" ");
                    await streamWriter.WriteLineAsync($" │");
                }

            await streamWriter.WriteAsync($"└");
            for (var i = 0; i < size + 2; i++)
                await streamWriter.WriteAsync("─");
            await streamWriter.WriteLineAsync($"┘");
            await streamWriter.WriteLineAsync();
            await streamWriter.FlushAsync();
        }
    #endif
    }
}