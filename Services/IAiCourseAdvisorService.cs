using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects;

namespace Services
{
    public interface IAiCourseAdvisorService
    {
        Task<AiRecommendationResponseDto> RecommendForStudentAsync(int studentId, string prompt);
    }
}