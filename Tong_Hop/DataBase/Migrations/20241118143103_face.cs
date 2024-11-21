using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataBase.Migrations
{
    /// <inheritdoc />
    public partial class face : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FaceFeatures_Students_StudentId",
                table: "FaceFeatures");

            migrationBuilder.DropIndex(
                name: "IX_FaceFeatures_StudentId",
                table: "FaceFeatures");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "FaceFeatures",
                newName: "StudentID");

            migrationBuilder.RenameColumn(
                name: "FeatureData",
                table: "FaceFeatures",
                newName: "img");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "FaceFeatures",
                newName: "Guid");

            migrationBuilder.AddColumn<Guid>(
                name: "StudentsId",
                table: "FaceFeatures",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FaceFeatures_StudentsId",
                table: "FaceFeatures",
                column: "StudentsId");

            migrationBuilder.AddForeignKey(
                name: "FK_FaceFeatures_Students_StudentsId",
                table: "FaceFeatures",
                column: "StudentsId",
                principalTable: "Students",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FaceFeatures_Students_StudentsId",
                table: "FaceFeatures");

            migrationBuilder.DropIndex(
                name: "IX_FaceFeatures_StudentsId",
                table: "FaceFeatures");

            migrationBuilder.DropColumn(
                name: "StudentsId",
                table: "FaceFeatures");

            migrationBuilder.RenameColumn(
                name: "StudentID",
                table: "FaceFeatures",
                newName: "StudentId");

            migrationBuilder.RenameColumn(
                name: "img",
                table: "FaceFeatures",
                newName: "FeatureData");

            migrationBuilder.RenameColumn(
                name: "Guid",
                table: "FaceFeatures",
                newName: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_FaceFeatures_StudentId",
                table: "FaceFeatures",
                column: "StudentId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FaceFeatures_Students_StudentId",
                table: "FaceFeatures",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
