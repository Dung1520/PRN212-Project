using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public class AiRecommendationResponseDto
    {
        public string Summary { get; set; } = string.Empty;
        public List<AiRecommendationItemDto> Items { get; set; } = new();
    }
}