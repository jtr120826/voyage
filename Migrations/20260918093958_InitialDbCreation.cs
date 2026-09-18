using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reiseplaner.Migrations
{
    /// <inheritdoc />
    public partial class InitialDbCreation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reisen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Titel = table.Column<string>(type: "TEXT", nullable: false),
                    Zielort = table.Column<string>(type: "TEXT", nullable: false),
                    Startdatum = table.Column<string>(type: "TEXT", nullable: false),
                    Enddatum = table.Column<string>(type: "TEXT", nullable: false),
                    Budget = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reisen", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Programmpunkte",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Titel = table.Column<string>(type: "TEXT", nullable: false),
                    Datum = table.Column<string>(type: "TEXT", nullable: false),
                    Kategorie = table.Column<string>(type: "TEXT", nullable: false),
                    Kosten = table.Column<decimal>(type: "TEXT", nullable: false),
                    Erledigt = table.Column<bool>(type: "INTEGER", nullable: false),
                    ReiseId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Programmpunkte", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Programmpunkte_Reisen_ReiseId",
                        column: x => x.ReiseId,
                        principalTable: "Reisen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Programmpunkte_ReiseId",
                table: "Programmpunkte",
                column: "ReiseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Programmpunkte");

            migrationBuilder.DropTable(
                name: "Reisen");
        }
    }
}
