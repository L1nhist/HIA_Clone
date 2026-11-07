
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
namespace HIA.Core.Models.FileUploads;

public class FileUpload
{
    [Required]
    [Display(Name = "File")]
    public List<IFormFile> FormFiles { get; set; } = new(); // convert to list
    public string? SuccessMessage { get; set; }
}
