using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnTrack.Backend.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddedTasksToAppUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Tasks_TaskId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_TaskId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TaskId",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "AppUserTask",
                columns: table => new
                {
                    AssignedMembersId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TasksId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUserTask", x => new { x.AssignedMembersId, x.TasksId });
                    table.ForeignKey(
                        name: "FK_AppUserTask_AspNetUsers_AssignedMembersId",
                        column: x => x.AssignedMembersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppUserTask_Tasks_TasksId",
                        column: x => x.TasksId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppUserTask_TasksId",
                table: "AppUserTask",
                column: "TasksId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppUserTask");

            migrationBuilder.AddColumn<Guid>(
                name: "TaskId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_TaskId",
                table: "AspNetUsers",
                column: "TaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Tasks_TaskId",
                table: "AspNetUsers",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id");
        }
    }
}
