using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fall2024_Assignment3_hrhazelwood.Data.Migrations
{
    /// <inheritdoc />
    public partial class actorAI : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OverallSentiment",
                table: "Actor",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TweetSentiment",
                table: "Actor",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tweets",
                table: "Actor",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OverallSentiment",
                table: "Actor");

            migrationBuilder.DropColumn(
                name: "TweetSentiment",
                table: "Actor");

            migrationBuilder.DropColumn(
                name: "Tweets",
                table: "Actor");
        }
    }
}
