using DataBase.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext()
        {
            
        }

        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Role> Roles { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<Students> Students { get; set; }
        public DbSet<Teachers> Teachers { get; set; }
        public DbSet<Student_Class> Student_Classes { get; set; }
        public DbSet<Exam_Room_Student> Exam_Room_Students { get; set; }
        public DbSet<Exam_Room_Student_AnswerHistory> Exam_Room_Student_AnswerHistories { get; set; }
        public DbSet<Teacher_Subject> Teacher_Subjects { get; set; }
        public DbSet<Subjects> Subjects { get; set; }
        public DbSet<Subject_Grade> Subject_Grades { get; set; }
        public DbSet<PointType_Subject> PointType_Subjects { get; set; }
        public DbSet<Classes> Classes { get; set; }
        public DbSet<Notification_Class> Notification_Classes { get; set; }
        public DbSet<Notifications> Notifications { get; set; }
        public DbSet<Grades> Grades { get; set; }
        public DbSet<PointTypes> PointTypes { get; set; }
        public DbSet<Learning_Summary> Learning_Summaries { get; set; }
        public DbSet<Exam_Room> Exam_Rooms { get; set; }
        public DbSet<Rooms> Rooms { get; set; }
        public DbSet<Exam_Room_TestCode> Exam_Room_TestCodes { get; set; }
        public DbSet<ExamHistorys> ExamHistorys { get; set; }
        public DbSet<Exams> Exams { get; set; }
        public DbSet<Scores> Scores { get; set; }
        public DbSet<Semesters> Semesters { get; set; }
        public DbSet<SystemConfigs> SystemConfigs { get; set; }
        public DbSet<TestCode_TestQuestion> TestCode_TestQuestion { get; set; }
        public DbSet<TestCodes> TestCodes { get; set; } 
        public DbSet<TestQuestionAnswers> TestQuestionAnswers { get; set; }
        public DbSet<TestQuestions> TestQuestions { get; set; }
        public DbSet<Tests> Tests { get; set; }
        public DbSet<FaceFeatures> FaceFeatures { get; set; }
    }
}
