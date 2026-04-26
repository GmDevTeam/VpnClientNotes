using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VpnClientNotes.Migrations
{
    /// <inheritdoc />
    public partial class AddWatchDogSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WatchDogSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TrackCpu = table.Column<bool>(type: "bit", nullable: false),
                    TrackRam = table.Column<bool>(type: "bit", nullable: false),
                    TrackHdd = table.Column<bool>(type: "bit", nullable: false),
                    IntervalSeconds = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatchDogSettings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "WatchDogSettings",
                columns: new[] { "Id", "IntervalSeconds", "IsActive", "TrackCpu", "TrackHdd", "TrackRam" },
                values: new object[] { 1, 10, true, true, true, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WatchDogSettings");
        }
    }
}
