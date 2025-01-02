using Data.DTOs;
using DataBase.Data;
using DataBase.DTOs;
using DataBase.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestQuestionController : ControllerBase
    {
        private readonly AppDbContext _db;
        public TestQuestionController(AppDbContext db)
        {
            _db = db;
        }
        [HttpGet("get-question-details/{id}")]
        public async Task<List<listdetailquestion>> GetQuestionDetails(Guid id)
        {
            var questionDetails = await _db.TestQuestions
                .Where(q => q.TestId == id)
                .Select(q => new listdetailquestion
                {
                    Id = q.Id,
                    Questionname = q.QuestionName,
                    RightAnswer = q.RightAnswer,
                    Type = q.Type,
                    level = q.Level,
                    answer = q.TestQuestionAnswer.Select(a => new AnswerDTO
                    {
                        Answer = a.Answer,
                    }).ToList()
                }).ToListAsync();

            return questionDetails;
        }
        [HttpGet("Get-testcodes-by-testid")]
        public async Task<ActionResult<List<DetailDTO>>> GetTestCodesByTestId(Guid testId)
        {
            var testcodes = await _db.TestCodes
                .Where(tc => tc.TestId == testId) // Lọc theo TestId
                .Select(tc => new DetailDTO
                {
                    IdTestcode = tc.Id,
                    CodeTescode = tc.Code,
                    time = tc.Tests.Minute, // Thời gian bài kiểm tra
                    NameSubject = tc.Tests.Subject.Name, // Tên môn học
                    NameQuestion = _db.TestCode_TestQuestion
                        .Where(tcq => tcq.TestCodeId == tc.Id) // Lấy danh sách câu hỏi liên quan
                        .Select(tcq => new TestQuestionDTO
                        {
                            Id = tcq.TestQuestion.Id,
                            QuestionName = tcq.TestQuestion.QuestionName, // Nội dung câu hỏi
                            /*code = tc.test.Code*/ // Mã TestCode
                            Level = tcq.TestQuestion.Level, // Mức độ câu hỏi
                            Type = tcq.TestQuestion.Type, // Loại câu hỏi
                            Answers = tcq.TestQuestion.TestQuestionAnswer // Truy cập trực tiếp từ quan hệ
                                .Select(a => new AnswerDTO
                                {
                                    Id = a.Id,
                                    Answer = a.Answer // Nội dung câu trả lời
                                }).ToList()
                        }).ToList()
                }).ToListAsync();

            return testcodes;
        }

        [HttpGet("get-all-testquestion")]
        public async Task<ActionResult<List<TestQuestion_AnswersDTO>>> GetAll()
        {
            var data = await _db.TestQuestions.ToListAsync();

            if (data == null)
            {
                return NotFound("Danh sach trong");
            }

            var dto = data.Select(x => new TestQuestion_AnswersDTO
            {
                Id = x.Id,
                QuestionName = x.QuestionName,
                QuestionType = x.Type,
                Level = x.Level,
                CorrectAnswers = x.RightAnswer.Split(',').ToList(),
                CreatedByName = x.CreatedByName,
                TestId = x.TestId ?? Guid.Empty,
            }).ToList();

            return Ok(dto);
        }

        public class ResponseModel
        {
            public object DtoQuestion { get; set; }
            public object DtoAnswer { get; set; }
        }

        [HttpGet("get-by-id-question-answer")]
        public async Task<ActionResult<TestQuestion_AnswersDTO>> GetById(Guid id)
        {
            var data = await _db.TestQuestions.FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return NotFound("Khong co cau hoi nay");
            }

            var dtoquestion = new TestQuestion_AnswersDTO
            {
                Id = data.Id,
                QuestionName = data.QuestionName,
                QuestionType = data.Type,
                Level = data.Level,
                CorrectAnswers = data.RightAnswer.Split(",").ToList(),
                CreatedByName = data.CreatedByName,
                TestId = data.TestId ?? Guid.Empty,
            };

            var answer = await _db.TestQuestionAnswers.FirstOrDefaultAsync(x => x.TestQuestionId == id);

            if (answer == null)
            {
                return NotFound("Loi");
            }

            var dtoanswer = new TestQuestion_AnswersDTO
            {
                Answers = answer.Answer.Split(",").ToList(),
            };

            var list = new ResponseModel
            {
                DtoQuestion = dtoquestion,
                DtoAnswer = dtoanswer,
            };

            return Ok(list);
        }

        [HttpPost("create_question_answwer")]
        public async Task<IActionResult> QuestionWithAnswers(TestQuestion_AnswersDTO dto)
        {
            // Kiểm tra loại câu hỏi và xử lý tương ứng
            switch (dto.QuestionType)
            {
                case 1: // Multiple Choice (Một đáp án đúng)
                    if (!dto.Answers.Contains(dto.CorrectAnswers.FirstOrDefault()))
                    {
                        throw new ArgumentException("Đáp án đúng phải nằm trong danh sách đáp án.");
                    }

                    var newQuestion_type1 = new TestQuestions
                    {
                        Id = Guid.NewGuid(),
                        QuestionName = dto.QuestionName,
                        Type = dto.QuestionType,
                        Level = dto.Level,
                        CreatedByName = dto.CreatedByName,
                        TestId = dto.TestId,
                        RightAnswer = dto.CorrectAnswers.FirstOrDefault() // Lưu đáp án đúng
                    };

                    _db.TestQuestions.Add(newQuestion_type1);

                    var answers = new List<TestQuestionAnswers>();
                    foreach (var answer in dto.Answers)
                    {
                        answers.Add(new TestQuestionAnswers
                        {
                            Id = Guid.NewGuid(),
                            Answer = answer,
                            TestQuestionId = newQuestion_type1.Id
                        });
                    }

                    // Lưu đáp án
                    _db.TestQuestionAnswers.AddRange(answers);
                    await _db.SaveChangesAsync();

                    break;

                case 2: // Multiple Answers (Nhiều đáp án đúng)
                    if (!dto.CorrectAnswers.All(ca => dto.Answers.Contains(ca)))
                    {
                        throw new ArgumentException("Tất cả đáp án đúng phải nằm trong danh sách đáp án.");
                    }

                    var newQuestion_type2 = new TestQuestions
                    {
                        Id = Guid.NewGuid(),
                        QuestionName = dto.QuestionName,
                        Type = dto.QuestionType,
                        Level = dto.Level,
                        CreatedByName = dto.CreatedByName,
                        TestId = dto.TestId,
                        RightAnswer = string.Join(", ", dto.CorrectAnswers) // Lưu tất cả đáp án đúng
                    };

                    // Lưu đáp án
                    _db.TestQuestions.Add(newQuestion_type2);

                    var answers2 = new List<TestQuestionAnswers>();
                    foreach (var answer in dto.Answers)
                    {
                        answers2.Add(new TestQuestionAnswers
                        {
                            Id = Guid.NewGuid(),
                            Answer = answer,
                            TestQuestionId = newQuestion_type2.Id
                        });
                    }

                    // Lưu đáp án
                    _db.TestQuestionAnswers.AddRange(answers2);
                    await _db.SaveChangesAsync();

                    break;

                case 3: // True/False
                    dto.Answers = new List<string> { "True", "False" };
                    if (!dto.CorrectAnswers.Contains("True") && !dto.CorrectAnswers.Contains("False"))
                    {
                        throw new ArgumentException("Đáp án đúng phải là True hoặc False.");
                    }

                    var newQuestion_TrueFalse = new TestQuestions
                    {
                        Id = Guid.NewGuid(),
                        QuestionName = dto.QuestionName,
                        Type = dto.QuestionType,
                        Level = dto.Level,
                        CreatedByName = dto.CreatedByName,
                        TestId = dto.TestId,
                        RightAnswer = dto.CorrectAnswers.FirstOrDefault() // Lưu đáp án đúng
                    };

                    // Lưu đáp án
                    _db.TestQuestions.Add(newQuestion_TrueFalse);

                    var answers3 = new List<TestQuestionAnswers>();
                    foreach (var answer in dto.Answers)
                    {
                        answers3.Add(new TestQuestionAnswers
                        {
                            Id = Guid.NewGuid(),
                            Answer = answer,
                            TestQuestionId = newQuestion_TrueFalse.Id
                        });
                    }

                    // Lưu đáp án
                    _db.TestQuestionAnswers.AddRange(answers3);
                    await _db.SaveChangesAsync();

                    break;

                case 4: // Fill in the Blank
                        // Không cần đáp án cố định
                    var newFillInTheBlankQuestion = new TestQuestions
                    {
                        Id = Guid.NewGuid(),
                        QuestionName = dto.QuestionName,
                        Type = dto.QuestionType,
                        Level = dto.Level,
                        CreatedByName = dto.CreatedByName,
                        TestId = dto.TestId,
                        RightAnswer = string.Join(", ", dto.CorrectAnswers) // Lưu đáp án đúng (nếu có)
                    };

                    _db.TestQuestions.Add(newFillInTheBlankQuestion);

                    await _db.SaveChangesAsync();

                    break;

                default:
                    throw new ArgumentException("Loại câu hỏi không hợp lệ.");
            }


            _db.SaveChanges();

            return Ok("thêm câu hỏi thành công");
        }

        #region làm lại
        [HttpPost("randomize-questions-for-test-codes")]
        public async Task<IActionResult> RandomizeQuestionsForTestCodes(Guid testId, int easyCount, int mediumCount, int hardCount, int veryHardCount)
        {
            // Lấy tất cả các TestCode liên quan đến TestId
            var allTestCodes = await _db.TestCodes
                .Where(tc => tc.TestId == testId)
                .ToListAsync();

            if (allTestCodes == null || allTestCodes.Count == 0)
            {
                return NotFound("Không tìm thấy mã kiểm tra liên quan đến bài thi.");
            }

            // Lấy tất cả các câu hỏi của bài thi và phân loại theo mức độ
            var questions = await _db.TestQuestions.Where(x => x.TestId == testId).ToListAsync();
            var easyQuestions = questions.Where(x => x.Level == 1).ToList();
            var mediumQuestions = questions.Where(x => x.Level == 2).ToList();
            var hardQuestions = questions.Where(x => x.Level == 3).ToList();
            var veryHardQuestions = questions.Where(x => x.Level == 4).ToList();

            // Kiểm tra số lượng câu hỏi đủ cho mỗi mức độ
            if (easyQuestions.Count < easyCount || mediumQuestions.Count < mediumCount ||
                hardQuestions.Count < hardCount || veryHardQuestions.Count < veryHardCount)
            {
                return BadRequest("Không đủ số câu hỏi cho một hoặc nhiều mức độ.");
            }

            // Giới hạn số lần mỗi câu hỏi có thể được sử dụng trong tổng số TestCode
            // Giả sử bạn muốn mỗi câu hỏi được sử dụng tối đa trong 4 TestCode
            int maxUsagePerQuestion = 4;

            // Theo dõi số lần mỗi câu hỏi đã được sử dụng
            var questionUsageCount = new Dictionary<Guid, int>();

            // Khởi tạo dictionary với giá trị 0 cho mỗi câu hỏi
            foreach (var question in questions)
            {
                questionUsageCount[question.Id] = 0;
            }

            Random globalRandom = new Random();

            foreach (var testCode in allTestCodes)
            {
                var selectedQuestionsForTestCode = new List<TestQuestions>();

                // Hàm để chọn câu hỏi cho mỗi mức độ
                List<TestQuestions> SelectQuestions(List<TestQuestions> pool, int count)
                {
                    // Lọc các câu hỏi chưa vượt quá giới hạn sử dụng và chưa được chọn cho TestCode hiện tại
                    var availableQuestions = pool
                        .Where(q => questionUsageCount[q.Id] < maxUsagePerQuestion &&
                                    !selectedQuestionsForTestCode.Any(sq => sq.Id == q.Id))
                        .OrderBy(q => globalRandom.Next())
                        .Take(count)
                        .ToList();

                    if (availableQuestions.Count < count)
                    {
                        throw new InvalidOperationException("Không đủ câu hỏi để phân bổ cho TestCode.");
                    }

                    // Cập nhật số lần sử dụng và thêm vào danh sách đã chọn
                    foreach (var q in availableQuestions)
                    {
                        selectedQuestionsForTestCode.Add(q);
                        questionUsageCount[q.Id]++;
                    }

                    return availableQuestions;
                }

                try
                {
                    // Chọn câu hỏi cho mỗi mức độ
                    SelectQuestions(easyQuestions, easyCount);
                    SelectQuestions(mediumQuestions, mediumCount);
                    SelectQuestions(hardQuestions, hardCount);
                    SelectQuestions(veryHardQuestions, veryHardCount);
                }
                catch (InvalidOperationException ex)
                {
                    return BadRequest($"Lỗi khi phân bổ câu hỏi: {ex.Message}");
                }

                // Shuffle câu hỏi trong TestCode hiện tại
                var shuffledQuestions = selectedQuestionsForTestCode.OrderBy(q => globalRandom.Next()).ToList();

                // Thêm các câu hỏi vào TestCode_TestQuestion
                foreach (var question in shuffledQuestions)
                {
                    var testCodeQuestion = new TestCode_TestQuestion
                    {
                        TestCodeId = testCode.Id,
                        TestQuestionId = question.Id
                    };

                    _db.TestCode_TestQuestion.Add(testCodeQuestion);
                }
            }

            await _db.SaveChangesAsync();

            return Ok("THÀNH CÔNG");
        }
        #endregion

        [HttpPut("update_question_answer")]
        public async Task<IActionResult> UpdateQuestion(TestQuestion_AnswersDTO dto)
        {
            // Tìm câu hỏi theo ID
            var existingQuestion = await _db.TestQuestions.FindAsync(dto.Id);
            if (existingQuestion == null)
            {
                return NotFound("Câu hỏi không tồn tại.");
            }

            // Cập nhật thông tin câu hỏi
            existingQuestion.QuestionName = dto.QuestionName;
            existingQuestion.Level = dto.Level;
            existingQuestion.CreatedByName = dto.CreatedByName;

            // Xử lý theo loại câu hỏi
            switch (dto.QuestionType)
            {
                case 1: // Multiple Choice (Một đáp án đúng)
                    if (!dto.Answers.Contains(dto.CorrectAnswers.FirstOrDefault()))
                    {
                        throw new ArgumentException("Đáp án đúng phải nằm trong danh sách đáp án.");
                    }
                    existingQuestion.RightAnswer = dto.CorrectAnswers.FirstOrDefault();

                    // Cập nhật đáp án
                    var existingAnswersType1 = await _db.TestQuestionAnswers
                        .Where(a => a.TestQuestionId == existingQuestion.Id).ToListAsync();
                    _db.TestQuestionAnswers.RemoveRange(existingAnswersType1);

                    var answersType1 = dto.Answers.Select(answer => new TestQuestionAnswers
                    {
                        Id = Guid.NewGuid(),
                        Answer = answer,
                        TestQuestionId = existingQuestion.Id
                    }).ToList();
                    _db.TestQuestionAnswers.AddRange(answersType1);
                    break;

                case 2: // Multiple Answers (Nhiều đáp án đúng)
                    if (!dto.CorrectAnswers.All(ca => dto.Answers.Contains(ca)))
                    {
                        throw new ArgumentException("Tất cả đáp án đúng phải nằm trong danh sách đáp án.");
                    }
                    existingQuestion.RightAnswer = string.Join(", ", dto.CorrectAnswers);

                    // Cập nhật đáp án
                    var existingAnswersType2 = await _db.TestQuestionAnswers
                        .Where(a => a.TestQuestionId == existingQuestion.Id).ToListAsync();
                    _db.TestQuestionAnswers.RemoveRange(existingAnswersType2);

                    var answersType2 = dto.Answers.Select(answer => new TestQuestionAnswers
                    {
                        Id = Guid.NewGuid(),
                        Answer = answer,
                        TestQuestionId = existingQuestion.Id
                    }).ToList();
                    _db.TestQuestionAnswers.AddRange(answersType2);
                    break;

                case 3: // True/False
                    dto.Answers = new List<string> { "True", "False" };
                    if (!dto.CorrectAnswers.Contains("True") && !dto.CorrectAnswers.Contains("False"))
                    {
                        throw new ArgumentException("Đáp án đúng phải là True hoặc False.");
                    }
                    existingQuestion.RightAnswer = dto.CorrectAnswers.FirstOrDefault();

                    // Cập nhật đáp án
                    var existingAnswersType3 = await _db.TestQuestionAnswers
                        .Where(a => a.TestQuestionId == existingQuestion.Id).ToListAsync();
                    _db.TestQuestionAnswers.RemoveRange(existingAnswersType3);

                    var answersType3 = dto.Answers.Select(answer => new TestQuestionAnswers
                    {
                        Id = Guid.NewGuid(),
                        Answer = answer,
                        TestQuestionId = existingQuestion.Id
                    }).ToList();
                    _db.TestQuestionAnswers.AddRange(answersType3);
                    break;

                case 4: // Fill in the Blank
                    existingQuestion.RightAnswer = string.Join(", ", dto.CorrectAnswers); // Cập nhật đáp án đúng (nếu có)
                    break;

                default:
                    throw new ArgumentException("Loại câu hỏi không hợp lệ.");
            }

            // Lưu thay đổi vào cơ sở dữ liệu
            await _db.SaveChangesAsync();

            return Ok("Cập nhật câu hỏi thành công");
        }

        [HttpGet("export-template")]
        public IActionResult ExportExcelTemplate()
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Câu hỏi");

                var headers = new List<string>
                {
                    "STT", "Kiểu câu hỏi", "Nội dung câu hỏi", "Mức độ tư duy",
                    "Đáp án đúng", "Câu A", "Câu B", "Câu C", "Câu D", "Câu E", "Câu F"
                };

                // Ghi tiêu đề vào hàng đầu tiên
                for (int i = 0; i < headers.Count; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                }

                // Thiết lập độ rộng cho các cột
                worksheet.Column(1).Width = 5;   // STT
                worksheet.Column(2).Width = 20;  // Kiểu câu hỏi
                worksheet.Column(3).Width = 30;  // Nội dung câu hỏi
                worksheet.Column(4).Width = 15;  // Mức độ tư duy
                worksheet.Column(5).Width = 15;  // Đáp án đúng
                worksheet.Column(6).Width = 10;  // Câu A
                worksheet.Column(7).Width = 10;  // Câu B
                worksheet.Column(8).Width = 10;  // Câu C
                worksheet.Column(9).Width = 10;  // Câu D
                worksheet.Column(10).Width = 10; // Câu E
                worksheet.Column(11).Width = 10; // Câu F
                using (var headerRange = worksheet.Cells[1, 1, 1, headers.Count])
                {
                    headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.RoyalBlue);
                    headerRange.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    headerRange.Style.Font.Bold = true;
                }


                var questionTypeList = new List<string> { "Trắc nghiệm 1 Đáp án", "Trắc nghiệm nhiều đáp án", "Đúng/sai", "Điền vào chỗ trống" };
                CreateDropdownList(worksheet, questionTypeList, 2, 2, 100, 2); // Áp dụng cho cột 2 (Kiểu câu hỏi)

                // Tạo dropdown list cho "Mức độ tư duy"
                var thinkingLevelList = new List<string> { "Dễ", "Trung bình", "Khó", "Rất khó" };
                CreateDropdownList(worksheet, thinkingLevelList, 2, 4, 100, 4); // Áp dụng cho cột 4 (Mức độ tư duy)

                // Lưu file vào MemoryStream
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template_Cauhoi.xlsx");
            }
        }

        private void CreateDropdownList(ExcelWorksheet worksheet, List<string> options, int fromRow, int fromCol, int toRow, int toCol)
        {
            // Tạo một List Validation trong khoảng ô được chỉ định
            var validation = worksheet.DataValidations.AddListValidation(worksheet.Cells[fromRow, fromCol, toRow, toCol].Address);

            // Thêm các giá trị từ danh sách 'options' vào dropdown list
            foreach (var option in options)
            {
                validation.Formula.Values.Add(option);
            }

            // Cấu hình thêm cho dropdown list
            validation.ShowErrorMessage = true;
            validation.ErrorTitle = "Giá trị không hợp lệ"; // Tiêu đề thông báo lỗi
            validation.Error = "Vui lòng chọn một giá trị từ danh sách."; // Nội dung thông báo lỗi
        }



        [HttpPost("import_questions")]
        public async Task<IActionResult> ImportQuestionsFromExcel(IFormFile file, Guid id)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Chọn một file Excel hợp lệ.");
            }

            var testCode = await _db.Tests.FirstOrDefaultAsync(c => c.Id == id);
            var questionsList = new List<TestQuestion_AnswersDTO>();
            var errorMessages = new List<string>();
            var uniqueQuestions = new HashSet<string>();

            // Lấy danh sách câu hỏi và đáp án từ cơ sở dữ liệu
            var existingQuestionsAndAnswers = new HashSet<string>(
                await _db.TestQuestions
                    .Where(q => q.TestId == id)
                    .Include(q => q.TestQuestionAnswer)
                    .Select(q => q.QuestionName + "|" + string.Join(",", q.TestQuestionAnswer.Select(a => a.Answer).OrderBy(a => a)))
                    .ToListAsync()
            );

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        string typeText = worksheet.Cells[row, 2].Text.Trim();
                        string levelText = worksheet.Cells[row, 4].Text.Trim();
                        string questionName = worksheet.Cells[row, 3].Text;

                        int convertype(string typetext)
                        {
                            switch (typetext.ToLower())
                            {
                                case "trắc nghiệm 1 đáp án": return 1;
                                case "trắc nghiệm nhiều đáp án": return 2;
                                case "đúng/sai": return 3;
                                case "điền vào chỗ trống": return 4;
                                default:
                                    errorMessages.Add($"Giá trị type không hợp lệ ở hàng {row}: {typetext}");
                                    return -1;
                            }
                        }

                        int ConvertLevel(string levelText)
                        {
                            switch (levelText.ToLower())
                            {
                                case "dễ": return 1;
                                case "trung bình": return 2;
                                case "khó": return 3;
                                case "rất khó": return 4;
                                default:
                                    errorMessages.Add($"Giá trị Level không hợp lệ ở hàng {row}: {levelText}");
                                    return -1;
                            }
                        }

                        int questionType = convertype(typeText);
                        int level = ConvertLevel(levelText);

                        if (questionType == -1 || level == -1)
                        {
                            continue;
                        }

                        // Lấy danh sách câu trả lời từ file Excel
                        var answers = new List<string>
                {
                    worksheet.Cells[row, 6].Text,
                    worksheet.Cells[row, 7].Text,
                    worksheet.Cells[row, 8].Text,
                    worksheet.Cells[row, 9].Text,
                    worksheet.Cells[row, 10].Text,
                    worksheet.Cells[row, 11].Text
                }.Where(x => !string.IsNullOrEmpty(x)).OrderBy(a => a).ToList();

                        // Tạo chuỗi để kiểm tra trùng lặp
                        string questionAndAnswers = questionName + "|" + string.Join(",", answers);

                        // Kiểm tra trùng lặp
                        if (existingQuestionsAndAnswers.Contains(questionAndAnswers) || !uniqueQuestions.Add(questionAndAnswers))
                        {
                            errorMessages.Add($"Câu hỏi '{questionName}' với các câu trả lời tương ứng đã tồn tại.");
                            continue;
                        }

                        var username = User.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
                        var dto = new TestQuestion_AnswersDTO
                        {
                            QuestionType = questionType,
                            QuestionName = questionName,
                            Level = level,
                            CreatedByName = "",
                            TestId = testCode.Id,
                            CorrectAnswers = worksheet.Cells[row, 5].GetValue<string>()
                                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(c => c.Trim())
                                .Where(c => !string.IsNullOrWhiteSpace(c))
                                .ToList(),
                            Answers = answers
                        };

                        questionsList.Add(dto);
                    }
                }
            }

            if (errorMessages.Count > 0)
            {
                return BadRequest(string.Join("\n", errorMessages));
            }

            foreach (var dto in questionsList)
            {
                await QuestionWithAnswers(dto);
            }

            return Ok("Nhập câu hỏi thành công từ file Excel.");
        }
        [HttpDelete("Delete_TestQuestion")]
        public async Task<ActionResult> Delete_question(Guid Id)
        {
            var delete = _db.TestQuestions.FirstOrDefault(temp => temp.Id == Id);
            if (delete != null)
            {
                _db.TestQuestions.Remove(delete);
                await _db.SaveChangesAsync();
                return Ok("Xóa thành công");
            }
            return BadRequest("Xóa thất bại");
        }
    }
}
