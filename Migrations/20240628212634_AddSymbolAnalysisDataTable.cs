using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockMarket.Migrations
{
    /// <inheritdoc />
    public partial class AddSymbolAnalysisDataTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SymbolAnalysisData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Symbol = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Buy = table.Column<int>(type: "int", nullable: true),
                    Neutral = table.Column<int>(type: "int", nullable: true),
                    Sell = table.Column<int>(type: "int", nullable: true),
                    Signal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Adx = table.Column<double>(type: "float", nullable: true),
                    Trending = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SymbolAnalysisData", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SymbolAnalysisData");
        }
    }
}
