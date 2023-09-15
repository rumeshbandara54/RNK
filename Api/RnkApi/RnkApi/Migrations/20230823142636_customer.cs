using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RnkApi.Migrations
{
    public partial class customer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    c_FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    c_Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    c_ContactNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    c_Nic = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    modifiedUser = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    c_AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    token = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
