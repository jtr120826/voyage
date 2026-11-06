using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reiseplaner.Migrations
{
    /// <inheritdoc />
    public partial class AddBenutzer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Benutzer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Benutzername = table.Column<string>(type: "TEXT", nullable: false),
                    PasswortHash = table.Column<string>(type: "TEXT", nullable: false),
                    Salt = table.Column<string>(type: "TEXT", nullable: false),
                    Iterationen = table.Column<int>(type: "INTEGER", nullable: false),
                    Rolle = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Benutzer", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Benutzer_Benutzername",
                table: "Benutzer",
                column: "Benutzername",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Benutzer");
        }
    }
}
