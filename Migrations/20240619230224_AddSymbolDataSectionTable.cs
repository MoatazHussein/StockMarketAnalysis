using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockMarket.Migrations
{
    /// <inheritdoc />
    public partial class AddSymbolDataSectionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SymbolDataSections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Symbol = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sector = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Price = table.Column<double>(type: "float", nullable: true),
                    ChangesPercentage = table.Column<double>(type: "float", nullable: true),
                    Change = table.Column<double>(type: "float", nullable: true),
                    DayLow = table.Column<double>(type: "float", nullable: true),
                    DayHigh = table.Column<double>(type: "float", nullable: true),
                    YearHigh = table.Column<double>(type: "float", nullable: true),
                    YearLow = table.Column<double>(type: "float", nullable: true),
                    MarketCap = table.Column<double>(type: "float", nullable: true),
                    PriceAvg50 = table.Column<double>(type: "float", nullable: true),
                    PriceAvg200 = table.Column<double>(type: "float", nullable: true),
                    Exchange = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Volume = table.Column<double>(type: "float", nullable: true),
                    AvgVolume = table.Column<double>(type: "float", nullable: true),
                    Open = table.Column<double>(type: "float", nullable: true),
                    PreviousClose = table.Column<double>(type: "float", nullable: true),
                    Eps = table.Column<double>(type: "float", nullable: true),
                    Pe = table.Column<double>(type: "float", nullable: true),
                    EarningsAnnouncement = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SharesOutstanding = table.Column<double>(type: "float", nullable: true),
                    Timestamp = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SymbolDataSections", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SymbolDataSections");
        }
    }
}
