    namespace HololensTemplate.Utils {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
#if !UNITY_EDIOR && UNITY_WSA
    using Windows.Foundation.Diagnostics;
    using Windows.Storage;
#endif


    using UnityEngine;
    /// <summary>
    /// Class listing every <code>Debug.Log()</code> called from the code, in a file in temp storage
    /// </summary>
    public class Logs {
        /// <summary>
        /// Logs start listening if in UWP
        /// </summary>
        public static void Init() {
        #if !UNITY_EDITOR && UNITY_WSA
            Application.logMessageReceived += Instance.ApplicationOnLogMessageReceived;
        #endif
        }
        /// <summary>
        /// Logs stop listening if in UWP
        /// </summary>
        public static void Stop() {
        #if !UNITY_EDITOR && UNITY_WSA
            Application.logMessageReceived -= Instance.ApplicationOnLogMessageReceived;
        #endif
        }
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

        /// <summary>
        ///     List of string which will be printed in the header of the log
        /// </summary>
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
            //Creating channel for logging in Holo Web portail 
            loggingChannel = new LoggingChannel(Application.productName, null, new Guid());
            fls            = new FileLoggingSession(Application.productName);
            fls.AddLoggingChannel(loggingChannel);

            WritingTask = Task.Run(async () => {
                //While logs instance exist, wait for a task for writing
                while (Writing)
                    if (queue.Count > 0)
                        queue.Dequeue()();
                    else
                        await Task.Delay(1000);
                // Clearing the queue before disposing
                while (queue.Count < 0)
                    queue.Dequeue()();
            });
        }

        /// <summary>
        ///     Handle a log message from Untiy Debug
        /// </summary>
        /// <param name="condition">The message</param>
        /// <param name="stackTrace"></param>
        /// <param name="type">The type of message (Warning,...)</param>
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

        /// <summary>
        ///     Dispose the stream, wait for evrything is clear
        /// </summary>
        ~Logs() {
            Stop();
            Task.Run(async () => {
                Writing = false;
                await WritingTask;
                StorageFile file = await fls.CloseAndSaveToFileAsync();
                streamWriter.Dispose();
                GC.Collect();
            });
        }

        /// <summary>
        ///     Task creating the log file in uwp
        /// </summary>
        private async void Create() {
            StorageFolder folder = ApplicationData.Current.TemporaryFolder;
            StorageFile   file   = null;

            file = await folder.CreateFileAsync("Logs.log", CreationCollisionOption.OpenIfExists);
            Stream stream = await file.OpenStreamForWriteAsync();
            if (stream.Length > 0)
                stream.Position = stream.Length - 1;
            streamWriter = new StreamWriter(stream);
            LogHeader(stream);
        }

        /// <summary>
        ///     Function writing the header of any files, using the UWP
        ///     <value>streamWriter</value>
        /// </summary>
        /// <param name="baseStream"></param>
        private async void LogHeader(Stream baseStream) {
            if (baseStream.Length > 0)
                await streamWriter.WriteLineAsync();

            string longest = Header.ToList().Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur);
            int    size    = longest.Length;

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