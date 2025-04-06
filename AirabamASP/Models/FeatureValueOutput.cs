using System.ComponentModel.DataAnnotations;

namespace AirabamASP.Models
{
    public class FeatureValueOutput
    {
        [Key]
        public int Id { get; set; }
        public string Feature { get; set; }
        public string Value { get; set; }
        public float Output { get; set; }
    }
}
