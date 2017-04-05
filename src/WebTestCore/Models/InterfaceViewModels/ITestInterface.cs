using System.ComponentModel.DataAnnotations;

namespace WebTestCore.Models
{
    public interface ITestInterface
    {
        [Range(1, 10)]
        int TestProperty { get; set; }
    }
}
