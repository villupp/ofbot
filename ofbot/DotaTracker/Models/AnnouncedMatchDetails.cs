using Discord;
using OfBot.Api.Dota;

namespace OfBot.DotaTracker
{
    public enum Team
    {
        Radiant,
        Dire,
    }

    public class AnnouncedMatchDetails
    {
        private Int64 matchId;
        private Team winner;
        private int scoreRadiant;
        private int scoreDire;
        private Team announcedPlayerTeam;
        private TimeSpan gameLength;
        private List<Int64> announcedPlayerIds;
        private List<string> announcedPlayerNames;

        public AnnouncedMatchDetails(
            GetMatchDetailsResponse response,
            List<Int64> announcedPlayerIds,
            List<string> announcedPlayerNames
        )
        {
            this.announcedPlayerIds = announcedPlayerIds;
            this.announcedPlayerNames = announcedPlayerNames;
            this.matchId = response.result.match_id;

            // The player in index 0 is the deciding player to determine the winning team
            var playerIndex = response.result.players
                .Select(p => p.account_id)
                .ToList()
                .IndexOf(announcedPlayerIds[0]);

            this.announcedPlayerTeam = playerIndex < 5 ? Team.Radiant : Team.Dire;
            this.winner = response.result.radiant_win ? Team.Radiant : Team.Dire;
            this.scoreRadiant = response.result.radiant_score;
            this.scoreDire = response.result.dire_score;
            this.gameLength = TimeSpan.FromSeconds(response.result.duration);
        }

        public Embed BuildEmbed()
        {
            var playerNames = String.Join(
                ", ",
                this.announcedPlayerNames.ToList().SkipLast(1)
            );
            playerNames += this.announcedPlayerNames.Count > 1 ?
                $" and {this.announcedPlayerNames.Last()}" :
                this.announcedPlayerNames.Last();
            var dotabuffLink = $"https://www.dotabuff.com/matches/{matchId}";
            var opendotaLink = $"https://www.opendota.com/matches/{matchId}";
            var isWin = this.announcedPlayerTeam == this.winner;
            var gameResultText = isWin ? "Victory" : "Defeat";
            var radiantResultText = $"[" + (this.winner == Team.Radiant ? "W" : "L") + $"] {this.scoreRadiant}";
            var direResultText = $"{this.scoreDire} [" + (this.winner == Team.Dire ? "W" : "L") + "]";
            var gameTime = "";
            if (this.gameLength.Hours > 0)
            {
                gameTime += this.gameLength.ToString(@"hh\:");
            }
            gameTime += this.gameLength.ToString(@"mm\:");
            gameTime += this.gameLength.ToString(@"ss");

            var embed = new EmbedBuilder();
            embed.WithColor(isWin ? Color.DarkGreen : Color.DarkRed)
            .WithTitle($"{playerNames} played a match of Dota!")
            .WithDescription(
                $"**{gameResultText}** in {gameTime}\n" +
                $"{radiantResultText} - {direResultText}\n" +
                $"[OpenDota]({opendotaLink}) | [DOTABUFF]({dotabuffLink})")
            .WithUrl(opendotaLink);

            return embed.Build();
        }
    }
}