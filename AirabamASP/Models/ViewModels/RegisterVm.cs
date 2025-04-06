using System.ComponentModel.DataAnnotations;

namespace AirabamASP.Models
{
    public class RegisterVm : LoginVm
    {
        [Required, Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}