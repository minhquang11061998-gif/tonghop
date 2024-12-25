using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataBase.Migrations
{
    /// <inheritdoc />
    public partial class DataValide202412252010 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Grades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<int>(type: "int", maxLength: 30, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grades", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PointTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Semesters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Semesters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Subjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    IsViewed = table.Column<bool>(type: "bit", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Avartar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    LockedEndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastMordificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Exams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exams_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PointType_Subjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PointTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointType_Subjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PointType_Subjects_PointTypes_PointTypeId",
                        column: x => x.PointTypeId,
                        principalTable: "PointTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PointType_Subjects_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subject_Grades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    GradeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subject_Grades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subject_Grades_Grades_GradeId",
                        column: x => x.GradeId,
                        principalTable: "Grades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Subject_Grades_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<int>(type: "int", nullable: false),
                    Minute = table.Column<int>(type: "int", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    MaxStudent = table.Column<int>(type: "int", nullable: false),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PointTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tests_PointTypes_PointTypeId",
                        column: x => x.PointTypeId,
                        principalTable: "PointTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tests_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Students_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Teachers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teachers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teachers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TestCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TestsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestCodes_Tests_TestsId",
                        column: x => x.TestsId,
                        principalTable: "Tests",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TestQuestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    RightAnswer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedByName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TestId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TestsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestQuestions_Tests_TestsId",
                        column: x => x.TestsId,
                        principalTable: "Tests",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Learning_Summaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Attendance = table.Column<double>(type: "float", nullable: false),
                    Point15 = table.Column<double>(name: "Point_15", type: "float", nullable: false),
                    Point45 = table.Column<double>(name: "Point_45", type: "float", nullable: false),
                    PointMidterm = table.Column<double>(name: "Point_Midterm", type: "float", nullable: false),
                    PointFinal = table.Column<double>(name: "Point_Final", type: "float", nullable: false),
                    PointSummary = table.Column<double>(name: "Point_Summary", type: "float", nullable: false),
                    IsView = table.Column<bool>(type: "bit", nullable: false),
                    SemesterID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PointTypesId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Learning_Summaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Learning_Summaries_PointTypes_PointTypesId",
                        column: x => x.PointTypesId,
                        principalTable: "PointTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Learning_Summaries_Semesters_SemesterID",
                        column: x => x.SemesterID,
                        principalTable: "Semesters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Learning_Summaries_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Learning_Summaries_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Scores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Score = table.Column<double>(type: "float", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PointTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SubjectsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PointTypesId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Scores_PointTypes_PointTypesId",
                        column: x => x.PointTypesId,
                        principalTable: "PointTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Scores_Students_StudentsId",
                        column: x => x.StudentsId,
                        principalTable: "Students",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Scores_Subjects_SubjectsId",
                        column: x => x.SubjectsId,
                        principalTable: "Subjects",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Classes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    MaxStudent = table.Column<int>(type: "int", nullable: false),
                    TeacherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GradeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Classes_Grades_GradeId",
                        column: x => x.GradeId,
                        principalTable: "Grades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Classes_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Exam_Rooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeacherId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeacherId2 = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exam_Rooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exam_Rooms_Exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Exam_Rooms_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Exam_Rooms_Teachers_TeacherId1",
                        column: x => x.TeacherId1,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Exam_Rooms_Teachers_TeacherId2",
                        column: x => x.TeacherId2,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Teacher_Subjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeacherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teacher_Subjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teacher_Subjects_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Teacher_Subjects_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TestCode_TestQuestion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TestCodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TestQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TestCodesId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestCode_TestQuestion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestCode_TestQuestion_TestCodes_TestCodesId",
                        column: x => x.TestCodesId,
                        principalTable: "TestCodes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TestCode_TestQuestion_TestQuestions_TestQuestionId",
                        column: x => x.TestQuestionId,
                        principalTable: "TestQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TestQuestionAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Answer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TestQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestQuestionAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestQuestionAnswers_TestQuestions_TestQuestionId",
                        column: x => x.TestQuestionId,
                        principalTable: "TestQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notification_Classes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    NotificationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification_Classes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notification_Classes_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notification_Classes_Notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Student_Classes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JoinTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Student_Classes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Student_Classes_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Student_Classes_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Exam_Room_TestCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamRoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TestsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExamRoomId0 = table.Column<Guid>(name: "Exam_RoomId", type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exam_Room_TestCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exam_Room_TestCodes_Exam_Rooms_Exam_RoomId",
                        column: x => x.ExamRoomId0,
                        principalTable: "Exam_Rooms",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Exam_Room_TestCodes_Tests_TestsId",
                        column: x => x.TestsId,
                        principalTable: "Tests",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Exam_Room_Students",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChenkTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ExamRoomTestCodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamRoomTestCodeId0 = table.Column<Guid>(name: "Exam_Room_TestCodeId", type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exam_Room_Students", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exam_Room_Students_Exam_Room_TestCodes_Exam_Room_TestCodeId",
                        column: x => x.ExamRoomTestCodeId0,
                        principalTable: "Exam_Room_TestCodes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Exam_Room_Students_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Exam_Room_Student_AnswerHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamRoomStudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TestQuestionAnswerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamRoomStudentId0 = table.Column<Guid>(name: "Exam_Room_StudentId", type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exam_Room_Student_AnswerHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exam_Room_Student_AnswerHistories_Exam_Room_Students_Exam_Room_StudentId",
                        column: x => x.ExamRoomStudentId0,
                        principalTable: "Exam_Room_Students",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Exam_Room_Student_AnswerHistories_TestQuestionAnswers_TestQuestionAnswerId",
                        column: x => x.TestQuestionAnswerId,
                        principalTable: "TestQuestionAnswers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamHistorys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Score = table.Column<double>(type: "float", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExamRoomStudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamRoomStudentId0 = table.Column<Guid>(name: "Exam_Room_StudentId", type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamHistorys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamHistorys_Exam_Room_Students_Exam_Room_StudentId",
                        column: x => x.ExamRoomStudentId0,
                        principalTable: "Exam_Room_Students",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Classes_GradeId",
                table: "Classes",
                column: "GradeId");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_TeacherId",
                table: "Classes",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Exam_Room_Student_AnswerHistories_Exam_Room_StudentId",
                table: "Exam_Room_Student_AnswerHistories",
                column: "Exam_Room_StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Exam_Room_Student_AnswerHistories_TestQuestionAnswerId",
                table: "Exam_Room_Student_AnswerHistories",
                column: "TestQuestionAnswerId");

            migrationBuilder.CreateIndex(
                name: "IX_Exam_Room_Students_Exam_Room_TestCodeId",
                table: "Exam_Room_Students",
                column: "Exam_Room_TestCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Exam_Room_Students_StudentId",
                table: "Exam_Room_Students",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Exam_Room_TestCodes_Exam_RoomId",
                table: "Exam_Room_TestCodes",
                column: "Exam_RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Exam_Room_TestCodes_TestsId",
                table: "Exam_Room_TestCodes",
                column: "TestsId");

            migrationBuilder.CreateIndex(
                name: "IX_Exam_Rooms_ExamId",
                table: "Exam_Rooms",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_Exam_Rooms_RoomId",
                table: "Exam_Rooms",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Exam_Rooms_TeacherId1",
                table: "Exam_Rooms",
                column: "TeacherId1");

            migrationBuilder.CreateIndex(
                name: "IX_Exam_Rooms_TeacherId2",
                table: "Exam_Rooms",
                column: "TeacherId2");

            migrationBuilder.CreateIndex(
                name: "IX_ExamHistorys_Exam_Room_StudentId",
                table: "ExamHistorys",
                column: "Exam_Room_StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_SubjectId",
                table: "Exams",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Learning_Summaries_PointTypesId",
                table: "Learning_Summaries",
                column: "PointTypesId");

            migrationBuilder.CreateIndex(
                name: "IX_Learning_Summaries_SemesterID",
                table: "Learning_Summaries",
                column: "SemesterID");

            migrationBuilder.CreateIndex(
                name: "IX_Learning_Summaries_StudentId",
                table: "Learning_Summaries",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Learning_Summaries_SubjectId",
                table: "Learning_Summaries",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_Classes_ClassId",
                table: "Notification_Classes",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_Classes_NotificationId",
                table: "Notification_Classes",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_PointType_Subjects_PointTypeId",
                table: "PointType_Subjects",
                column: "PointTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PointType_Subjects_SubjectId",
                table: "PointType_Subjects",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Scores_PointTypesId",
                table: "Scores",
                column: "PointTypesId");

            migrationBuilder.CreateIndex(
                name: "IX_Scores_StudentsId",
                table: "Scores",
                column: "StudentsId");

            migrationBuilder.CreateIndex(
                name: "IX_Scores_SubjectsId",
                table: "Scores",
                column: "SubjectsId");

            migrationBuilder.CreateIndex(
                name: "IX_Student_Classes_ClassId",
                table: "Student_Classes",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Student_Classes_StudentId",
                table: "Student_Classes",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_UserId",
                table: "Students",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Subject_Grades_GradeId",
                table: "Subject_Grades",
                column: "GradeId");

            migrationBuilder.CreateIndex(
                name: "IX_Subject_Grades_SubjectId",
                table: "Subject_Grades",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Teacher_Subjects_SubjectId",
                table: "Teacher_Subjects",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Teacher_Subjects_TeacherId",
                table: "Teacher_Subjects",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_UserId",
                table: "Teachers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TestCode_TestQuestion_TestCodesId",
                table: "TestCode_TestQuestion",
                column: "TestCodesId");

            migrationBuilder.CreateIndex(
                name: "IX_TestCode_TestQuestion_TestQuestionId",
                table: "TestCode_TestQuestion",
                column: "TestQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_TestCodes_TestsId",
                table: "TestCodes",
                column: "TestsId");

            migrationBuilder.CreateIndex(
                name: "IX_TestQuestionAnswers_TestQuestionId",
                table: "TestQuestionAnswers",
                column: "TestQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_TestQuestions_TestsId",
                table: "TestQuestions",
                column: "TestsId");

            migrationBuilder.CreateIndex(
                name: "IX_Tests_PointTypeId",
                table: "Tests",
                column: "PointTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Tests_SubjectId",
                table: "Tests",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Exam_Room_Student_AnswerHistories");

            migrationBuilder.DropTable(
                name: "ExamHistorys");

            migrationBuilder.DropTable(
                name: "Learning_Summaries");

            migrationBuilder.DropTable(
                name: "Notification_Classes");

            migrationBuilder.DropTable(
                name: "PointType_Subjects");

            migrationBuilder.DropTable(
                name: "Scores");

            migrationBuilder.DropTable(
                name: "Student_Classes");

            migrationBuilder.DropTable(
                name: "Subject_Grades");

            migrationBuilder.DropTable(
                name: "SystemConfigs");

            migrationBuilder.DropTable(
                name: "Teacher_Subjects");

            migrationBuilder.DropTable(
                name: "TestCode_TestQuestion");

            migrationBuilder.DropTable(
                name: "TestQuestionAnswers");

            migrationBuilder.DropTable(
                name: "Exam_Room_Students");

            migrationBuilder.DropTable(
                name: "Semesters");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Classes");

            migrationBuilder.DropTable(
                name: "TestCodes");

            migrationBuilder.DropTable(
                name: "TestQuestions");

            migrationBuilder.DropTable(
                name: "Exam_Room_TestCodes");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "Grades");

            migrationBuilder.DropTable(
                name: "Exam_Rooms");

            migrationBuilder.DropTable(
                name: "Tests");

            migrationBuilder.DropTable(
                name: "Exams");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropTable(
                name: "Teachers");

            migrationBuilder.DropTable(
                name: "PointTypes");

            migrationBuilder.DropTable(
                name: "Subjects");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
