using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace server.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExtensionList",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PackageIdentifier = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Version = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtensionList", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Galleries",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DisplayName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Galleries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Extension",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    VsixId = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    ExtensionListId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Extension", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Extension_ExtensionList_ExtensionListId",
                        column: x => x.ExtensionListId,
                        principalTable: "ExtensionList",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Packages",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PackageIdentifier = table.Column<string>(nullable: true),
                    FileName = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Author = table.Column<string>(nullable: true),
                    Version = table.Column<string>(nullable: true),
                    Icon = table.Column<string>(nullable: true),
                    Preview = table.Column<string>(nullable: true),
                    Tags = table.Column<string>(nullable: true),
                    DatePublished = table.Column<DateTime>(nullable: false),
                    SupportedVersions = table.Column<string>(nullable: true),
                    License = table.Column<string>(nullable: true),
                    GettingStartedUrl = table.Column<string>(nullable: true),
                    ReleaseNotesUrl = table.Column<string>(nullable: true),
                    MoreInfoUrl = table.Column<string>(nullable: true),
                    Repo = table.Column<string>(nullable: true),
                    IssueTracker = table.Column<string>(nullable: true),
                    ExtensionListId = table.Column<long>(nullable: true),
                    GalleryId = table.Column<long>(nullable: false),
                    Included = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Packages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Packages_ExtensionList_ExtensionListId",
                        column: x => x.ExtensionListId,
                        principalTable: "ExtensionList",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Packages_Galleries_GalleryId",
                        column: x => x.GalleryId,
                        principalTable: "Galleries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Extension_ExtensionListId",
                table: "Extension",
                column: "ExtensionListId");

            migrationBuilder.CreateIndex(
                name: "IX_Packages_ExtensionListId",
                table: "Packages",
                column: "ExtensionListId");

            migrationBuilder.CreateIndex(
                name: "IX_Packages_GalleryId",
                table: "Packages",
                column: "GalleryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Extension");

            migrationBuilder.DropTable(
                name: "Packages");

            migrationBuilder.DropTable(
                name: "ExtensionList");

            migrationBuilder.DropTable(
                name: "Galleries");
        }
    }
}
