using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects;

namespace Services
{
    public class AiCourseAdvisorService : IAiCourseAdvisorService
    {
        private readonly IStudentCourseService _studentCourseService;
        private readonly IRecommendationProvider _provider;

        public AiCourseAdvisorService(
            IStudentCourseService studentCourseService,
            IRecommendationProvider provider)
        {
            _studentCourseService = studentCourseService;
            _provider = provider;
        }

        public async Task<AiRecommendationResponseDto> RecommendForStudentAsync(int studentId, string prompt)
        {
            var pref = StudentPreferenceParser.Parse(prompt);

            var courses = _studentCourseService.GetCourses(null, "Open");
            var candidates = new List<AiRecommendationCandidateDto>();

            foreach (var course in courses)
            {
                if (_studentCourseService.IsStudentAlreadyEnrolledInCourse(studentId, course.Id))
                    continue;

                var classes = _studentCourseService.GetClassesByCourseId(course.Id, studentId);

                foreach (var cls in classes)
                {
                    if (cls.CurrentEnrollment >= cls.Capacity)
                        continue;

                    if (pref.MinFee.HasValue && course.Fee < pref.MinFee.Value)
                        continue;

                    if (pref.MaxFee.HasValue && course.Fee > pref.MaxFee.Value)
                        continue;

                    if (!StudentPreferenceParser.MatchesPreferredSession(cls.Slot, pref))
                        continue;

                    if (!StudentPreferenceParser.MatchesLevel(course.CourseName, pref))
                        continue;

                    candidates.Add(new AiRecommendationCandidateDto
                    {
                        CandidateId = cls.Id,
                        CourseId = course.Id,
                        CourseCode = course.CourseCode,
                        CourseName = course.CourseName,
                        Category = course.Category,
                        DurationWeeks = course.DurationWeeks,
                        Fee = course.Fee,
                        ClassId = cls.Id,
                        ClassCode = cls.ClassCode,
                        StartDate = cls.StartDate,
                        EndDate = cls.EndDate,
                        DayOfWeek = cls.DayOfWeek,
                        Slot = cls.Slot,
                        Capacity = cls.Capacity,
                        CurrentEnrollment = cls.CurrentEnrollment
                    });
                }
            }

            if (!candidates.Any())
            {
                return new AiRecommendationResponseDto
                {
                    Summary = "Không tìm thấy lớp nào phù hợp với điều kiện hiện tại.",
                    Items = new List<AiRecommendationItemDto>()
                };
            }

            var shortlisted = candidates
                .Select(x => new
                {
                    Candidate = x,
                    Score = CalculateScore(x, pref)
                })
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Candidate.Fee)
                .ThenBy(x => x.Candidate.StartDate)
                .Take(10)
                .Select(x => x.Candidate)
                .ToList();

            try
            {
                var aiResult = await _provider.RecommendAsync(pref, shortlisted);

                var validIds = shortlisted.Select(x => x.CandidateId).ToHashSet();
                aiResult.Items = aiResult.Items
                    .Where(x => validIds.Contains(x.CandidateId))
                    .OrderByDescending(x => x.Score)
                    .Take(3)
                    .ToList();

                if (!aiResult.Items.Any())
                    return BuildFallback(shortlisted, pref);

                return aiResult;
            }
            catch (Exception ex)
            {
                var fallback = BuildFallback(shortlisted, pref);
                fallback.Summary = "AI lỗi, đang fallback. Chi tiết: " + ex.Message;
                return fallback;
            }
        }

        private static AiRecommendationResponseDto BuildFallback(
            List<AiRecommendationCandidateDto> candidates,
            StudentPreferenceDto pref)
        {
            var ranked = candidates
                .Select(x => new
                {
                    Candidate = x,
                    Score = CalculateScore(x, pref)
                })
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Candidate.Fee)
                .ThenBy(x => x.Candidate.StartDate)
                .Take(3)
                .ToList();

            return new AiRecommendationResponseDto
            {
                Summary = "Đây là gợi ý tự động dựa trên điều kiện hiện có.",
                Items = ranked.Select(x => new AiRecommendationItemDto
                {
                    CandidateId = x.Candidate.CandidateId,
                    Score = x.Score,
                    Reason = "Phù hợp với điều kiện học phí, lịch học và đang còn chỗ."
                }).ToList()
            };
        }

        private static double CalculateScore(AiRecommendationCandidateDto c, StudentPreferenceDto pref)
        {
            double score = 0.0;

            var feeMatched = true;

            if (pref.MinFee.HasValue && c.Fee < pref.MinFee.Value)
                feeMatched = false;

            if (pref.MaxFee.HasValue && c.Fee > pref.MaxFee.Value)
                feeMatched = false;

            if (feeMatched)
                score += 0.4;

            if (StudentPreferenceParser.MatchesPreferredSession(c.Slot, pref))
                score += 0.3;

            if (StudentPreferenceParser.MatchesLevel(c.CourseName, pref))
                score += 0.2;

            if (c.SeatsLeft > 0)
                score += 0.1;

            return score;
        }
    }
}