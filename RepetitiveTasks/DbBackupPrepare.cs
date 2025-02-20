﻿using Discord.WebSocket;
using CryptoAlertsBot.ApiHandler;
using CryptoAlertsBot.Discord;
using System.Reflection;
using GenericApiHandler.Helpers;
using System.Text;
using CryptoAlertsBot.Helpers;

namespace CryptoAlertsBot.RepetitiveTasks
{
    public class DbBackupPrepare
    {
        private readonly Logger _logger;
        private readonly DbBackupExecute _dataBaseBackup;
        private readonly ConstantsHandler _constantsHandler;
        private readonly DiscordSocketClient _client;

        public DbBackupPrepare(Logger logger, DbBackupExecute dataBaseBackup, ConstantsHandler constantsHandler, DiscordSocketClient client)
        {
            _logger = logger;
            _dataBaseBackup = dataBaseBackup;
            _constantsHandler = constantsHandler;
            _client = client;
        }

        public void Initialize()
        {
            try
            {
                var timer = new System.Timers.Timer(1000 * 60 * 60 * 24); //It should be 1000 * 60 * 60 * 24 (1 day)
                timer.Start();
                timer.Elapsed += ExeBackup;
            }
            catch (Exception e)
            {
                _ = _logger.Log(exception: e);
            }
        }

        private async void ExeBackup(object? sender, System.Timers.ElapsedEventArgs elapsed)
        {
            try
            {
                List<Type> tableTypes = GenericHelpers.GetTypesInNamespace(Assembly.GetExecutingAssembly(), "CryptoAlertsBot.Models");

                string backupResult = await _dataBaseBackup.GetDataBaseBackup(tableTypes);

                var dbBackupChannel = _client.Guilds.First().GetChannel(ulong.Parse(await _constantsHandler.GetConstantAsync(ConstantsNames.BACKUP_DB_CHANNEL_ID)));

                using var stream = GenerateStreamFromString(backupResult);
                await (dbBackupChannel as SocketTextChannel).SendFileAsync(stream, $"CryptoAllertsBackup_{DateTime.Now:ddMMyyyy}.sql", "");
            }
            catch (Exception e)
            {
                _ = _logger.Log(exception: e);
            }
        }
        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }

}
