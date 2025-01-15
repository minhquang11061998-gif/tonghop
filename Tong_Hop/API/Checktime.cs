using DataBase.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace API
{
    public class Checktime : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(1); // Kiểm tra mỗi 10 giây

        public Checktime(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                        // Lấy danh sách các bài thi từ cơ sở dữ liệu
                        var exams = await dbContext.Exam_Rooms.AsNoTracking().ToListAsync(stoppingToken);

                        foreach (var exam in exams)
                        {
                            var currentTime = DateTime.Now;

                            // Kiểm tra trạng thái bài thi
                            int newStatus;
                            if (currentTime < exam.StartTime)
                            {
                                newStatus = 2; // Chưa đến giờ
                            }
                            else if (currentTime >= exam.StartTime && currentTime <= exam.EndTime)
                            {
                                newStatus = 1; // Đang thi
                            }
                            else
                            {
                                newStatus = 0; // Quá hạn
                            }

                            // Chỉ cập nhật nếu trạng thái thay đổi
                            if (exam.Status != newStatus)
                            {
                                var trackedExam = await dbContext.Exam_Rooms.FindAsync(exam.Id);
                                if (trackedExam != null)
                                {
                                    trackedExam.Status = newStatus;
                                    dbContext.Exam_Rooms.Update(trackedExam);
                                }
                            }
                        }

                        // Lưu thay đổi vào cơ sở dữ liệu
                        await dbContext.SaveChangesAsync(stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    // Log lỗi nếu có vấn đề xảy ra
                    Console.WriteLine($"Lỗi trong Checktime: {ex.Message}");
                }

                // Đợi khoảng thời gian trước khi kiểm tra lại
                await Task.Delay(_interval, stoppingToken);
            }
        }
	}
}
