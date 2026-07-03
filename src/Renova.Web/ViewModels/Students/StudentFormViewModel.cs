using Microsoft.AspNetCore.Http;
using Renova.Domain.Entities;

namespace Renova.Web.ViewModels.Students;

public class StudentFormViewModel
{
    public Student Student { get; set; } = new();

    public IFormFile? Photo { get; set; }
}