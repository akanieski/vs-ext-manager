using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace server.Migrations
{
    public partial class AddBlobs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "Packages");

            migrationBuilder.AddColumn<long>(
                name: "ContentId",
                table: "Packages",
                nullable: true,
                defaultValue: null);

            migrationBuilder.AddColumn<long>(
                name: "IconId",
                table: "Packages",
                nullable: true,
                defaultValue: null);

            migrationBuilder.CreateTable(
                name: "Blobs",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ContentLength = table.Column<int>(nullable: false),
                    Data = table.Column<byte[]>(nullable: true),
                    Filename = table.Column<string>(nullable: true),
                    MimeType = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blobs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Packages_ContentId",
                table: "Packages",
                column: "ContentId");

            migrationBuilder.CreateIndex(
                name: "IX_Packages_IconId",
                table: "Packages",
                column: "IconId");

            migrationBuilder.AddForeignKey(
                name: "FK_Packages_Blobs_ContentId",
                table: "Packages",
                column: "ContentId",
                principalTable: "Blobs",
                principalColumn: "Id",
                onUpdate: ReferentialAction.NoAction,
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Packages_Blobs_IconId",
                table: "Packages",
                column: "IconId",
                principalTable: "Blobs",
                principalColumn: "Id",
                onUpdate: ReferentialAction.NoAction,
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Packages_Blobs_ContentId",
                table: "Packages");

            migrationBuilder.DropForeignKey(
                name: "FK_Packages_Blobs_IconId",
                table: "Packages");

            migrationBuilder.DropTable(
                name: "Blobs");

            migrationBuilder.DropIndex(
                name: "IX_Packages_ContentId",
                table: "Packages");

            migrationBuilder.DropIndex(
                name: "IX_Packages_IconId",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "ContentId",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "IconId",
                table: "Packages");

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Packages",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "Packages",
                nullable: true);
        }
    }
}
