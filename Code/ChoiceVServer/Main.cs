using AltV.Net;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using Microsoft.EntityFrameworkCore;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Renci.SshNet;
using System.IO;
using ChoiceVServer.Controller.Discord;
using File = System.IO.File;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ChoiceVServer {
    public class Main : RealScript {
        public override IEntityFactory<IVehicle> GetVehicleFactory() {
            return new ChoiceVVehicleFactory();
        }

        public Main() { }

        private static readonly List<ChoiceVScript> LoadedTypes = new List<ChoiceVScript>();
        
        public override void OnStart() {
            loadConfig();
            loadLogging();

            var allTypes = Assembly.GetExecutingAssembly().GetTypes();
            LoadedTypes.Add((ChoiceVScript)Activator.CreateInstance(Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(i => i.Name == "WorldController")));
            foreach(var item in allTypes) {
                //Load WorldController first
                if(item.IsSubclassOf(typeof(ChoiceVScript)) && !item.IsAbstract && item.Name != "WorldController") {
                    Logger.logInfo(LogCategory.System, LogActionType.Created, $"Loading class {item.Name}");
                    var o = (ChoiceVScript)Activator.CreateInstance(item);

                    // Important! Prevent Garbage Collection
                    LoadedTypes.Add(o);
                }

                if(item.IsSubclassOf(typeof(Resource)) && (item != typeof(Main)) && !item.IsAbstract)
                    throw new InvalidOperationException($"Derivates of Script are not allowed! ({item.Name}). Use abstract class ChoiceVScript!");
            }
            
            loadAllResourcesToCdn();

            databaseRunthrough();

            InvokeController.Start();
            InvokeController.AddTimedInvoke("Check-Killer", (i) => {
                if(File.Exists("kill")) {
                    File.Delete("kill");
                    Logger.logInfo(LogCategory.System, LogActionType.Removed, "Kill-File detected. Shutting down server.");
                    Alt.Resource.Stop();
                    Alt.Core.StopServer();

                    Environment.Exit(0);
                }
            }, TimeSpan.FromSeconds(1), true);
            
            EventController.onMainReady();
        }

        private static void loadAllResourcesToCdn() {
            if(Config.IsUploadResourcesToCDN) {
                var config = Alt.GetServerConfig();
                var processStartInfo = new ProcessStartInfo {
                    FileName = "altv-server.exe",
                    Arguments = $"--host {Config.CDNConnectServerIp} --port {config.Get("port").GetInt()} --justpack",
                    UseShellExecute = false,
                };

                var process = Process.Start(processStartInfo);
                process?.WaitForExit();

                var updatedList = new List<string>();
                using(var sftpClient = new SftpClient(Config.CDNFtpAddress, 22, Config.CDNFtpUser, Config.CDNFtpPassword)) {
                    sftpClient.Connect();
                    if(sftpClient.IsConnected) {
                        foreach(var file in Directory.GetFiles("cdn_upload/")) {
                            var fileInfo = new FileInfo(file);
                            if(sftpClient.Exists(fileInfo.Name)) {
                                sftpClient.UploadFile(File.OpenRead(file), fileInfo.Name, true);
                                updatedList.Add(fileInfo.Name);
                            } else {
                                sftpClient.UploadFile(File.OpenRead(file), fileInfo.Name);
                                updatedList.Add(fileInfo.Name);
                                Logger.logTrace(LogCategory.ServerStartup, LogActionType.Event, $"File {fileInfo.Name} did not already exist on CDN and was uploaded.");
                            }
                        }
                    } else {
                        throw new Exception("Could not update resources on the CDN");
                    }
                }

                Logger.logInfo(LogCategory.ServerStartup, LogActionType.Event, $"Uploaded {updatedList.Count} files to CDN.");
                System.Threading.Thread.Sleep(5000);
            }
        }

        private void loadLogging() {
            //Register the Custom AltV-Target-Logger
            ConfigurationItemFactory.Default.Targets.RegisterDefinition("AltV", typeof(NLogAltV));

            var processModule = Process.GetCurrentProcess().MainModule;
            if(processModule != null) {
                //Create an NLog-Config
                var config = new LoggingConfiguration();

                // Targets where to log to: File and ALTV-Console
                var logAltV = new NLogAltV();
                var logfile = new FileTarget("logfile") { FileName = "logs/log-all.csv" };
                var exepFile = new FileTarget("logfile") { FileName = "logs/log-errors.csv" };

                // Set the File-Log to CSV-Layout
                var csvLayout = new CsvLayout();
                csvLayout.Columns.Add(new CsvColumn("time", Layout.FromString("${longdate}")));
                csvLayout.Columns.Add(new CsvColumn("level",
                    Layout.FromString("${level:upperCase=true}")));
                csvLayout.Columns.Add(new CsvColumn("message", Layout.FromString("${message}")));
                csvLayout.Columns.Add(new CsvColumn("callsite",
                    Layout.FromString("${callsite:includeSourcePath=true:skipFrames=1}")));
                csvLayout.Columns.Add(new CsvColumn("stacktrace",
                    Layout.FromString("${stacktrace:topFrames=10:skipFrames=1}")));
                csvLayout.Columns.Add(new CsvColumn("exception",
                    Layout.FromString("${exception:format=ToString}")));
                csvLayout.Delimiter = CsvColumnDelimiterMode.Tab;

                logfile.Layout = csvLayout;
                exepFile.Layout = csvLayout;

                // Max-Filesize of log = 500MB
                logfile.ArchiveNumbering = ArchiveNumberingMode.DateAndSequence;
                logfile.ArchiveAboveSize = 524288000;

                exepFile.ArchiveNumbering = ArchiveNumberingMode.DateAndSequence;
                exepFile.ArchiveAboveSize = 524288000;

                // Sets the AltV-Layout to the known format
                logAltV.Layout = Layout.FromString("[${date:format=HH\\:mm\\:ss}][${level}] ${message}");

                // Rules for mapping loggers to targets            
                if(Config.IsDevServer) {
                    config.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, logAltV);
                } else {
                    config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logAltV);
                }
                config.AddRule(NLog.LogLevel.Warn, NLog.LogLevel.Fatal, exepFile);
                config.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, logfile);

                // Apply config           
                NLog.LogManager.Configuration = config;
            }
        }

        //Here all Config Data (isDevserver, specific weather, etc. are read) are written in the Constants.
        private void loadConfig() {
            var document = XDocument.Load("ChoiceVConfig.xml");

            var root = document.Root;
            var results =
              root?.Elements()
                .ToDictionary(element => element.Name.ToString(), element => element.Value);


            var type = typeof(Config);
            if(results != null)
                foreach(var key in results.Keys) {
                    var property = type.GetProperty(key);

                    var propertyInfo = type.GetProperty(key);
                    var propertyType = propertyInfo?.PropertyType;
                    if (propertyType != null) {
                        var value = Convert.ChangeType(results[key], propertyType);
                        property?.SetValue(type, value);
                    }
                }


            Logger.logDebug(LogCategory.ServerStartup, LogActionType.Viewed, "Database Schema " + Config.DatabaseDatabase + " loaded!");
            Logger.logInfo(LogCategory.ServerStartup, LogActionType.Event, "ChoiceV-Server-Version: " + Config.ServerVersion);
        }

        //Put your foreach Database Stuff here. So that, when you have to initialize something like garages another class can (in the same runthrough) also initialize something if needed
        private static void databaseRunthrough() {
            using(var db = new ChoiceVDb()) {
                foreach(var row in db.inventoryspots) {
                    InventorySpot.load(row);
                }
            }
        }

        public override void OnStop() {
            EventController.onMainShutdown();
        }

        private static uint LastLongTick;
        private static uint LastTick;

        public override void OnTick() {
            base.OnTick();

            try {
                var tCount = (uint)Environment.TickCount;

                if((tCount - LastLongTick) > 200) {
                    LastLongTick = tCount;
                    EventController.onLongTick();
                }

                if((tCount - LastTick) > 50) {
                    LastTick = tCount;
                    EventController.onTick();
                }
            } catch(Exception e) {
                Logger.logException(e);
            }
        }
    }
}
