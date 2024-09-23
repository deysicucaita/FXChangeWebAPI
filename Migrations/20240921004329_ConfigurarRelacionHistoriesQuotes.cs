using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FXChangeWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class ConfigurarRelacionHistoriesQuotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AdditionalInfo",
                table: "Histories",
                newName: "CurrencyPair");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CurrencyPair",
                table: "Histories",
                newName: "AdditionalInfo");
        }
    }
}
